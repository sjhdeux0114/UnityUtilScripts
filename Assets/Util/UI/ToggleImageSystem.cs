using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ToggleImageData
{
    public Sprite Image;
    public Material mat;
    public Color color;
    public Vector3 pos;
    public Vector3 scale;
}

public class ToggleImageSystem : MonoBehaviour
{
    public ToggleImageData[] toggleData;
    public ToggleImageData currentData => toggleData[Value ? 1 : 0];
    public ToggleImageData StData => toggleData[Value ? 0 : 1];
    public AnimationCurve Curve_Pos;
    public AnimationCurve Curve_Scale;
    public AnimationCurve Curve_Color;
    public float ChangeTime = 1.0f;
    float timer = 0;
    public Image img;
    [SerializeField]
    bool value = false;
    public bool Value
    {
        get { return value; }
        set
        {
            this.value = value;
            ApplyData();
        }
    }

    void ApplyData()
    {
        if (img == null)
            img = GetComponent<Image>();

        if (img)
        {
            if (currentData.Image != null)
                img.sprite = currentData.Image;
            if (currentData.mat != null)
                img.material = currentData.mat;
        }
        timer = ChangeTime;

    }
    // Start is called before the first frame update
    void Start()
    {
        if (img == null)
            img = GetComponent<Image>();
        timer = ChangeTime;


    }

    public void SetValue(bool val)
    {
        if (val != value)
        {
            value = val;
            ApplyData();
        }
    }

    [InspectorButton]
    public void Toggle()
    {
        Value = !Value;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            float t = 1.0f - (timer / ChangeTime);
            if (timer < 0)
                t = 1.0f;
            if (img)
            {
                img.color = Color.LerpUnclamped(StData.color, currentData.color, Curve_Color.Evaluate(t));
            }
            transform.localPosition = Vector3.LerpUnclamped(StData.pos, currentData.pos, Curve_Pos.Evaluate(t));
            transform.localScale = Vector3.LerpUnclamped(StData.scale, currentData.scale, Curve_Scale.Evaluate(t));
        }
    }
}
