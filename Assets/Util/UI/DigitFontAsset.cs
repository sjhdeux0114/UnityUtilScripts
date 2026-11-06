// Assets/Scripts/UI/DigitFontAsset.cs
using UnityEngine;

[CreateAssetMenu(fileName = "DigitFontAsset", menuName = "SJHDeux/UI/DigitFontAsset")]
public class DigitFontAsset : ScriptableObject
{
    public Sprite[] digitSprites = new Sprite[10]; // index: 0~9
    public Sprite commaSprite;
    public Vector2 FontSize;

    [ContextMenu("GetSize")]
    public void Get_FontSize()
    {
        if (digitSprites != null && digitSprites.Length > 0)
        {
            if (digitSprites[0] != null)
            {
                FontSize = digitSprites[0].rect.size;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }
    }

    private void OnEnable()
    {
        // 프로젝트 창 등에서 에셋을 클릭/선택하여 메모리에 로드될 때 자동 호출
        Get_FontSize();
    }

    private void OnValidate()
    {
        // 인스펙터에서 스프라이트 등록/수정 등 값이 변경될 때 자동 호출
        Get_FontSize();
    }
}
