using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterRotateRound : MonoBehaviour
{
    public Transform TR_Center;

    public Transform[] TR_Objs;
    public float Distance = 100;
    public float StartAngle;
    public float EndAngle;
    public float AddAngle;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    [ContextMenu("Setup")]
    public void Set_Rotate()
    {
        if (TR_Objs.Length < 2) return;

        float gap = (EndAngle - StartAngle) / (float)TR_Objs.Length;

        for (int i = 0; i < TR_Objs.Length; i++)
        {
            float targetAngle = StartAngle - gap * i + AddAngle;
            Debug.Log($"{targetAngle}");
            float radian = targetAngle * Mathf.Deg2Rad;
            float x = TR_Center.localPosition.x + Mathf.Cos(radian);
            float y = TR_Center.localPosition.y + Mathf.Sin(radian);

            // 3. 계산된 위치로 오브젝트를 이동 (Z축은 기존 위치 유지)
            TR_Objs[i].localPosition = new Vector3(x, y, 0);

            // 4. 오브젝트의 Z축 회전 각도를 targetAngle로 설정합니다.
            // 이렇게 하면 오브젝트가 항상 원의 바깥쪽을 바라보는 것처럼 회전합니다.
            // (오브젝트 방향에 따라 90도 등을 더하거나 뺄 수 있습니다.)
            TR_Objs[i].rotation = Quaternion.Euler(0, 0, targetAngle);

            TR_Objs[i].localPosition += TR_Objs[i].up * Distance;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
