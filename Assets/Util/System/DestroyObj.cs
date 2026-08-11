using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObj : MonoBehaviour {

    public GameObject target;
    public float delay;

	// Use this for initialization
	void Start () {
        if (!target)
            target = gameObject;

        Destroy(target, delay);
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
