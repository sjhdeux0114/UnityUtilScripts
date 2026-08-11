using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FND", menuName = "SJHDeux/UI/FNDAsset")]
public class FND_SpriteInfo : ScriptableObject
{
    public FND_Data fndData;
    public Sprite spr_up;
    public Sprite spr_up_left;
    public Sprite spr_up_right;
    public Sprite spr_mid;
    public Sprite spr_bottom_left;
    public Sprite spr_bottom_right;
    public Sprite spr_bottom;
    public float offset_x = 0f;

}
