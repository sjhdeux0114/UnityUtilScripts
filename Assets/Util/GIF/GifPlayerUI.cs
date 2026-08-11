// GifPlayerUI.cs (stutter-free lazy-sprite version)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GifPlayerUI : MonoBehaviour
{
    public enum SourceType { Resources, StreamingAssets, Bytes }

    [Header("Source")]
    [Tooltip("Resources path (no extension) or StreamingAssets relative path")]
    public string path;
    [Tooltip("Provide bytes directly if sourceType = Bytes")]
    public byte[] bytes;
    [Tooltip("Where to load GIF data from")]
    public SourceType sourceType = SourceType.Resources;

    [Header("Target")]
    [Tooltip("Use RawImage (recommended to reduce Sprite overhead). If false => Image")]
    public bool useRawImage = false; // false => Image
    [Tooltip("Sprite.Create() pixelsPerUnit when using Image")]
    public int pixelsPerUnit = 100;
    [Tooltip("Call StartPlay on OnEnable when data not loaded yet")]
    public bool playOnAwake = true;
    [Tooltip("Match the texture/sprite pixel size to UI rect")]
    public bool bNativeSize = false;

    [Header("Playback")]
    [Tooltip("Use unscaled time (ignore Time.timeScale)")]
    public bool unscaledTime = false;
    [Tooltip("-1: use GIF loopCount, 0: infinite, >0: times")]
    public int overrideLoopCount = -1; // -1: use GIF loopCount, 0: infinite
    [Tooltip("Global playback speed multiplier")]
    [Range(0.05f, 5f)] public float PlayeSpeed = 1.0f;
    [Tooltip("Force additional loop even if GIF loopCount is finite")]
    public bool bLoop = true;

    [Header("Stutter Reduction")]
    [Tooltip("Avoid prebuilding all Sprites; cache only what's needed (recommended)")]
    public bool lazySprite = true;
    [Tooltip("Max number of Sprites to keep in cache (LRU). 0 disables caching.")]
    [Range(0, 64)] public int spriteCacheSize = 8;
    [Tooltip("Warm up sprite cache gradually in background after first frame shows")]
    public bool warmupSpritesInBackground = true;
    [Tooltip("How many sprites to create per warmup step")]
    [Range(1, 32)] public int warmupBatch = 4;
    [Tooltip("Minimum frame delay to avoid ultra-fast yielding (sec)")]
    [Range(0.001f, 0.1f)] public float minFrameDelay = 0.002f;

    // components
    private Image _img;
    private RawImage _raw;

    // decoded data
    private Texture2D[] _frames;
    private float[] _delays;
    private int _gifLoopCount; // 0 = infinite from file

    // state
    private Coroutine _co;
    private Coroutine _warmupCo;
    private int _frameIndex = 0;
    private int _loopsLeft = 0;  // 0 = infinite

    // sprite cache for Image mode (LRU)
    private readonly Dictionary<int, Sprite> _spriteCache = new Dictionary<int, Sprite>();
    private readonly LinkedList<int> _lru = new LinkedList<int>(); // most-recent at First

    void Awake()
    {
        if (useRawImage) _raw = GetComponent<RawImage>();
        else _img = GetComponent<Image>();
    }

    void OnEnable()
    {
        if (_frames != null && _frames.Length > 0)
        {
            ResumePlay();
        }
        else if (playOnAwake)
        {
            StartPlay();
        }
    }

    void OnDisable()
    {
        PausePlay(); // keep data
    }

    void OnDestroy()
    {
        PausePlay();
        StopWarmup();
        UnloadAll();
    }

    // ---- Public API -------------------------------------------------------

    /// <summary>If loaded, resume; else load + play.</summary>
    public void StartPlay()
    {
        if (_co != null) return;

        if (_frames != null && _frames.Length > 0)
        {
            ResumePlay();
            return;
        }

        _co = StartCoroutine(LoadAndPlay());
    }

    /// <summary>Stop only the play coroutine (keep decoded data).</summary>
    public void PausePlay()
    {
        if (_co != null) StopCoroutine(_co);
        _co = null;
    }

    /// <summary>Resume from current frame index.</summary>
    public void ResumePlay()
    {
        if ((_frames?.Length ?? 0) == 0) return;
        if (_co != null) return;

        ApplyFrameImmediate(_frameIndex);
        _co = StartCoroutine(PlayRoutine());

        // optional background warmup (Image-only, lazy mode)
        if (!useRawImage && lazySprite && warmupSpritesInBackground)
        {
            StartWarmup();
        }
    }

    /// <summary>Fully release textures/sprites/refs.</summary>
    public void UnloadAll()
    {
        if (_raw) _raw.texture = null;
        if (_img) _img.sprite = null;

        ClearSpriteCache();

        if (_frames != null)
        {
            foreach (var t in _frames) if (t) Destroy(t);
        }

        _frames = null;
        _delays = null;
        _gifLoopCount = 0;
        _frameIndex = 0;
        _loopsLeft = 0;
    }

    // ---- Internal ---------------------------------------------------------

    private IEnumerator LoadAndPlay()
    {
        // 1) async-like I/O (StreamingAssets) or normal load
        byte[] data = null;
        if (sourceType == SourceType.Resources)
        {
            // Resources는 동기지만 보통 크기가 아주 크지 않으면 빠릅니다.
            data = GifLoader.LoadFromResources(path);
        }
        else if (sourceType == SourceType.StreamingAssets)
        {
            bool done = false;
            yield return GifLoader.LoadFromStreamingAssets(path, b => { data = b; done = true; });
            if (!done) { _co = null; yield break; }
        }
        else
        {
            data = bytes;
        }

        if (data == null || data.Length == 0)
        {
            Debug.LogError($"GIF load failed: {path}");
            _co = null;
            yield break;
        }

        // 2) decode (이미 Texture2D[]을 반환하는 기존 디코더를 사용)
        //    *디코더가 무거운 경우엔 첫 프레임 우선 디코드/표시가 가능한 API를 제공하면 더 좋습니다.
        GifFrames gf = null;
        try { gf = GifDecoder.Decode(data); }
        catch (System.Exception e) { Debug.LogException(e); _co = null; yield break; }

        _frames = gf.textures;
        _delays = gf.delays;
        _gifLoopCount = gf.loopCount; // 0 = infinite

        _frameIndex = 0;
        _loopsLeft = (overrideLoopCount >= 0) ? overrideLoopCount : _gifLoopCount; // 0=infinite

        // 3) 첫 프레임 즉시 반영 후 재생 시작
        ApplyFrameImmediate(_frameIndex);
        _co = StartCoroutine(PlayRoutine());

        // 4) (선택) 백그라운드 워밍 (Image + lazySprite)
        if (!useRawImage && lazySprite && warmupSpritesInBackground)
        {
            StartWarmup();
        }
    }

    private void ApplyFrameImmediate(int idx)
    {
        idx = Mathf.Clamp(idx, 0, (_frames?.Length ?? 1) - 1);

        if (useRawImage)
        {
            if (_raw) _raw.texture = _frames[idx];
            if (bNativeSize) _raw?.SetNativeSize();
        }
        else
        {
            if (_img)
            {
                _img.sprite = GetSpriteForFrame(idx);
                if (bNativeSize) _img.SetNativeSize();
            }
        }
    }

    private IEnumerator PlayRoutine()
    {
        if ((_frames?.Length ?? 0) == 0) { _co = null; yield break; }

        while (true)
        {
            int i = Mathf.Clamp(_frameIndex, 0, _frames.Length - 1);

            // draw
            if (useRawImage)
            {
                if (_raw) _raw.texture = _frames[i];
                if (bNativeSize) _raw?.SetNativeSize();
            }
            else
            {
                if (_img) _img.sprite = GetSpriteForFrame(i);
                if (bNativeSize) _img?.SetNativeSize();
            }

            // wait
            float dt = (i < _delays.Length) ? Mathf.Max(minFrameDelay, _delays[i]) : 0.1f;
            if (unscaledTime) yield return new WaitForSecondsRealtime(dt / Mathf.Max(0.001f, PlayeSpeed));
            else yield return new WaitForSeconds(dt / Mathf.Max(0.001f, PlayeSpeed));

            // next
            _frameIndex++;

            if (_frameIndex >= _frames.Length)
            {
                if (_loopsLeft > 0) _loopsLeft--;

                if (_loopsLeft <= 0 && !bLoop)
                    break; // finish

                _frameIndex = 0; // loop
            }
        }

        _co = null;
    }

    // ---- Sprite cache (LRU) ----------------------------------------------

    private Sprite GetSpriteForFrame(int frameIndex)
    {
        if (useRawImage) return null; // not used

        // lazy mode: create on demand
        if (lazySprite)
        {
            if (_spriteCache.TryGetValue(frameIndex, out var spr) && spr)
            {
                TouchLRU(frameIndex);
                return spr;
            }

            // create new
            var tex = _frames[frameIndex];
            var created = Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit, 0, SpriteMeshType.FullRect);

            if (spriteCacheSize > 0)
            {
                _spriteCache[frameIndex] = created;
                TouchLRU(frameIndex);
                TrimLRU(); // evict if needed
            }

            return created;
        }
        else
        {
            // non-lazy: behave like old BuildSprites but without upfront cost
            // still cache, but we could also prebuild all with a separate call if desired.
            if (_spriteCache.TryGetValue(frameIndex, out var spr) && spr) return spr;

            var tex = _frames[frameIndex];
            var created = Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit, 0, SpriteMeshType.FullRect);

            if (spriteCacheSize > 0)
            {
                _spriteCache[frameIndex] = created;
                TouchLRU(frameIndex);
                TrimLRU();
            }
            return created;
        }
    }

    private void TouchLRU(int key)
    {
        // move key to front (most recent)
        var node = _lru.Find(key);
        if (node != null) _lru.Remove(node);
        _lru.AddFirst(key);
    }

    private void TrimLRU()
    {
        while (spriteCacheSize > 0 && _lru.Count > spriteCacheSize)
        {
            int victim = _lru.Last.Value;
            _lru.RemoveLast();

            if (_spriteCache.TryGetValue(victim, out var s) && s)
            {
                _spriteCache.Remove(victim);
                Destroy(s);
            }
            else
            {
                _spriteCache.Remove(victim);
            }
        }
    }

    private void ClearSpriteCache()
    {
        foreach (var kv in _spriteCache)
        {
            if (kv.Value) Destroy(kv.Value);
        }
        _spriteCache.Clear();
        _lru.Clear();
    }

    // ---- Background warmup ------------------------------------------------

    private void StartWarmup()
    {
        StopWarmup();
        _warmupCo = StartCoroutine(WarmupSpritesGradually());
    }

    private void StopWarmup()
    {
        if (_warmupCo != null) StopCoroutine(_warmupCo);
        _warmupCo = null;
    }

    /// <summary>
    /// Create a few sprites per step so that, after the first frames are already playing,
    /// the cache fills without a single huge spike.
    /// </summary>
    private IEnumerator WarmupSpritesGradually()
    {
        if (useRawImage || !lazySprite || _frames == null) { _warmupCo = null; yield break; }

        // fill around current index first (better for perceived smoothness)
        int total = _frames.Length;
        int center = Mathf.Clamp(_frameIndex, 0, total - 1);
        int radius = 0;

        while (true)
        {
            int createdThisStep = 0;

            // expand ring around current frame
            for (int k = 0; k < warmupBatch; k++)
            {
                int left = center - radius;
                int right = center + radius;

                bool created = false;

                if (left >= 0 && !_spriteCache.ContainsKey(left))
                {
                    // create (will auto-trim)
                    var _ = GetSpriteForFrame(left);
                    created = true;
                }

                if (createdThisStep + 1 >= warmupBatch) break;

                if (right < total && right != left && !_spriteCache.ContainsKey(right))
                {
                    var _ = GetSpriteForFrame(right);
                    created = true;
                }

                if (created) createdThisStep++;

                // prepare next radius increment only after trying both sides
                radius++;
                if (radius > total) break;
            }

            if (createdThisStep == 0) break; // done

            // yield so we don't spike a frame
            // (EndOfFrame나 WaitForSecondsRealtime(0)로도 충분)
            yield return null;
        }

        _warmupCo = null;
    }
}
