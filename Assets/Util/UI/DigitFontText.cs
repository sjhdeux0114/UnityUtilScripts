using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public enum FontAlginment
{
    Left,
    Center,
    Right
}

[ExecuteAlways] // ← 에디터에서도 실행되도록 설정
public class DigitFontText : MonoBehaviour
{
    public DigitFontAsset fontAsset;
    public bool UseCount = false;
    public bool IsCount;
    public int value;

    private float displayedValue;
    private float animStartTime;
    [SerializeField]
    private float animDuration = 2.5f;
    private float animStartValue = 1000;
    private int lastDisplayedInt = -999999;

    public int Value
    {
        get { return value; }
        set
        {
            if (this.value == value) return;
            this.value = value;

            if (!UseCount || !Application.isPlaying)
            {
                displayedValue = value;
                Refresh();
            }
        }
    }
    public float FontSize = 32f;
    [Range(0.0f, 1.0f)]
    public float spacingPer = 0.8f;
    public float spacing
    {
        get { return FontSize * spacingPer; }
        set { spacingPer = value / FontSize; }
    }
    public float commaSpacing = 16f; // ← 콤마 간격 추가
    public bool zeroPadding = false;
    public int padDigits = 0;
    public bool useComma = false; // ← 3자리 쉼표 옵션 추가
    public Material material;
    public FontAlginment Algin = FontAlginment.Left;
    private readonly List<Image> digits = new List<Image>();
    public Sprite addFirstImg;
    public Vector2 vAddSize;


    [ContextMenu("Clear")]
    public void Clear()
    {
        int cnt = transform.childCount;
        for (int i = 0; i < cnt; i++)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        digits.Clear();
    }

    private void OnEnable()
    {

    }

    public void SetValue(int newValue, bool immediate = false)
    {
        if (immediate)
        {
            this.value = newValue;
            displayedValue = newValue;
            lastDisplayedInt = Mathf.RoundToInt(displayedValue);
            old_Val = newValue;

            if (!Application.isPlaying)
            {
                Clear();
            }
            Refresh();
        }
        else
        {
            Value = newValue;
        }
    }

    public void AddValue(int delta, bool immediate = false)
    {
        SetValue(value + delta, immediate);
    }

    public static implicit operator int(DigitFontText d)
    {
        return d != null ? d.Value : 0;
    }



    void Refresh()
    {
        if (fontAsset == null)
        {
            Clear();
            return;
        }

        int valToDisplay = Mathf.RoundToInt(displayedValue);
        string str = zeroPadding ? valToDisplay.ToString($"D{padDigits}") : valToDisplay.ToString();

        if (useComma)
        {
            // 쉼표 추가 (문화권 고정)
            // displayedValue가 float이므로 반올림된 정수를 사용합니다.
            str = valToDisplay.ToString("N0", CultureInfo.InvariantCulture);
        }

        if (addFirstImg != null)
        {
            str = string.Format("!{0}", str);
        }

        EnsureImageCount(str.Length);

        float[] charWidths = new float[str.Length];
        float TotalSize = 0f;

        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            float charSpacing = spacing;

            if (c == '!')
                charSpacing = vAddSize.x;
            else if (char.IsDigit(c))
                charSpacing = spacing;
            else if (c == ',')
                charSpacing = commaSpacing;

            charWidths[i] = charSpacing;
            TotalSize += charSpacing;
        }
        TotalSize += FontSize - spacing; // 마지막 글자 크기 추가
        // 정렬 기준에 따른 시작 offset 계산
        float startOffset = 0f;
        switch (Algin)
        {
            case FontAlginment.Left:
                startOffset = 0f;
                break;
            case FontAlginment.Center:
                startOffset = -TotalSize / 2f + (FontSize / 2.0f);
                break;
            case FontAlginment.Right:
                startOffset = (-TotalSize) + FontSize;
                break;
        }

        // 두 번째 루프: 위치 배치
        float offset = startOffset;
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            float charSpacing = charWidths[i];

            if (c == '!')
            {
                digits[i].sprite = addFirstImg;
            }
            else if (char.IsDigit(c))
            {
                int digit = c - '0';
                digits[i].sprite = fontAsset.digitSprites[digit];
            }
            else if (c == ',')
            {
                digits[i].sprite = fontAsset.commaSprite;
            }

