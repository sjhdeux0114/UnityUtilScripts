using UnityEngine;
using System.Collections;

public class UvAnimation : MonoBehaviour {

    public Vector2 Power;
    public Vector2 Power_Normal;
    private Vector2 _texOffset;
    private Vector2 _normaltexOffset;
    private Material _material;
public bool bNormal=false;


	// Use this for initialization
    void Start()
    {
        _material = gameObject.GetComponent<Renderer>().material;
        _texOffset = _material.GetTextureOffset("_MainTex");
    }
	
	// Update is called once per frame
	void Update () {

        _texOffset.x += Time.smoothDeltaTime * Power.x;
        _texOffset.y += Time.smoothDeltaTime * Power.y;
        _normaltexOffset.x += Time.smoothDeltaTime * Power_Normal.x;
        _normaltexOffset.y += Time.smoothDeltaTime * Power_Normal.y;

        _material.SetTextureOffset("_MainTex", _texOffset);
        if (_texOffset.x <= -1)
        {
            _texOffset.x = 0;
        }

	if(bNormal)
{
        _material.SetTextureOffset("_BumpMap", _normaltexOffset);
        if (_normaltexOffset.x <= -1)
        {
            _normaltexOffset.x = 0;
        }
}
    }
}
