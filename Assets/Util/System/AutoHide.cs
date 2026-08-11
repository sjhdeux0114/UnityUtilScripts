using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum HIDE_OPTION
{
    NONE,
    SCALE,
    ALPHA,
    SCALE_ALPHA,
}

public class AutoHide : MonoBehaviour
{
    public float Times = 2;
    public HIDE_OPTION hideOption = HIDE_OPTION.NONE;
    [Header("옵션에 따라 사라지는 시간")]
    public float HideOptionTime = 1;
    [Header("옵션에 따라 사라지는 Scale")]
    public float HideOptionScale = 1;

    public bool bRandom;
    public float Random_Min;
    public float Random_Max;
    private float timer = 0f;

    private Vector3 startScale;
    private bool isStartScaleSaved = false;

    private Dictionary<Component, Color> initialColors = new Dictionary<Component, Color>();

    private void Awake()
    {
        if (!isStartScaleSaved)
        {
            startScale = transform.localScale;
            isStartScaleSaved = true;
        }
    }

    private void OnEnable()
    {
        if (bRandom)
        {
            Times = Random.Range(Random_Min, Random_Max);
        }
        timer = 0f;

        ResetAndGatherVisuals();
    }

    private void ResetAndGatherVisuals()
    {
        if (!isStartScaleSaved)
        {
            startScale = transform.localScale;
            isStartScaleSaved = true;
        }
        else
        {
            transform.localScale = startScale;
        }

        // UI.Image
        Image[] images = GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            if (img == null) continue;
            if (!initialColors.ContainsKey(img))
            {
                initialColors[img] = img.color;
            }
            else
            {
                img.color = initialColors[img];
            }
        }

        // UI.Text
        Text[] texts = GetComponentsInChildren<Text>(true);
        foreach (var txt in texts)
        {
            if (txt == null) continue;
            if (!initialColors.ContainsKey(txt))
            {
                initialColors[txt] = txt.color;
            }
            else
            {
                txt.color = initialColors[txt];
            }
        }

        // TMPro.TMP_Text (covers TextMeshPro and TextMeshProUGUI)
        TMP_Text[] tmpTexts = GetComponentsInChildren<TMP_Text>(true);
        foreach (var tmp in tmpTexts)
        {
            if (tmp == null) continue;
            if (!initialColors.ContainsKey(tmp))
            {
                initialColors[tmp] = tmp.color;
            }
            else
            {
                tmp.color = initialColors[tmp];
            }
        }

        // SpriteRenderer
        SpriteRenderer[] sprs = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var spr in sprs)
        {
            if (spr == null) continue;
            if (!initialColors.ContainsKey(spr))
            {
                initialColors[spr] = spr.color;
            }
            else
            {
                spr.color = initialColors[spr];
            }
        }
    }

    void AutoHideOn()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        timer += Time.deltaTime;

        float totalTime = Times;
        if (hideOption != HIDE_OPTION.NONE)
        {
            totalTime += HideOptionTime;

            if (timer >= Times)
            {
                float animDuration = HideOptionTime;
                float progress = (animDuration > 0) ? Mathf.Clamp01((timer - Times) / animDuration) : 1.0f;

                if (hideOption == HIDE_OPTION.SCALE || hideOption == HIDE_OPTION.SCALE_ALPHA)
                {
                    UpdateScale(progress);
                }

                if (hideOption == HIDE_OPTION.ALPHA || hideOption == HIDE_OPTION.SCALE_ALPHA)
                {
                    UpdateAlpha(progress);
                }
            }
        }

        if (timer >= totalTime)
        {
            AutoHideOn();
        }
    }

    private void UpdateScale(float progress)
    {
        Vector3 targetScale = Vector3.one * HideOptionScale;
        transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
    }

    private void UpdateAlpha(float progress)
    {
        float alphaFactor = 1.0f - progress;

        foreach (var kvp in initialColors)
        {
            Component comp = kvp.Key;
            if (comp == null) continue;

            Color baseColor = kvp.Value;
            Color currentColor = baseColor;
            currentColor.a = baseColor.a * alphaFactor;

            if (comp is Image img)
            {
                img.color = currentColor;
            }
            else if (comp is Text txt)
            {
                txt.color = currentColor;
            }
            else if (comp is TMP_Text tmp)
            {
                tmp.color = currentColor;
            }
            else if (comp is SpriteRenderer spr)
            {
                spr.color = currentColor;
            }
        }
    }
}