            RectTransform rt = digits[i].rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(offset, 0);
            rt.sizeDelta = new Vector2(FontSize, FontSize);
            offset += charSpacing;

            digits[i].gameObject.SetActive(true);
        }
        for (int i = str.Length; i < digits.Count; i++)
            digits[i].gameObject.SetActive(false);
    }

    void EnsureImageCount(int count)
    {
        while (digits.Count < count)
        {
            var go = new GameObject($"Digit_{digits.Count}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            Image img = go.GetComponent<Image>();
            if (material != null)
                img.material = material;
            digits.Add(img);
        }
    }

    int old_Val = -9982374;
    float old_size = -1;
    float old_spacing = -1;
    float old_commaSpacing = -1;
    bool old_zeroPadding = false;
    int old_padDigits = 0;
    bool old_useComma = false;
    FontAlginment old_Algin = (FontAlginment)(-1);
    DigitFontAsset old_fontAsset = null;
    Sprite old_addFirstImg = null;
    Vector2 old_vAddSize = Vector2.zero;

    private void OnValidate()
    {
        // 인스펙터 값이 변경되면 즉시 갱신이 필요함을 표시합니다.
        // OnValidate는 에디터에서만 호출됩니다.
        old_Val = -9982374; // Update에서 강제로 Refresh가 호출되도록 초기화
    }

    private void Update()
    {
        bool visualChanged = FontSize != old_size ||
                             !Mathf.Approximately(spacingPer, old_spacing) ||
                             !Mathf.Approximately(commaSpacing, old_commaSpacing) ||
                             zeroPadding != old_zeroPadding ||
                             padDigits != old_padDigits ||
                             useComma != old_useComma ||
                             Algin != old_Algin ||
                             fontAsset != old_fontAsset ||
                             addFirstImg != old_addFirstImg ||
                             vAddSize != old_vAddSize;

        if (visualChanged)
        {
            old_size = FontSize;
            old_spacing = spacingPer;
            old_commaSpacing = commaSpacing;
            old_zeroPadding = zeroPadding;
            old_padDigits = padDigits;
            old_useComma = useComma;
            old_Algin = Algin;
            old_fontAsset = fontAsset;
            old_addFirstImg = addFirstImg;
            old_vAddSize = vAddSize;
        }

        bool needsAnimationTrigger = (value != old_Val);
        bool needsRefresh = visualChanged || needsAnimationTrigger;

        if (UseCount && Application.isPlaying)
        {
            if (needsAnimationTrigger)
            {
                animStartValue = displayedValue;
                animStartTime = Time.time;
                float diff = Mathf.Abs(value - animStartValue);
                // 100,000 차이일 때 5.0초가 되도록 (5/100000 = 0.00005)
                animDuration = Mathf.Clamp(diff * 0.00005f, 0.5f, 5.0f);
            }

            if (Mathf.Abs(displayedValue - value) > 0.0001f)
            {
                IsCount = true;
                float t = (Time.time - animStartTime) / animDuration;
                if (t >= 1.0f)
                {
                    displayedValue = value;
                }
                else
                {
                    displayedValue = Mathf.Lerp(animStartValue, value, t);
                }

                int currentInt = Mathf.RoundToInt(displayedValue);
                if (currentInt != lastDisplayedInt)
                {
                    needsRefresh = true;
                    lastDisplayedInt = currentInt;
                }
            }
            else
            {
                IsCount = false;
            }
        }
        else
        {
            IsCount = false;
            if (Mathf.Abs(displayedValue - value) > 0.0001f)
            {
                displayedValue = value;
                needsRefresh = true;
                lastDisplayedInt = Mathf.RoundToInt(displayedValue);
            }
        }

        if (FontSize != old_size || old_spacing != spacingPer)
        {
            needsRefresh = true;
            old_size = FontSize;
            old_spacing = spacingPer;
        }

        if (needsRefresh)
        {
            if (!Application.isPlaying)
            {
                Clear();
            }
            Refresh();
        }

        old_Val = value;
    }

    void Start()
    {
        int cnt = transform.childCount;
        for (int i = 0; i < cnt; i++)
        {
            if (Application.isPlaying)
                Destroy(transform.GetChild(i).gameObject);
            else
                DestroyImmediate(transform.GetChild(0).gameObject);
        }
        digits.Clear();

        displayedValue = value;
        lastDisplayedInt = Mathf.RoundToInt(displayedValue);
        Refresh();
    }
}