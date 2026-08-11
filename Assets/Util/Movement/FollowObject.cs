using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour {

    public Transform TagetObj;
    public Vector3 Add_Pos;
    public bool bLookAt;
	public bool b2D;
	public bool bLate;
	public bool bFollow;

	// Use this for initialization
	void Start () {
		
	}
	
	void Update()
	{
		if(!bLate)
			Follow();
		
	}
	
	// Update is called once per frame
	void LateUpdate () {

		if(bLate)
			Follow();
	}
    
	public void Follow()
	{
		if(!bFollow)
			return;
		if (!TagetObj.gameObject.activeInHierarchy)
		{
			transform.position = Camera.main.transform.position + Camera.main.transform.up * 2000;
			return;
		}


		transform.position = TagetObj.position + Add_Pos;

		if(bLookAt)
		{
			transform.LookAt(Camera.main.transform);
			if(b2D)
			{
				transform.Rotate(Vector3.up, 180);
			}
		}
	}
}
