using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolingObject : MonoBehaviour
{
    public GameObject Obj;
    public int MaxCnt;
    public List<GameObject> PoolObjects = new List<GameObject>();
    public float HideTime = 0;
    int Count = 0;
    public bool isHirerarchi = false;
    public bool isNewFirstUp = false;
    public bool isNewLastUp = false;

    // Start is called before the first frame update
    void Start()
    {
        if (isHirerarchi)
        {
            PoolObjects.Add(Obj);
            Obj.SetActive(false);
        }

        if (HideTime > 0)
        {
            Obj.AddComponent<AutoHide>().Times = HideTime;
        }
        for (int i=0;i<MaxCnt;i++)
        {
            GameObject g = Instantiate(Obj, transform);
            if (HideTime > 0)
                g.AddComponent<AutoHide>().Times = HideTime;
            g.SetActive(false);
            PoolObjects.Add(g);
        }
        Count = 0;


    }

    public GameObject Create(Vector3 _pos)
    {
        GameObject _obj = PoolObjects[Count];

        _obj.SetActive(true);
        if (isNewFirstUp)
        {
            _obj.transform.SetAsLastSibling();
        }
        if(isNewLastUp)
        {
            _obj.transform.SetAsLastSibling();
        }
        _obj.transform.position = _pos;

        Count++;
        if (Count >= PoolObjects.Count)
            Count = 0;

        return _obj;
    }
    public GameObject Create()
    {
        GameObject _obj = PoolObjects[Count];

        _obj.SetActive(true);
        if (isNewFirstUp)
        {
            _obj.transform.SetAsLastSibling();
        }
        if(isNewLastUp)
        {
            _obj.transform.SetAsLastSibling();
        }
        Count++;
        if (Count >= PoolObjects.Count)
            Count = 0;

        return _obj;
    }

    public void HideAll()
    {
        for(int i=0;i< PoolObjects.Count;i++)
        {
            PoolObjects[i].SetActive(false);
        }
        Count = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
