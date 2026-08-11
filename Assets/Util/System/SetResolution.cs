using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.InteropServices;

public class SetResolution : MonoBehaviour {
#if UNITY_IOS || UNITY_ANDROID
#else
    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    private static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern IntPtr FindWindow(System.String className, System.String windowName);


    [DllImport("user32.dll")]
    internal static extern IntPtr SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
#endif
    private void GetFocusWin()
    {
#if UNITY_IOS || UNITY_ANDROID
#else
        // get hWnd, nb. hWnd always seems to return 0 with this method
        // http://stackoverflow.com/questions/7357675/how-can-i-set-the-focus-to-application-which-is-allready-in-running-state?rq=1
        //System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
        //IntPtr hWnd = currentProcess.MainWindowHandle;

        // get hWnd, nb. this works ok on Windows 7 x64
        // http://ronniediaz.com/2011/05/03/start-a-process-in-the-foreground-in-c-net-without-appactivate/
        IntPtr hWnd = FindWindow(null, "GWANG");


        if (hWnd != IntPtr.Zero)
        {
            Debug.Log("GetFocusWin() setting foreground window, showing window SW_SHOWDEFAULT");

            SetForegroundWindow(hWnd);
            // enum values from https://msdn.microsoft.com/en-us/library/windows/desktop/ms633548(v=vs.85).aspx
            ShowWindow(hWnd, 10); // SW_SHOWDEFAULT = 10, SW_MAXIMIZE = 3, SW_SHOW = 5

        }
        else
        {
            Debug.LogWarning("GetFocusWin() failed, hWnd is 0!");
        }
#endif
    }

    public void SetPosition(int x, int y, int resX = 0, int resY = 0)
    {
#if UNITY_IOS || UNITY_ANDROID
#else
        SetWindowPos(FindWindow(null, TitleName), 0, x, y, resX, resY, resX * resY == 0 ? 1 : 0);
#endif
    }

    public string TitleName;

    public bool bFull = false;
    public float Delays = 3;
    public int MonitorNum = 0;

    public Vector2[] Resolution_Wants;

    // Use this for initialization
    IEnumerator Start () {
        string FileNames = "Resolution.txt";

        if (File.Exists(FileNames))
        {
            StreamReader sr = new StreamReader(FileNames);
            string[] data = sr.ReadLine().Split(',');
            sr.Close();

            if(data.Length >= 2)
            {
                int tmp = 0;
                if(int.TryParse(data[0],out tmp))
                {
                    Resolution_Wants[0].x = tmp;
                }
                if (int.TryParse(data[1], out tmp))
                {
                    Resolution_Wants[0].y = tmp;
                }
            }
        }
        
#if UNITY_EDITOR
        yield break;
#else

        yield return new WaitForSeconds(Delays);

        GetFocusWin();
        yield return new WaitForSeconds(1.0f);
        FindResolution();

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif
    }

    void FindResolution()
    {
       
        Resolution[] resolutions = Screen.resolutions;
        Debug.Log(string.Format("current res:{0}/{1}", Screen.currentResolution.width, Screen.currentResolution.height));
        for (int i = 0; i < resolutions.Length; i++)
        {
//            Debug.Log(string.Format("{2} - support res:{0}/{1}", resolutions[i].width, resolutions[i].height, (i + 1)));
        }

        float w = Screen.currentResolution.width;
        float h = Screen.currentResolution.height;

        for (int j = 0; j < Resolution_Wants.Length; j++)
        {
//            Debug.Log(string.Format("find:{0}/{1}", Resolution_Wants[j].x, Resolution_Wants[j].y));
            for (int i = 0; i < resolutions.Length; i++)
            {
//                Debug.Log(string.Format("search res:{0}/{1}", resolutions[i].width, resolutions[i].height));
                if (Mathf.Abs(resolutions[i].width - Resolution_Wants[j].x) <= 10 && Mathf.Abs(resolutions[i].height - Resolution_Wants[j].y) <= 10)
                {
                    Debug.Log(string.Format("search res:{0}/{1}", resolutions[i].width, resolutions[i].height));
                    w = Resolution_Wants[j].x;
                    h = Resolution_Wants[j].y;

                    Screen.SetResolution((int)w, (int)h, bFull);
                    return;
                }
            }
        }


    }

    // Update is called once per frame
    void Update () {

        


    }


}
