using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FND_ImageData : MonoBehaviour
{
    public Image img_up;
    public Image img_up_left;
    public Image img_up_right;
    public Image img_mid;
    public Image img_bottom_left;
    public Image img_bottom_right;
    public Image img_bottom;

    public void Init(FND_SpriteInfo data)
    {
        img_up.sprite = data.spr_up;
        img_up_left.sprite = data.spr_up_left;
        img_up_right.sprite = data.spr_up_right;
        img_mid.sprite = data.spr_mid;
        img_bottom_left.sprite = data.spr_bottom_left;
        img_bottom_right.sprite = data.spr_bottom_right;
        img_bottom.sprite = data.spr_bottom;
            
    }

    public void SetColor( Color col)
    {
        img_up.color = col;
        img_up_left.color = col;
        img_up_right.color = col;
        img_mid.color = col;
        img_bottom_left.color = col;
        img_bottom_right.color = col;
        img_bottom.color = col;
    }

    public void Set(FND_Dataset data,Color col)
    {


        if (data == null)
        {

            img_up.enabled = false;
            img_up_left.enabled = false;
            img_up_right.enabled = false;
            img_mid.enabled = false;
            img_bottom_left.enabled = false;
            img_bottom_right.enabled = false;
            img_bottom.enabled = false;
            return;
        }

        img_up.color = col;
        img_up_left.color = col;
        img_up_right.color = col;
        img_mid.color = col;
        img_bottom_left.color = col;
        img_bottom_right.color = col;
        img_bottom.color = col;

        img_up.enabled = data.up;
        img_up_left.enabled = data.up_left;
        img_up_right.enabled = data.up_right;
        img_mid.enabled = data.mid;
        img_bottom_left.enabled = data.bottom_left;
        img_bottom_right.enabled = data.bottom_right;
        img_bottom.enabled = data.bottom;
    }
}
