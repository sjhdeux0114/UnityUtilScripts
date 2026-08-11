using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class SjhUtilEditor{

    [MenuItem("Tools/PlayerPref삭제")]
    private static void ClearPlayerPref()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPref Delete All !");

    }
    
    [MenuItem("Tools/모든 컴포넌트 삭제")]
    private static void ClearComponent()
    {
        GameObject[] g = Selection.gameObjects;
        for(int i=0;i<g.Length;i++)
        {
            Component[] objs = g[i].GetComponents(typeof(Component));
            foreach (Component c in objs)
            {
                DestroyObj.DestroyImmediate(c);
            }
        }

        g = Selection.gameObjects;
        for (int i = 0; i < g.Length; i++)
        {
            Component[] objs = g[i].GetComponents(typeof(Component));
            foreach (Component c in objs)
            {
                DestroyObj.DestroyImmediate(c);
            }
        }

    }
    [MenuItem("Tools/잘못된 컴포넌트 씬 검색")]
    private static void FindInSelected()
    {
        Transform[] go = GameObject.FindObjectsByType<Transform>(FindObjectsSortMode.None);
        int go_count = 0, components_count = 0, missing_count = 0;
        foreach (Transform t in go)
        {
            go_count++;
            Component[] components = t.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                components_count++;
                if (components[i] == null)
                {
                    missing_count++;
                    string s = t.name;
                    Transform tt = t.transform;
                    while (tt.parent != null)
                    {
                        s = tt.parent.name + "/" + s;
                        tt = tt.parent;
                    }
                    Debug.Log(s + " has an empty script attached in position: " + i, t);
                }
            }
        }

        Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} missing", go_count, components_count, missing_count));
    }

    static int missing_count = 0;
    [MenuItem("Tools/잘못된 컴포넌트 오브젝트 검색")]
    private static void FindInSelectedObject()
    {
        GameObject[] go = Selection.gameObjects;

        int go_count = 0, components_count = 0;
        missing_count = 0;
        foreach (GameObject g in go)
        {
            go_count++;
            Component[] components = g.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                components_count++;
                if (components[i] == null)
                {
                    missing_count++;
                    string s = g.name;
                    Transform t = g.transform;
                    while (t.parent != null)
                    {
                        s = t.parent.name + "/" + s;
                        t = t.parent;
                    }
                    Debug.Log(s + " has an empty script attached in position: " + i, g);
                }
            }

            if(g.transform.childCount > 0)
            {
                for(int i=0;i< g.transform.childCount;i++)
                    FindInSelectedObject(g.transform.GetChild(i).gameObject);
            }
        }

        Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} missing", go_count, components_count, missing_count));
        
    }

    private static void FindInSelectedObject(GameObject obj)
    {
        int go_count = 0, components_count = 0;
        go_count++;
        Component[] components = obj.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            components_count++;
            if (components[i] == null)
            {
                missing_count++;
                string s = obj.name;
                Transform t = obj.transform;
                while (t.parent != null)
                {
                    s = t.parent.name + "/" + s;
                    t = t.parent;
                }
                Debug.Log(s + " has an empty script attached in position: " + i, obj);
            }
        }

        if (obj.transform.childCount > 0)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
                FindInSelectedObject(obj.transform.GetChild(i).gameObject);
        }

    }
}
