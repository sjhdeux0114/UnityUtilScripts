using UnityEngine;
using System.Collections;

[System.Serializable]
public enum DIR_ENUM{
    DIR_X,
    DIR_Y,
    DIR_Z,
}
public class RotationObject : MonoBehaviour {

    public bool m_bWorldSpace = false;
    public Vector3 m_vRotationValue = new Vector3(0, 360, 0);

    public bool bLoop=false;
    
    float Add_Dir = 1;
    public float Loop_Times;
    float Loop_Times_Check;
    public float fRandomLoopTime = 1;
    Quaternion Org_Quaternion;


    // Use this for initialization
    void Start()
    {
        if (bLoop)
        {
            Loop_Times_Check += Random.Range(-fRandomLoopTime, fRandomLoopTime);
        }
        
        Org_Quaternion = transform.rotation;


    }

    
	
	// Update is called once per frame
	void Update () {
        if (bLoop)
        {
            if (Add_Dir > 0)
            {

                transform.Rotate(Time.deltaTime * m_vRotationValue.x * Add_Dir,
                    Time.deltaTime * m_vRotationValue.y * Add_Dir,
                    Time.deltaTime * m_vRotationValue.z * Add_Dir,
                    (m_bWorldSpace ? Space.World : Space.Self));
                Loop_Times_Check += Time.deltaTime;

                if (Loop_Times_Check >= Loop_Times)
                {
                    Add_Dir *= -1;
                    Loop_Times_Check -= Loop_Times;

                    Loop_Times_Check += Random.Range(-fRandomLoopTime, fRandomLoopTime);
                }
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Org_Quaternion, Time.deltaTime * m_vRotationValue.magnitude / 10.0f);

                if (Quaternion.Angle(transform.rotation, Org_Quaternion) < 1f)
                {
                    Add_Dir *= -1;

                    Loop_Times_Check += Random.Range(-fRandomLoopTime, fRandomLoopTime);
                }   
            }
        }
        else
        {
            transform.Rotate(Time.deltaTime * m_vRotationValue.x,
                Time.deltaTime * m_vRotationValue.y,
                Time.deltaTime * m_vRotationValue.z,
                (m_bWorldSpace ? Space.World : Space.Self));
        }
	
	}
}
