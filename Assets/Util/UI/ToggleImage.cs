using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleImage : MonoBehaviour
{
        [Header("Target Image")]
    public Image image;

    [Header("Sprites [0 = Off, 1 = On]")]
    public Sprite[] Sprites;

    [Tooltip("체크하면 Sprites[1], 해제하면 Sprites[0]")]
    [SerializeField]
    private bool toggle;

    public bool Toggle
    {
        get => toggle;
        set
        {
            toggle = value;
            if (image == null)
                image = GetComponent<Image>();

            if (Sprites != null && Sprites.Length >= 2)
                image.sprite = toggle ? Sprites[1] : Sprites[0];
        }
    }

    private void OnValidate()
    {
        // 인스펙터에서 값이 바뀔 때 바로 반영
        Toggle = toggle;
    }
}
