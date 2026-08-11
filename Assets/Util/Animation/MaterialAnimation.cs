using System.Collections;
using System.Collections.Generic;
using UnityEngine;




[System.Serializable]
public class MaterialGroup
{
    public string mat_Name;
    public AnimationCurve ValueCurve;
    public float AnimationTime;
}

public class MaterialAnimation : MonoBehaviour
{
    public Material mat;
    public MaterialGroup[] materialGroups;



    // Start is called before the first frame update
    void Start()
    {
        if (mat == null)
        {
            var renderer = GetComponent<Renderer>();

            if (renderer == null)
            {
                UnityEngine.UI.Image img = GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                {
                    mat = img.material;
                }
            }
            else if (renderer != null)
            {
                mat = renderer.material;
            }
        }

    }

    [ContextMenu("Test Play 0")]
    public void TestPlay0()
    {
        StartCoroutine(AniMat_Proc(0));
    }
    [ContextMenu("Test Play 1")]
    public void TestPlay1()
    {
        StartCoroutine(AniMat_Proc(1));
    }
    [ContextMenu("Test Play 2")]
    public void TestPlay2()
    {
        StartCoroutine(AniMat_Proc(2));
    }

    public void PlayAnimation(int index)
    {
        if (mat == null) return;
        StartCoroutine(AniMat_Proc(index));
    }

    public IEnumerator AniMat_Proc(int index)
    {
        MaterialGroup group = materialGroups[index];

        float time = 0;
        float maxTime = group.AnimationTime;

        while (time < maxTime)
        {
            time += Time.deltaTime;
            float per = Mathf.Clamp01(time / maxTime);
            mat.SetFloat(group.mat_Name, group.ValueCurve.Evaluate(per));
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
