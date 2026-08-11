using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyBoxChange : MonoBehaviour {

    public Material sky;
    public Material Oldsky;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnEnable()
    {
        Oldsky = RenderSettings.skybox;
        RenderSettings.skybox = sky;
    }

    private void OnDisable()
    {
        
        RenderSettings.skybox = Oldsky;
    }
}
