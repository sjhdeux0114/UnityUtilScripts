using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class aniMationSpeed : MonoBehaviour {
    public float Speed = 1;

	// Use this for initialization
	void Start () {

        Animation ani = GetComponent<Animation>();

        AnimationState _state = ani[ani.clip.name];

        _state.speed = Speed;


    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
