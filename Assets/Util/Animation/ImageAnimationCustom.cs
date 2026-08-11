using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class ImageAniData
{
    public Sprite[] ImgList;
    public Rect[] ImgRect;
}
[System.Serializable]
public class NextAni
{
    public int AniNum;
    public IMAGE_ANI_PLAY_MODE _mode;
    public NextAni(int _n, IMAGE_ANI_PLAY_MODE _m= IMAGE_ANI_PLAY_MODE._ONCE)
    {
        AniNum = _n;
        _mode = _m;
    }
}
[System.Serializable]
public class ImageAniList
{
    public List<NextAni> AniList = new List<NextAni>();
}

public class ImageAnimationCustom : MonoBehaviour {
    public IMAGE_ANI_PLAY_MODE _mode;
    public float _Fps = 60.0f;
    public bool StartPlay = true;
    public bool bPlay;
    public bool RandomStartFrame = false;
    public string Image_addr;

    public UnityEvent EndEvent;

    public Image _img;
    public ImageAniData[] _AniData;
    public int AniNum = 0;
    public List<NextAni> StartAni;
    public List<NextAni> NextAniNum = new List<NextAni>();
    public ImageAniList[] AniList;
    public int Call_AniList = -1;
    int frame = 0;
    float fps_delay = 0;
    float frameTime = 0;
    public string _DecodeString;
    RectTransform _RectTr;
    Vector3 OrgPos;
    // Use this for initialization
    void Start () {

        _RectTr = GetComponent<RectTransform>();
        OrgPos = transform.localPosition;

    }
    public void _Ani_Start(int Ani)
    {
        Call_AniList = Ani;

    }
    [ContextMenu("DecodeString")]
    public void Decode_Data()
    {
        ImageAniData _ANI = _AniData[AniNum];
        string[] tmp_Main = _DecodeString.Split('}');
        _ANI.ImgRect = new Rect[tmp_Main.Length-1];
        for (int i=0;i< _ANI.ImgRect.Length;i++)
        {
            _ANI.ImgRect[i] = new Rect();
            string[] tmp = tmp_Main[i].Split(',');

            int st_P = 0;
            if(i > 0)
                st_P = 1;

            string _x = tmp[st_P+1].Split(':')[1];
            string _y = tmp[st_P + 2].Split(':')[1];
            string _w = tmp[st_P + 3].Split(':')[1];
            string _h = tmp[st_P + 4].Split(':')[1];

            _ANI.ImgRect[i].x = float.Parse(_x);
            _ANI.ImgRect[i].y = float.Parse(_y);
            _ANI.ImgRect[i].width = float.Parse(_w);
            _ANI.ImgRect[i].height = float.Parse(_h);

        }
    }
    private void OnEnable()
    {
        _img = GetComponent<Image>();
        bPlay = false;
        if (StartPlay)
        {
            Play();
        }
    }
    public void Set_Frame(int n)
    {
        _RectTr = GetComponent<RectTransform>();
        _img.enabled = true;
        ImageAniData _ANI = _AniData[0];
        _img.sprite = _ANI.ImgList[0];

        _RectTr.sizeDelta = new Vector2(_ANI.ImgRect[0].width, _ANI.ImgRect[0].height);

        _RectTr.anchoredPosition = new Vector2(_ANI.ImgRect[0].x, -_ANI.ImgRect[0].y);

    }
    public void Play(int _aniNum=0)
    {
        _img.enabled = true;
        bPlay = true;
        frame = 0;
        fps_delay = 1.0f / _Fps;
        frameTime = 0;
        NextAniNum.Clear();
        if (StartAni.Count > 0)
        {
            AniNum = StartAni[0].AniNum;
            _mode = StartAni[0]._mode;

            if (StartAni.Count > 1)
            {
                for(int i=1;i< StartAni.Count;i++)
                {
                    NextAni(StartAni[i]);
                }
            }

        }
        else
        {
            if (_aniNum >= 0 && _aniNum < _AniData.Length)
                AniNum = _aniNum;
            else
                AniNum = 0;
        }
        ImageAniData _ANI = _AniData[AniNum];
        if (RandomStartFrame)
            frameTime = Random.Range(0, _ANI.ImgList.Length);
        _img.sprite = _ANI.ImgList[frame];

    }

