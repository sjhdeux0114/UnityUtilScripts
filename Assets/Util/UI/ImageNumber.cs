using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public enum _ALGIN
{
    _CENTER,
    _LEFT,
    _RIGHT,
}
public class ImageNumber : MonoBehaviour
{
    public GameObject Img_Obj;
    public int MAX_LENGH = 10;
    public int Num;
    public bool bUpdate;
    public float GapX;
    public float FontW;
    public _ALGIN _algin;
    public float Zoom;
    public Sprite[] _spr_Number;
    public List<Image> _imgNumber = new List<Image>();
    public bool b_Zero_Hide = false;
    public bool b_Zero_FULL = false;
    // Start is called before the first frame update
    void Start()
    {
        if(_imgNumber.Count <= 0)
            _Init(false);
        bUpdate = true;
    }

    public void _Init(bool bSizeInit=true)
    {
        int childmax = transform.childCount;
        for(int i=0;i< childmax;i++)
        {
            if(Application.isPlaying)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            else
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }
        _imgNumber.Clear();

        for (int i = 0; i < MAX_LENGH; i++)
        {
            GameObject g = Instantiate(Img_Obj, transform);
            _imgNumber.Add(g.GetComponent<Image>());
            g.GetComponent<RectTransform>().sizeDelta = new Vector2(_spr_Number[0].rect.width, _spr_Number[0].rect.height);
        }

        if (bSizeInit)
            FontW = _spr_Number[0].rect.width;

    }
    public void Set_Number(int n)
    {

        bUpdate = true;
        Num = n;
    }

    public void _Number_Update()
    {
        if (_imgNumber.Count <= 0)
            _Init();
        if (Num < 0)
            Num = -Num;

        if(b_Zero_Hide)
        {
            if(Num <= 0)
            {
                for (int i = 0; i < _imgNumber.Count; i++)
                    _imgNumber[i].enabled = false;

                return;
            }

        }
        string tmp = string.Format("{0}", Num);
        if (b_Zero_FULL)
        {

            tmp = string.Format("{0}", Num.ToString("D"+ MAX_LENGH.ToString()));
        }
        int cnt = tmp.Length;

        float st = 0;

        if(_algin == _ALGIN._CENTER)
        {
            st = - (((FontW + GapX) * cnt*Zoom) / 2) + FontW*Zoom/2;
        }
        if (_algin == _ALGIN._RIGHT)
        {
            st = -(((FontW + GapX) * (cnt-1) * Zoom) );
        }

        for (int i=0;i< _imgNumber.Count;i++)
        {
            _imgNumber[i].transform.localScale = Vector3.one * Zoom;
            _imgNumber[i].transform.localPosition = new Vector3(st + (FontW + GapX) * i * Zoom, 0, 0);
            if (i < cnt)
            {
                _imgNumber[i].enabled = true;
                
                int n = int.Parse(tmp[i].ToString());
                _imgNumber[i].sprite = _spr_Number[n];
            }
            else
            {
                _imgNumber[i].enabled = false;
            }
        }
       
    }

    // Update is called once per frame
    void Update()
    {
        if(bUpdate)
        {
            bUpdate = false;
            _Number_Update();
        }
    }
}
