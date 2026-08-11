using Microsoft.Win32;
using UnityEngine;

public class RegistryManager : MonoBehaviour
{
    // 예시 레지스트리 경로: HKEY_CURRENT_USER\Software\MyUnityApp
    private static string RegistryPath = @"Software\Applesoft";

    public static void SetRegistryPath(string path)
    {
        RegistryPath = path;
    }

    // 값 저장
    public static void SaveValue(string key, string value)
    {
        RegistryKey regKey = Registry.CurrentUser.CreateSubKey(RegistryPath);
        if (regKey != null)
        {
            regKey.SetValue(key, value);
            regKey.Close();
            //            Debug.Log($"Saved '{key}' = '{value}' to registry.");
        }
    }

    // 값 불러오기
    public static string LoadValue(string key, string defaultValue = "")
    {
        RegistryKey regKey = Registry.CurrentUser.OpenSubKey(RegistryPath);
        if (regKey != null)
        {
            object val = regKey.GetValue(key);
            regKey.Close();
            return val != null ? val.ToString() : defaultValue;
        }
        return defaultValue;
    }

    // 값 삭제
    public static void DeleteValue(string key)
    {
        RegistryKey regKey = Registry.CurrentUser.OpenSubKey(RegistryPath, writable: true);
        if (regKey != null)
        {
            regKey.DeleteValue(key, false);
            regKey.Close();
            Debug.Log($"Deleted key '{key}' from registry.");
        }
    }
}