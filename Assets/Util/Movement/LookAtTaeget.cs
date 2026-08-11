using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTaeget : MonoBehaviour {
    public Transform target;

    public float damping = 0.5f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if(target)
        {
            Quaternion old = transform.rotation;
            transform.LookAt(target.position);

            transform.rotation = Quaternion.Slerp(old, transform.rotation, Time.deltaTime * damping);


        }
		
	}
}
