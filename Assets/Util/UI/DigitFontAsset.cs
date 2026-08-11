// Assets/Scripts/UI/DigitFontAsset.cs
using UnityEngine;

[CreateAssetMenu(fileName = "DigitFontAsset", menuName = "SJHDeux/UI/DigitFontAsset")]
public class DigitFontAsset : ScriptableObject
{
    public Sprite[] digitSprites = new Sprite[10]; // index: 0~9
    public Sprite commaSprite;
}
