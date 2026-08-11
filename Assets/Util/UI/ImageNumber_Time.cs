using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageNumber_Time : MonoBehaviour
{
    public System.DateTime _Start;
    public ImageNumber _HH;
    public ImageNumber _MM;
    public ImageNumber _SS;

    public bool bMinusTime;
    public bool bOnlyMinute;
    public bool bOnlySecond;
    bool bStart = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnEnable()
    {
        bStart = false;
        if (_HH)
        {
            _HH.Set_Number(0);
        }
        if (_MM)
        {
            _MM.Set_Number(5);
        }
        if (_SS)
        {
            _SS.Set_Number(0);
        }
    }
    public void _Wait( int addSec)
    {
        bStart = false;
        bMinusTime = true;
        _Start = System.DateTime.Now.AddSeconds(addSec + 0.5f);
        _Update();
    }

    public void _Init(bool bMinus,int addSec)
    {
        bMinusTime = bMinus;
        _Start = System.DateTime.Now.AddSeconds(addSec);
        bStart = true;
    }
    public void Add_Time(int addSec)
    {
        _Start = _Start.AddSeconds(addSec);
    }
    public int Get_Extra_Time()
    {
        System.TimeSpan TS;

        if (System.DateTime.Now > _Start)
            TS = new System.TimeSpan(0);
        else
            TS = _Start - System.DateTime.Now;

        return (int)TS.TotalSeconds;
    }

    void _Update()
    {
        System.TimeSpan TS = System.DateTime.Now - _Start;

        if (bMinusTime)
        {
            if (System.DateTime.Now > _Start)
                TS = new System.TimeSpan(0);
            else
                TS = _Start - System.DateTime.Now;
        }
        if (_HH)
        {
            _HH.Set_Number(TS.Hours);
        }
        if (_MM)
        {
            if (bOnlyMinute)
                _MM.Set_Number((int)TS.TotalMinutes);
            else
                _MM.Set_Number(TS.Minutes);
        }
        if (_SS)
        {
            if (bOnlySecond)
                _SS.Set_Number((int)TS.TotalSeconds);
            else
                _SS.Set_Number(TS.Seconds);
        }

    }
    // Update is called once per frame
    void Update()
    {
        if (!bStart)
            return;
        _Update();
    }
}
