using UnityEngine;
using System.Collections;

public class RotationObjectSimple : MonoBehaviour {

    public bool m_bWorldSpace = false;
    public Vector3 m_vRotationValue = new Vector3(0, 360, 0);

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(Time.deltaTime * m_vRotationValue.x, Time.deltaTime * m_vRotationValue.y, Time.deltaTime * m_vRotationValue.z, (m_bWorldSpace ? Space.World : Space.Self));
	
	}
}
