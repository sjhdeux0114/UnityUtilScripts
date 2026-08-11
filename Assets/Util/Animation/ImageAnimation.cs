using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public enum IMAGE_ANI_PLAY_MODE
{
    _LOOP = 0,
    _ONCE,
    _ONCE_DESTROY,
    _ONCE_HIDE,
    _BACKWARD,
    _BACKWARD_HIDE,
    _PINGPONG,
    _PINGPONG_LOOP,
    _PINGPONG_BACK,
    _NONE
}
public class ImageAnimation : MonoBehaviour
{
    public IMAGE_ANI_PLAY_MODE _mode = IMAGE_ANI_PLAY_MODE._ONCE;
    IMAGE_ANI_PLAY_MODE _Org_Mode = IMAGE_ANI_PLAY_MODE._NONE;
    public float _Fps = 30.0f;
    public bool StartPlay = true;
    public bool bPlay;
    public bool RandomStartFrame = false;
    public string Image_addr;

    public UnityEvent StartEvent;
    public UnityEvent EndEvent;

    public bool bSpriteRender = false;
    public SpriteRenderer _Spr;
    public Image _img;
    public Sprite[] ImgList;
    int frame = 0;
    float fps_delay = 0;
    float frameTime = 0;
    public bool bInit_Frame = false;
    public bool bUnscaled;
    // Use this for initialization
    void Start()
    {

    }
    private void OnEnable()
    {
        if (_Org_Mode == IMAGE_ANI_PLAY_MODE._NONE)
            _Org_Mode = _mode;

        _mode = _Org_Mode;
        if (bSpriteRender)
            _Spr = GetComponent<SpriteRenderer>();
        else
            _img = GetComponent<Image>();
        if (ImgList.Length <= 0)
        {
            Get_Image_List();
        }

        if (bInit_Frame)
        {
            if (ImgList.Length > 0)
            {
                frame = 0;
                if (bSpriteRender)
                    _Spr.sprite = ImgList[frame];
                else
                    _img.sprite = ImgList[frame];
            }
        }

        if (StartPlay)
        {
            Play();
        }

    }
    bool bDir = true;
    float delayTime;
    public void Play(float delay = 0)
    {
        if (bSpriteRender)
            _Spr.enabled = true;
        else
            _img.enabled = true;
        bPlay = true;
        frame = 0;
        fps_delay = 1.0f / _Fps;
        frameTime = Time.time;
        if (bUnscaled)
            frameTime = Time.unscaledTime;
        delayTime = delay;
        if (RandomStartFrame)
            frameTime = Time.time - Random.Range(0.0f, 1.0f);

        int ret_Fr = frame;

        if (_mode == IMAGE_ANI_PLAY_MODE._BACKWARD ||
            _mode == IMAGE_ANI_PLAY_MODE._BACKWARD_HIDE)
        {
            ret_Fr = ImgList.Length - 1 - frame;
        }

        if (bSpriteRender)
            _Spr.sprite = ImgList[ret_Fr];
        else
            _img.sprite = ImgList[ret_Fr];
        StartEvent.Invoke();
    }
    public void PlayFrame(int fr)
    {
        Play();
        frame = fr;
    }

    public void Stop()
    {
        bPlay = false;
        frame = 0;
        fps_delay = 1.0f / _Fps;
        frameTime = Time.time;
        if (bSpriteRender)
            _Spr.sprite = ImgList[frame];
        else
            _img.sprite = ImgList[frame];
    }

    public void Set_Frame(int n)
    {
        if (n >= ImgList.Length)
            return;

        if (bSpriteRender)
            _Spr.sprite = ImgList[n];
        else
            _img.sprite = ImgList[n];
    }

