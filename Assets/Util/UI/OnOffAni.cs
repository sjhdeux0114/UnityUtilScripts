using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OnOffAni : MonoBehaviour
{
    public enum AniType
    {
        FADE,
        SCALE,
        FADE_SCALE,
        SLIDE,
        ROTATE,
        FILL
    }

    [Header("Target")]
    [SerializeField] private Image targetImage;

    [Header("Animation Settings")]
    public AniType aniType = AniType.FADE;
    public bool value = false;
    [Min(0f)]
    public float AniTime = 0.3f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private bool useUnscaledTime = false;
    [SerializeField] private bool disableRaycastWhenOff = true;

    [Header("Type Options")]
    [Tooltip("SCALE / FADE_SCALE 에서 Off 상태일 때의 스케일")]
    [SerializeField] private Vector3 offScale = Vector3.zero;
    [Tooltip("SLIDE 에서 Off 상태일 때의 위치 오프셋")]
    [SerializeField] private Vector2 slideOffset = new Vector2(0f, -100f);
    [Tooltip("ROTATE 에서 Off 상태일 때의 회전 오프셋 (Euler)")]
    [SerializeField] private Vector3 rotateOffset = new Vector3(0f, 90f, 0f);
    [Tooltip("SLIDE / ROTATE 애니메이션 시 알파 페이드도 함께 적용")]
    [SerializeField] private bool withFade = true;

    [Header("Events")]
    public UnityEvent<bool> onComplete;

    private RectTransform _targetRect;
    private Color _initialColor;
    private float _initialAlpha = 1f;
    private Vector3 _initialScale = Vector3.one;
    private Vector2 _initialAnchoredPos;
    private Vector3 _initialEulerAngles;
    private float _initialFillAmount = 1f;
    private bool _initialRaycastTarget;

    private bool _initialized;
    private bool _currentTargetValue;
    private bool _isAnimating;
    private float _progress;

    public bool Value
    {
        get => value;
        set => SetValue(value);
    }

    public bool IsAnimating => _isAnimating;

    private void Reset()
    {
        targetImage = GetComponent<Image>();
    }

    private void Awake()
    {
        EnsureInitialized();
    }

    private void OnValidate()
    {
        if (AniTime < 0f)
            AniTime = 0f;

        if (!Application.isPlaying || !_initialized)
            return;

        if (value != _currentTargetValue)
            StartTransition(value);
    }

    private void Update()
    {
        EnsureInitialized();

        if (value != _currentTargetValue)
            StartTransition(value);

        if (!_isAnimating)
            return;

        float targetProgress = _currentTargetValue ? 1f : 0f;
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        if (AniTime <= 0f)
        {
            _progress = targetProgress;
        }
        else
        {
            _progress = Mathf.MoveTowards(_progress, targetProgress, dt / AniTime);
        }

        ApplyAnimation(_progress);

        if (Mathf.Approximately(_progress, targetProgress))
        {
            _progress = targetProgress;
            _isAnimating = false;
            ApplyAnimation(_progress);
            onComplete?.Invoke(_currentTargetValue);
        }
    }

    public void SetValue(bool newValue, bool immediate = false)
    {
        EnsureInitialized();
        value = newValue;

        if (immediate || AniTime <= 0f)
        {
            _currentTargetValue = newValue;
            _isAnimating = false;
            _progress = newValue ? 1f : 0f;
            ApplyAnimation(_progress);
            onComplete?.Invoke(_currentTargetValue);
            return;
        }

        float targetProgress = newValue ? 1f : 0f;
        if (_currentTargetValue != newValue || (!_isAnimating && !Mathf.Approximately(_progress, targetProgress)))
        {
            StartTransition(newValue);
        }
    }

    public void Toggle()
    {
        SetValue(!value);
    }

    private void EnsureInitialized()
    {
        if (_initialized)
            return;

        if (targetImage == null)
            targetImage = GetComponent<Image>();

        _targetRect = targetImage != null ? targetImage.rectTransform : GetComponent<RectTransform>();

        if (targetImage != null)
        {
            _initialColor = targetImage.color;
            _initialAlpha = _initialColor.a > 0.001f ? _initialColor.a : 1f;
            _initialFillAmount = targetImage.fillAmount > 0.001f ? targetImage.fillAmount : 1f;
            _initialRaycastTarget = targetImage.raycastTarget;

            if (aniType == AniType.FILL && targetImage.type != Image.Type.Filled)
            {
                Debug.LogWarning("[OnOffAni] FILL 타입은 Image.Type이 Filled로 설정되어 있어야 정상 동작합니다.", this);
            }
        }

        if (_targetRect != null)
        {
            _initialScale = _targetRect.localScale.sqrMagnitude > 0.0001f ? _targetRect.localScale : Vector3.one;
            _initialAnchoredPos = _targetRect.anchoredPosition;
            _initialEulerAngles = _targetRect.localEulerAngles;
        }

        _initialized = true;
        _currentTargetValue = value;
        _progress = value ? 1f : 0f;
        _isAnimating = false;
        ApplyAnimation(_progress);
    }

    private void StartTransition(bool targetValue)
    {
        _currentTargetValue = targetValue;
        float targetProgress = targetValue ? 1f : 0f;

        if (AniTime <= 0f)
        {
            _progress = targetProgress;
            _isAnimating = false;
            ApplyAnimation(_progress);
            onComplete?.Invoke(_currentTargetValue);
        }
        else
        {
            _isAnimating = !Mathf.Approximately(_progress, targetProgress);
        }
    }

    private void ApplyAnimation(float progress)
    {
        float curved = curve != null ? curve.Evaluate(progress) : progress;

        switch (aniType)
        {
            case AniType.FADE:
                ApplyAlpha(curved);
                break;

            case AniType.SCALE:
                ApplyScale(curved);
                break;

            case AniType.FADE_SCALE:
                ApplyAlpha(curved);
                ApplyScale(curved);
                break;

            case AniType.SLIDE:
                if (_targetRect != null)
                    _targetRect.anchoredPosition = Vector2.LerpUnclamped(_initialAnchoredPos + slideOffset, _initialAnchoredPos, curved);
                if (withFade)
                    ApplyAlpha(curved);
                break;

            case AniType.ROTATE:
                if (_targetRect != null)
                    _targetRect.localRotation = Quaternion.Euler(Vector3.LerpUnclamped(_initialEulerAngles + rotateOffset, _initialEulerAngles, curved));
                if (withFade)
                    ApplyAlpha(curved);
                break;

            case AniType.FILL:
                if (targetImage != null)
                    targetImage.fillAmount = Mathf.Clamp01(Mathf.Lerp(0f, _initialFillAmount, curved));
                break;
        }

        if (targetImage != null && disableRaycastWhenOff && _initialRaycastTarget)
        {
            targetImage.raycastTarget = progress > 0.001f;
        }
    }

    private void ApplyAlpha(float curved)
    {
        if (targetImage == null)
            return;

        Color c = _initialColor;
        c.a = Mathf.Clamp01(Mathf.Lerp(0f, _initialAlpha, curved));
        targetImage.color = c;
    }

    private void ApplyScale(float curved)
    {
        if (_targetRect == null)
            return;

        _targetRect.localScale = Vector3.LerpUnclamped(offScale, _initialScale, curved);
    }
}