    public void NextAni(NextAni _ani)
    {
        if(_ani.AniNum < _AniData.Length)
            NextAniNum.Add(_ani);
    }

    public void Stop()
    {
        bPlay = false;
        frame = 0;
        fps_delay = 1.0f / _Fps;
        frameTime = 0;
        ImageAniData _ANI = _AniData[AniNum];
        _img.sprite = _ANI.ImgList[frame];
    }

    void Set_AniList(int n)
    {
        if (AniList.Length > n)
        {
            bPlay = true;
            frame = 0;
            fps_delay = 1.0f / _Fps;
            frameTime = 0;

            NextAniNum.Clear();

            AniNum = AniList[n].AniList[0].AniNum;
            _mode = AniList[n].AniList[0]._mode;

            if (AniList[n].AniList.Count > 1)
            {
                for (int i = 1; i < AniList[n].AniList.Count; i++)
                {
                    NextAni(AniList[n].AniList[i]);
                }
            }
        }
    }

    // Update is called once per frame
    void Update () {

        if(Call_AniList >= 0)
        {
            Set_AniList(Call_AniList);
            Call_AniList = -1;
        }

        if (bPlay)
        {
            ImageAniData _ANI = _AniData[AniNum];

            fps_delay = 1.0f / _Fps;
            frameTime += Time.deltaTime;
            if(frameTime >= fps_delay)
            {
                frameTime = 0;

                int ret_fr = frame;
                if(_mode == IMAGE_ANI_PLAY_MODE._BACKWARD ||
                    _mode == IMAGE_ANI_PLAY_MODE._BACKWARD_HIDE)
                {
                    ret_fr = _ANI.ImgList.Length - frame - 1;
                }

                _img.sprite = _ANI.ImgList[ret_fr];
                if(_ANI.ImgRect[ret_fr].width > 0 && _ANI.ImgRect[ret_fr].height > 0)
                    _RectTr.sizeDelta = new Vector2(_ANI.ImgRect[ret_fr].width, _ANI.ImgRect[ret_fr].height);
                //transform.localPosition = OrgPos + new Vector3(ImgRect[0].x - ImgRect[frame].x,
                //                    ImgRect[0].y - ImgRect[frame].y, 0);
                _RectTr.anchoredPosition = new Vector2(_ANI.ImgRect[ret_fr].x,-_ANI.ImgRect[ret_fr].y);

                frame++;
                bool bMax = false;
                if (frame >= _ANI.ImgList.Length)
                    bMax = true;
                if (bMax)
                {
                    if (_mode == IMAGE_ANI_PLAY_MODE._LOOP)
                        frame = 0;
                    else if (_mode == IMAGE_ANI_PLAY_MODE._ONCE)
                    {
                        if (NextAniNum.Count > 0)
                        {
                            AniNum = NextAniNum[0].AniNum;
                            _mode = NextAniNum[0]._mode;
                            NextAniNum.RemoveAt(0);
                            frame = 0;
                        }
                        else
                        {
                            bPlay = false;
                            EndEvent.Invoke();
                        }
                    }
                    else if (_mode == IMAGE_ANI_PLAY_MODE._ONCE_DESTROY)
                    {
                        bPlay = false;
                        _img.enabled = false;

                        EndEvent.Invoke();
                        Destroy(gameObject);
                    }
                    else if (_mode == IMAGE_ANI_PLAY_MODE._ONCE_HIDE ||
                         _mode == IMAGE_ANI_PLAY_MODE._BACKWARD_HIDE)
                    {
                        bPlay = false;
                        gameObject.SetActive(false);
                        EndEvent.Invoke();
                    }
                }
            }
            
            
        }
		
	}


}
