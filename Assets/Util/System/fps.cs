using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fps : MonoBehaviour {

    public bool bgui;
    public int _fps;
    int cnt;
    float times;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        times += Time.deltaTime;
        cnt++;
        if (times >= 1)
        {
            times = 0;
            _fps = cnt;
            cnt = 0;
        }
		
	}

    void OnGUI()
    {
        if (bgui)
        {
            GUI.Button(new Rect(400, 0, 100, 30), "fps:" + _fps);
        }
    }
}
