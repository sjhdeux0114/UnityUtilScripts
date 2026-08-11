using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class FND_Image : MonoBehaviour
{

    public FND_SpriteInfo sprInfo;
    public GameObject img_Object;
    public float sizeScale;
    public Color color;
    public Color[] RndColor;
    [SerializeField]
    bool _RandomColor;
    public bool RandomColor
    {
        get { return _RandomColor; }
        set 
        { 
            _RandomColor = value;
            updateFND();
        }
    }
    public List<FND_ImageData> imgData = new List<FND_ImageData>();
    public string ToStringCode = "D4";
    [SerializeField]
    string _str_value;

    public string str_value
    {
        get { return _str_value; }
        set
        {
            _str_value = value;
            updateFND();

        }
    }
    [SerializeField]
    int _value;
    public int value
    {
        get { return _value; }
        set
        {
            if (_value != value)
            {
                _value = value;
                str_value = _value.ToString(ToStringCode);
                updateFND();
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Create_FndData()
    {
        int make_n = str_value.Length - imgData.Count;

        for (int i = 0; i < make_n; i++)
        {
            GameObject g = Instantiate(img_Object, transform);

            FND_ImageData fnd = g.GetComponent<FND_ImageData>();
            fnd.Init(sprInfo);
            fnd.Set(null,Color.white);
            imgData.Add(fnd);

        }

    }
    [InspectorButton]
    void Init()
    {
        foreach(var img in imgData)
        {
            DestroyImmediate(img.gameObject);
        }
        imgData.Clear();
        str_value = _value.ToString();
    }
    [InspectorButton]
    void Inscetor_Update()
    {
        str_value = _value.ToString(ToStringCode);

    }
    [InspectorButton]
    void InscetorString_Update()
    {
        str_value = _str_value;

    }
    public void updateFND()
    {
        if (!img_Object) return;

        if (imgData.Count < str_value.Length)
        {
            Create_FndData();
        }

        for(int i=0; i<str_value.Length;i++)
        {
            string c = str_value[i].ToString();
            FND_Dataset data = sprInfo.fndData.GetData(c);
            Color ret_Col = color;
            if (RandomColor)
            {
                ret_Col = RndColor[Random.Range(0, RndColor.Length)];
            }
            imgData[i].Set(data, ret_Col);
            imgData[i].transform.localPosition = new Vector3(i * sprInfo.offset_x, 0, 0);
        }
        transform.localScale = Vector3.one * sizeScale;
    }


    // Update is called once per frame
    void Update()
    {
        if(RandomColor)
        {
            if (RndColor.Length > 0)
            {
                for (int i = 0; i < str_value.Length; i++)
                {
                    imgData[i].SetColor(RndColor[Random.Range(0, RndColor.Length)]);
                }
            }
        }
        
    }
}
