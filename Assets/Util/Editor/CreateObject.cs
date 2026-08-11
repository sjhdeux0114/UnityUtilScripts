using System.Collections;
using UnityEditor;
using UnityEngine;


class CreateObject : EditorWindow
{

    GameObject SelObj;
    GameObject ParentObj;
    float Scale_min;
    float Scale_max;
    Vector3 Rotation;
    bool RandomRotate;
    Vector3 Random_Rotation_Min;
    Vector3 Random_Rotation_Max;
    double old_time = 0;
    GameObject OldObject;

    [MenuItem("Window/Create Obj")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(CreateObject));
    }

    public void OnEnable()
    {
        SceneView.duringSceneGui += SceneUpdate;
    }

    public void OnDisable()
    {
        SceneView.duringSceneGui -= SceneUpdate;
    }
    void OnGUI()
    {
        GUILayout.Label("Select Prefab", EditorStyles.boldLabel);


        SelObj = EditorGUILayout.ObjectField("Select", SelObj, typeof(Object), false) as GameObject;

        ParentObj = EditorGUILayout.ObjectField("Parent Object", ParentObj, typeof(Object), true) as GameObject;

        Scale_min = EditorGUILayout.FloatField("Min Scale", Scale_min);
        Scale_max = EditorGUILayout.FloatField("Max Scale", Scale_max);

        Rotation = EditorGUILayout.Vector3Field("Rotation", Rotation);

        RandomRotate = EditorGUILayout.Toggle("RandomRotate", RandomRotate);

        Random_Rotation_Min = EditorGUILayout.Vector3Field("Random min", Random_Rotation_Min);
        Random_Rotation_Max = EditorGUILayout.Vector3Field("Random max", Random_Rotation_Max);

    }

    void SceneUpdate(SceneView sceneview)
    {
        Event e = Event.current;
        if (e.keyCode == KeyCode.Q)
        {
            double t = EditorApplication.timeSinceStartup;

            GameObject obj;
            if (SelObj && (t >= old_time || Random.Range(0, 100) < 6))
            {
                old_time = t + 1;
                obj = (GameObject)Instantiate(SelObj);

                Vector3 position = new Vector3(0, 0, 0);
                Vector2 mouse = Event.current.mousePosition;
                mouse.y = sceneview.camera.pixelHeight - mouse.y;
                Ray ray = Camera.current.ScreenPointToRay(mouse);
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(ray, out hit, 1000.0f))
                {
                    position = hit.point;
                    //                    t.rotation = Quaternion.FromToRotation(t.up, hit.normal) * t.rotation;
                }
                obj.transform.position = position;

                if (ParentObj)
                {
                    obj.transform.parent = ParentObj.transform;

                }

                float scale = Random.Range(Scale_min, Scale_max);

                obj.transform.localScale = new Vector3(obj.transform.localScale.x * scale, obj.transform.localScale.y * scale, obj.transform.localScale.z * scale);

                if (RandomRotate)
                {
                    Vector3 rot = new Vector3(Random.Range(Random_Rotation_Min.x, Random_Rotation_Max.x),
                        Random.Range(Random_Rotation_Min.y, Random_Rotation_Max.y),
                        Random.Range(Random_Rotation_Min.z, Random_Rotation_Max.z));

                    obj.transform.eulerAngles = rot;
                }
                else
                {
                    obj.transform.eulerAngles = Rotation;
                }

                OldObject = obj;
                Selection.activeGameObject = obj;

            }
        }
        else if (e.shift)
        {
            if (e.keyCode == KeyCode.D)
            {
                if (OldObject)
                {
                    DestroyImmediate(OldObject);
                    OldObject = null;
                }
            }
        }
        else if (e.isKey && e.character == 'd')
        {
            GameObject obj;
            if (SelObj)
            {
                obj = (GameObject)Instantiate(SelObj);

                Vector3 position = new Vector3(0, 0, 0);
                Vector2 mouse = Event.current.mousePosition;
                mouse.y = sceneview.camera.pixelHeight - mouse.y;
                Ray ray = Camera.current.ScreenPointToRay(mouse);
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(ray, out hit, 1000.0f))
                {
                    position = hit.point;
                    //                    t.rotation = Quaternion.FromToRotation(t.up, hit.normal) * t.rotation;
                }
                obj.transform.position = position;

                if (ParentObj)
                {
                    obj.transform.parent = ParentObj.transform;

                }

                float scale = Random.Range(Scale_min, Scale_max);

                obj.transform.localScale = new Vector3(obj.transform.localScale.x * scale, obj.transform.localScale.y * scale, obj.transform.localScale.z * scale);

                if (RandomRotate)
                {
                    Vector3 rot = new Vector3(Random.Range(Random_Rotation_Min.x, Random_Rotation_Max.x),
                        Random.Range(Random_Rotation_Min.y, Random_Rotation_Max.y),
                        Random.Range(Random_Rotation_Min.z, Random_Rotation_Max.z));

                    obj.transform.eulerAngles = rot;
                }
                else
                {
                    obj.transform.eulerAngles = Rotation;
                }

                OldObject = obj;
                Selection.activeGameObject = obj;

            }

        }
    }

    void Update()
    {
    }
}