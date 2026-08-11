using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class _PlayList
{
    
    public int Num;
    public int ST_Num;
    public int ColorNum;
    public Color NowCol;
}

public class zoomOnPlay : MonoBehaviour {

    public ZoomOn[] _List;
    public MeshRenderer[] _Renders;
    public _PlayList[] playList;
    public float Speed;
    public Color[] ColorList;
    
    
    // Use this for initialization
    void Start () {
        
        for (int i=0;i<playList.Length;i++)
        {
            playList[i].Num = playList[i].ST_Num;
            playList[i].ColorNum = 0;
            playList[i].NowCol = ColorList[playList[i].ColorNum];
        }
        StartCoroutine(Action());
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator Action()
    {
        while(true)
        {
            for (int i = 0; i < playList.Length; i++)
            {
                _Renders[playList[i].Num].material.SetColor("_EmissionColor", playList[i].NowCol);
                _List[playList[i].Num].Play();
                playList[i].Num++;
                if (playList[i].Num >= _List.Length)
                {
                    playList[i].Num = 0;
                    playList[i].ColorNum++;
                    if (playList[i].ColorNum >= ColorList.Length)
                        playList[i].ColorNum = 0;

                    playList[i].NowCol = ColorList[playList[i].ColorNum];

                }
            }

            yield return new WaitForSeconds(Speed);
        }
    }

    [ContextMenu("GetData")]
    public void GetData()
    {
        int n = transform.childCount;

        _List = new ZoomOn[n];
        _Renders = new MeshRenderer[n];

        for (int i=0;i<n;i++)
        {
            _List[i] = transform.GetChild(i).GetComponent<ZoomOn>();
            _Renders[i] = _List[i].GetComponent<MeshRenderer>();
        }
    }
}