    public void _Update(float deltaTime)
    {
        if (bPlay)
        {
            if (delayTime > 0)
            {
                delayTime -= deltaTime;

                frameTime = Time.time;
                if (bUnscaled)
                    frameTime = Time.unscaledTime;
                return;
            }

            fps_delay = 1.0f / _Fps;
            float tmp = Time.time - frameTime;
            if (bUnscaled)
                tmp = Time.unscaledTime - frameTime;
            int Total_frame = (int)(tmp / fps_delay);
            int oldFrame = frame;
            int Count = 0;
            int ret_Fr = 0;
            bool bEnd = false;

            int maxFrame = ImgList.Length;

            if (_mode == IMAGE_ANI_PLAY_MODE._PINGPONG ||
                _mode == IMAGE_ANI_PLAY_MODE._PINGPONG_BACK ||
                _mode == IMAGE_ANI_PLAY_MODE._PINGPONG_LOOP)
            {
                maxFrame = ImgList.Length * 2 - 1;
            }
            if (Total_frame > 0)
            {
                Count = Total_frame / maxFrame;
                ret_Fr = Total_frame % maxFrame;
                switch (_mode)
                {
                    case IMAGE_ANI_PLAY_MODE._PINGPONG:
                    case IMAGE_ANI_PLAY_MODE._PINGPONG_LOOP:
                    case IMAGE_ANI_PLAY_MODE._PINGPONG_BACK:
                        int half = ImgList.Length;
                        if (ret_Fr >= half)
                        {
                            ret_Fr = (half - 1) - (ret_Fr - half);
                        }
                        break;

                }
            }

            switch (_mode)
            {
                case IMAGE_ANI_PLAY_MODE._BACKWARD:
                case IMAGE_ANI_PLAY_MODE._BACKWARD_HIDE:
                case IMAGE_ANI_PLAY_MODE._PINGPONG_BACK:
                    ret_Fr = ImgList.Length - ret_Fr - 1;
                    break;
            }

            if (ret_Fr < 0) ret_Fr = 0;
            if (ret_Fr >= ImgList.Length) ret_Fr = ImgList.Length - 1;
            if (Count >= 1)
            {
                if (_mode == IMAGE_ANI_PLAY_MODE._ONCE ||
                    _mode == IMAGE_ANI_PLAY_MODE._ONCE_HIDE ||
                    _mode == IMAGE_ANI_PLAY_MODE._BACKWARD ||
                    _mode == IMAGE_ANI_PLAY_MODE._BACKWARD_HIDE ||
                    _mode == IMAGE_ANI_PLAY_MODE._ONCE_DESTROY ||
                    _mode == IMAGE_ANI_PLAY_MODE._PINGPONG)
                {
                    bEnd = true;
                    ret_Fr = ImgList.Length - 1;
                    if (_mode == IMAGE_ANI_PLAY_MODE._BACKWARD ||
                    _mode == IMAGE_ANI_PLAY_MODE._BACKWARD_HIDE)
                        ret_Fr = 0;
                }
            }
            if (oldFrame != ret_Fr)
            {
                if (bSpriteRender)
                    _Spr.sprite = ImgList[ret_Fr];
                else
                    _img.sprite = ImgList[ret_Fr];
                frame = ret_Fr;
            }
            if (bEnd)
            {
                if (_mode == IMAGE_ANI_PLAY_MODE._LOOP)
                    frame = 0;
                else if (_mode == IMAGE_ANI_PLAY_MODE._ONCE)
                {
                    bPlay = false;
                    EndEvent.Invoke();
                }
                else if (_mode == IMAGE_ANI_PLAY_MODE._PINGPONG)
                {
                    frame = 0;
                    if (bDir)
                        bDir = false;
                    else
                    {
                        bPlay = false;
                        EndEvent.Invoke();
                    }
                }
                else if (_mode == IMAGE_ANI_PLAY_MODE._PINGPONG_BACK)
                {
                    frame = 0;
                    if (!bDir)
                        bDir = true;
                    else
                    {
                        bPlay = false;
                        EndEvent.Invoke();
                    }
                }
                else if (_mode == IMAGE_ANI_PLAY_MODE._PINGPONG_LOOP)
                {
                    frame = 0;
                    bDir ^= true;
                }
                else if (_mode == IMAGE_ANI_PLAY_MODE._BACKWARD)
                {
                    bPlay = false;
                    EndEvent.Invoke();

                }
                else if (_mode == IMAGE_ANI_PLAY_MODE._BACKWARD_HIDE)
                {
                    bPlay = false;
                    if (Application.isPlaying)
                        gameObject.SetActive(false);
                    EndEvent.Invoke();
                }
                else if (_mode == IMAGE_ANI_PLAY_MODE._ONCE_DESTROY)
                {
                    bPlay = false;
                    if (bSpriteRender)
                        _Spr.enabled = false;
                    else
                        _img.enabled = false;

                    EndEvent.Invoke();
                    Destroy(gameObject);
                }
                else if (_mode == IMAGE_ANI_PLAY_MODE._ONCE_HIDE)
                {
                    bPlay = false;
                    if(Application.isPlaying)
                        gameObject.SetActive(false);
                    EndEvent.Invoke();
                }
            }



        }

    }

    // Update is called once per frame
    void Update()
    {

        if (bUnscaled)
            _Update(Time.unscaledDeltaTime);
        else
            _Update(Time.deltaTime);



    }
    public void PlayBackward()
    {
        _Org_Mode = _mode;
        _mode = IMAGE_ANI_PLAY_MODE._BACKWARD;
        Play();
    }

    [ContextMenu("GetImage")]
    public void Get_Image_List()
    {
        if (bSpriteRender)
            _Spr = GetComponent<SpriteRenderer>();
        else
            _img = GetComponent<Image>();
        ImgList = Resources.LoadAll<Sprite>(Image_addr);

    }
}
