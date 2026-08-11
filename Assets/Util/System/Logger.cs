using System;
using System.IO;
using UnityEngine;

public static class Logger
{
    public static bool bLog = true;
    private static string logFilePath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), $"log{System.DateTime.Now.ToString("d-HH-mm-ss")}.txt");

    public static void Log(string message)
    {
        if (!bLog)
        {
            Debug.Log($"[DEBUG-ONLY] {message}");
            return;
        }
        if (message.Contains(">>OIDD"))
        {
            Debug.Log($"[OIDD-LOG] {message}");
            AppendToFile($"[OIDD-LOG] {message}");
        }
        else if (message.Contains(">>EV"))
        {
            Debug.Log($"[EVENT-LOG] {message}");
            AppendToFile($"[EVENT-LOG] {message}");
        }
        else if (message.Contains(">>SYS"))
        {
            Debug.Log($"[SYS-LOG] {message}");
            AppendToFile($"[SYS-LOG] {message}");
        }
        else
        {

            Debug.Log($"[LOG] {message}");
            AppendToFile($"[LOG] {message}");
        }
    }

    public static void Warn(string message)
    {
        if (!bLog)
        {
            Debug.Log("[DEBUG-ONLY] " + message);
            return;
        }
        Debug.LogWarning("[WARN] " + message);
        AppendToFile("[WARN] " + message);
    }

    public static void Error(string message)
    {
        if (!bLog)
        {
            Debug.Log("[DEBUG-ONLY] " + message);
            return;
        }
        Debug.LogError("[ERROR] " + message);
        AppendToFile("[ERROR] " + message);
    }

    private static void AppendToFile(string message)
    {
        try
        {
            File.AppendAllText(logFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}\n");
        }
        catch (Exception ex)
        {
            Debug.LogError("[Logger] Failed to write to log file: " + ex.Message);
        }
    }

}