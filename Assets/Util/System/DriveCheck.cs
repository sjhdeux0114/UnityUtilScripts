using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class DriveCheck : MonoBehaviour
{

    public string[] Drive_Name;
    public string[] Drive_Type;
    public UnityEvent _Event;
    int Old_Num = 0;

    bool bQuit = false;


    // Use this for initialization
    void Start()
    {

        bQuit = false;
        Debug.Log($"{Application.productName}");

        _UpdateDriveInfo();

    }

    void _UpdateDriveInfo()
    {
#if UNITY_EDITOR
        if (Editor_Test)
        {
            return;
        }
#endif
        var dInfo = DriveInfo.GetDrives();

        Drive_Name = new string[dInfo.Length];
        Drive_Type = new string[dInfo.Length];
        bQuit = true;

        for (int i = 0; i < Drive_Name.Length; i++)
        {
            Drive_Name[i] = dInfo[i].Name;
            Drive_Type[i] = dInfo[i].DriveType.ToString();

            if (dInfo[i].DriveType == DriveType.CDRom)
            {


                DirectoryInfo di = new DirectoryInfo($"{dInfo[i].Name}{Application.productName}");
                if (!di.Exists)
                {
                    //Debug.Log($"Not found - {dInfo[i].Name}{Application.productName}");
                    //                    bQuit = true;
                }
                else
                {
                    bQuit = false;
                }
            }
        }

        if (bQuit)
        {
#if UNITY_EDITOR
            if (!Editor_Test)
            {
                Debug.Log($"cd Not found ");
                Editor_Test = true;
            }
#else
            Debug.Log($"cd Not found ");
#endif
        }

        Old_Num = Directory.GetLogicalDrives().Length;
    }
#if UNITY_EDITOR
    bool Editor_Test = false;
#endif

    float t = 0;
    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

#if USE_TEST_MODE
        return;
#endif
        t += Time.unscaledDeltaTime;
        if (t >= 1)
        {
            t = 0;
            _UpdateDriveInfo();
            _Event.Invoke();
        }

        if (bQuit)
            Application.Quit();

    }
}
