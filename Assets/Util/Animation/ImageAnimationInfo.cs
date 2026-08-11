using UnityEngine;

public enum SPRITE_DIR
{
    NORMAL,
    FLIP_X,
    FLIP_Y,
    FLIP_XY
}

[CreateAssetMenu(fileName = "SpriteGroup", menuName = "SpriteGroup")]
public class SpriteGroup : ScriptableObject
{
    public string _Name;
    public Sprite[] Sprites;
    public float _fps;
    public SndClass _snd;
    public SPRITE_DIR _dir;
}