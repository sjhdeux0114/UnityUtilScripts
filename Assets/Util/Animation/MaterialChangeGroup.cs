using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MaterialData
{
    public string materialName;
    public float times;
    public bool bValue = true;
    public float dataValue;
    public bool bColor=false;
    public Color color;
    public AnimationCurve timeCurve;
}

[System.Serializable]
public class MaterialChangeGroupData
{
    public Material material;
    public MaterialData[] materialData;

    public void ChangeMaterial()
    {
        for (int i = 0; i < materialData.Length; i++)
        {
            float times = Time.time % materialData[i].times;

            float t = materialData[i].timeCurve.Evaluate(times / materialData[i].times);

            float ret_value = materialData[i].dataValue * t;


            if(materialData[i].bValue)
                material.SetFloat(materialData[i].materialName, ret_value);
            if(materialData[i].bColor)
                material.SetColor(materialData[i].materialName, materialData[i].color);
        }
    }
}

public class MaterialChangeGroup : MonoBehaviour
{
    public MaterialChangeGroupData[] materialChangeGroupData;
    public int Index;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetIndex(int index)
    {
        Index = index;
        materialChangeGroupData[Index].ChangeMaterial();

    }


    [InspectorButton("Set_0")]
    public void Set_0()
    {
        SetIndex(0);
    }

    [InspectorButton("Set_1")]
    public void Set_1()
    {
        SetIndex(1);
    }
    [InspectorButton("Set_2")]
    public void Set_2()
    {
        SetIndex(2);
    }
    [InspectorButton("Set_3")]
    public void Set_3()
    {
        SetIndex(3);
    }
    [InspectorButton("Set_4")]
    public void Set_4()
    {
        SetIndex(4);
    }
    // Update is called once per frame
    void Update()
    {
        materialChangeGroupData[Index].ChangeMaterial();
        
    }
}
