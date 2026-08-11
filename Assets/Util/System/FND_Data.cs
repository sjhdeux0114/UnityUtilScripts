using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FND_Dataset
{
    public string character;
    public bool up;
    public bool up_left;
    public bool up_right;
    public bool mid;
    public bool bottom_left;
    public bool bottom_right;
    public bool bottom;
}

[CreateAssetMenu(fileName = "FND", menuName = "SJHDeux/UI/FNDData")]
public class FND_Data : ScriptableObject
{
    public FND_Dataset[] datasets;
    public Dictionary<string, FND_Dataset> Dic_FND_Data = new Dictionary<string, FND_Dataset>();
    
    public void Init()
    {
        Dic_FND_Data = new Dictionary<string, FND_Dataset>();
        foreach (var data in datasets)
        {
            Dic_FND_Data.Add(data.character, data);
        }
    }
    public FND_Dataset GetData(string character)
    {
        if (Dic_FND_Data.Count == 0)
        {
            Init();
        }
        if (Dic_FND_Data.ContainsKey(character))
        {
            return Dic_FND_Data[character];
        }
        return null;
    }

}
