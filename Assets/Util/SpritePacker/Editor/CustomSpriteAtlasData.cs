using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpriteMetaInfo
{
    public string name;
    public Rect rect;
    public int alignment;
    public Vector2 pivot;
    public Vector4 border;
    public string spriteID; // Unity의 GUID를 string으로 보존
}

[CreateAssetMenu(fileName = "NewSpriteAtlasData", menuName = "Custom/Sprite Atlas Data")]
public class CustomSpriteAtlasData : ScriptableObject
{
    [Header("아틀라스 원본 이미지")]
    public Texture2D atlasTexture;

    [Header("스프라이트 메타 정보")]
    public List<SpriteMetaInfo> sprites = new List<SpriteMetaInfo>();
}
