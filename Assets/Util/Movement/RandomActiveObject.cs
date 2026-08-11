using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomActiveObject : MonoBehaviour
{
    public GameObject[] _Obj;
    public Vector2[] Timming;
    public float[] Times;
    // Start is called before the first frame update
    void Start()
    {
        Times = new float[_Obj.Length];
        for (int i=0;i< _Obj.Length;i++)
        {
            _Obj[i].SetActive(false);
            Times[i] = Random.Range(0, Timming[i].y);

        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < _Obj.Length; i++)
        {
            Times[i] -= Time.deltaTime;
            if (Times[i] < 0)
            {
                if (_Obj[i].activeInHierarchy)
                    _Obj[i].SetActive(false);
                else
                    _Obj[i].SetActive(true);
                Times[i] = Random.Range(Timming[i].x, Timming[i].y);
            }

        }
    }
}
