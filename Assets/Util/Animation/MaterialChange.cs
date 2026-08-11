using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialChange : MonoBehaviour
{

    public Material[] MatList;
    public Renderer[] _Render;
    public UnityEngine.UI.Image[] _Image;
    public UnityEngine.UI.RawImage[] _raw_Image;
    public bool[] bTest;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < bTest.Length; i++)
        {
            if (bTest[i])
            {
                bTest[i] = false;
                Change_Mat(i);
            }

        }
    }

    public void Change_Mat(int n)
    {
        if (n < 0 || n >= MatList.Length)
            return;
        for (int i = 0; i < _Render.Length; i++)
            _Render[i].material = MatList[n];
        for (int i = 0; i < _Image.Length; i++)
            _Image[i].material = MatList[n];
        for (int i = 0; i < _raw_Image.Length; i++)
            _raw_Image[i].material = MatList[n];
    }
}
