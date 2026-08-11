using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;


/// <summary>
/// 인터넷을 통해 서버 시간(GMT)을 받아와 한국시간으로 변환하는 유틸리티
/// </summary>
public class InternetTime : MonoBehaviour
{
    [Tooltip("시간을 가져올 HTTP 서버 주소 (HEAD에서 'Date' 헤더를 읽음)")]
    public string serverUrl = "https://www.google.com";

    [Tooltip("가져온 시간 (yyyy-MM-dd)")]
    public string worldTime;

    [field: SerializeField]
    public bool isLoaded { get; private set; } = false;

    public event Action<DateTime> OnTimeReceived;

    void Start()
    {
        StartCoroutine(GetInternetTime());
    }

    public IEnumerator GetInternetTime()
    {
        isLoaded = false;
        worldTime = string.Empty;

        using (UnityWebRequest request = UnityWebRequest.Head(serverUrl))
        {
            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                Debug.LogWarning("[InternetTime] Failed to fetch time: " + request.error);
                worldTime = DateTime.Now.ToString("yyyy-MM-dd");
                isLoaded = true;
                yield break;
            }

            string dateHeader = request.GetResponseHeader("date");
            if (!string.IsNullOrEmpty(dateHeader))
            {
                DateTime serverTime = DateTime.Parse(dateHeader).ToLocalTime();
                worldTime = serverTime.ToString("yyyy-MM-dd");
                OnTimeReceived?.Invoke(serverTime);
            }
            else
            {
                Debug.LogWarning("[InternetTime] 'date' header not found.");
                worldTime = DateTime.Now.ToString("yyyy-MM-dd");
            }
        }

        isLoaded = true;
    }
}

