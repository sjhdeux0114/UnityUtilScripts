
// =============================================================
// GifLoader.cs — load bytes from Resources or StreamingAssets
// =============================================================
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public static class GifLoader
{
    // Resources: path without extension, e.g. "gifs/cat"
    public static byte[] LoadFromResources(string resourcesPath)
    {
        TextAsset ta = Resources.Load<TextAsset>(resourcesPath);
        return ta != null ? ta.bytes : null;
    }

    // StreamingAssets: use coroutine because on Android it’s inside APK
    public static IEnumerator LoadFromStreamingAssets(string relativePath, System.Action<byte[]> onDone)
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, relativePath);
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var req = UnityWebRequest.Get(path))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
                onDone?.Invoke(req.downloadHandler.data);
            else
                onDone?.Invoke(null);
        }
#else
        // Desktop/iOS editor path can be read directly
        byte[] data = File.Exists(path) ? File.ReadAllBytes(path) : null;
        onDone?.Invoke(data);
        yield break;
#endif
    }
}
