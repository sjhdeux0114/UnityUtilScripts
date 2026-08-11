using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RAMDON_MODE
{
    SELECT_ONE, //한개만 랜덤으로 뽑기
    PERCENT_ON,  //확률에 의해 나오기

}

public class RandomActiveObj : MonoBehaviour
{
    public RAMDON_MODE _Mode;
    public GameObject[] Objs;
    [Header("0~1000 사이 값으로 지정")]
    public int[] iPer_On;
    [Header("Check_Time 시간이 지나면 다시 설정할지 유무")]
    public bool bTime_Repeat;
    public float Check_Time=0;
    public bool bPositionChange;
    public bool bLocal;
    public Vector3 vMin;
    public Vector3 vMax;

    float times = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Set_Position(int n)
    {
        if(bPositionChange)
        {
            if (bLocal)
            {
                Objs[n].transform.localPosition = new Vector3(
                    Random.Range(vMin.x, vMax.x),
                    Random.Range(vMin.y, vMax.y),
                    Random.Range(vMin.z, vMax.z));
            }
            else
            {
                Objs[n].transform.position = new Vector3(
                    Random.Range(vMin.x, vMax.x),
                    Random.Range(vMin.y, vMax.y),
                    Random.Range(vMin.z, vMax.z));
            }
        }
    }

    void OnAction()
    {
        times = 0;
        switch (_Mode)
        {
            case RAMDON_MODE.SELECT_ONE:
                int n = Random.Range(0, Objs.Length);

                for (int i = 0; i < Objs.Length; i++)
                {
                    if (i == n)
                    {
                        Objs[i].SetActive(true);
                        Set_Position(i);
                    }
                    else
                        Objs[i].SetActive(false);

                    
                }
                times = Check_Time;
                break;
            case RAMDON_MODE.PERCENT_ON:
                
                
                if (iPer_On.Length != Objs.Length)
                {
                    iPer_On = new int[Objs.Length];
                    for (int i = 0; i < iPer_On.Length; i++)
                    {
                        iPer_On[i] = 1000 / Objs.Length;
                    }
                }

                for (int i = 0; i < Objs.Length; i++)
                {
                    if (Random.Range(0, 1000) < iPer_On[i])
                    {
                        Objs[i].SetActive(true);
                        Set_Position(i);
                    }
                    else
                        Objs[i].SetActive(false);
                }
                times = Check_Time;
                break;
        }
    }

    private void OnEnable()
    {
        OnAction();
    }

    // Update is called once per frame
    void Update()
    {
        if (!bTime_Repeat) return;

        if(times > 0)
        {
            times -= Time.deltaTime;
            if (times < 0) OnAction();
        }
        
    }
}
