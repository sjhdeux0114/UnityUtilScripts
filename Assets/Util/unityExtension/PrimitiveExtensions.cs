using System;
using System.Collections.Generic;
using UnityEngine;

public static class PrimitiveExtensions
{
    // ==========================================
    // A. float / int (숫자 관련)
    // ==========================================

    /// <summary> % 확률을 계산합니다. 예: 35f.PercentChance() -> 35% 확률로 true 반환 </summary>
    public static bool PercentChance(this float chance)
    {
        return UnityEngine.Random.Range(0f, 100f) <= chance;
    }

    /// <summary> % 확률을 계산합니다. (int 버전) </summary>
    public static bool PercentChance(this int chance)
    {
        return UnityEngine.Random.Range(0, 100) < chance;
    }


    public static string ToTimeFormat(this float seconds, string text1 = "", string text2 = "",
                                string splitChar = ":", bool ZeroFull = true)
    {
        if (seconds < 0) seconds = 0;
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        if (ZeroFull)
            return string.Format("{0:00}{1}{2:00}{3}", minutes, text1, splitChar, text2, secs);
        else
            return string.Format("{0}{1}{2}{3}", minutes, text1, splitChar, text2, secs);
    }

    /// <summary> 값이 최소~최대 범위 내에 있는지 확인합니다. </summary>
    public static bool IsBetween(this float value, float min, float max)
    {
        return value >= min && value <= max;
    }

    /// <summary> 00000000 구조의 레이어 마스크(LayerMask)에 해당 레이어(int)가 포함되어 있는지 확인 </summary>
    public static bool ContainsLayer(this LayerMask mask, int layer)
    {
        return (mask & (1 << layer)) != 0;
    }


    // ==========================================
    // B. string (문자열 관련)
    // ==========================================

    /// <summary> string.IsNullOrEmpty(str)를 매번 쓰는 게 귀찮을 때 숏컷 </summary>
    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    /// <summary> 문자열이 비어있지 않은지 검사 (if(!str.IsNullOrEmpty()) 대신 사용) </summary>
    public static bool IsPresent(this string str)
    {
        return !string.IsNullOrEmpty(str);
    }

    /// <summary> 문자열을 유니티 컬러(Color)로 변환합니다. 예: "#FF0000".ToColor() </summary>
    public static Color ToColor(this string hexString)
    {
        if (ColorUtility.TryParseHtmlString(hexString, out Color color))
        {
            return color;
        }
        Debug.LogWarning($"Color 변환 실패: {hexString}");
        return Color.white; // 실패 시 기본 흰색 반환
    }

    // ==========================================
    // 1. float / int (수학 및 연산 심화)
    // ==========================================

    /// <summary> 값을 특정 범위 내로 제한(Clamp)합니다. 예: 평점.Clamp(0, 100); </summary>
    public static float Clamp(this float value, float min, float max) => Mathf.Clamp(value, min, max);
    public static int Clamp(this int value, int min, int max) => Mathf.Clamp(value, min, max);

    /// <summary> 0부터 (현재 값 - 1) 사이의 무작위 정수를 반환합니다. 예: 10.Roll() -> 0~9 중 무작위 </summary>
    public static int Roll(this int maxValue) => UnityEngine.Random.Range(0, maxValue);

    /// <summary> 현재 값에서 목표 값까지 부드럽게 보간(Lerp)합니다. 주로 Update문에서 가속도 연산할 때 씁니다. </summary>
    public static float LerpTo(this float current, float target, float t) => Mathf.Lerp(current, target, t);

    /// <summary> 숫자가 짝수인지 확인합니다. </summary>
    public static bool IsEven(this int value) => value % 2 == 0;

    /// <summary> 숫자가 홀수인지 확인합니다. </summary>
    public static bool IsOdd(this int value) => value % 2 != 0;


    // ==========================================
    // 2. string (텍스트 파싱 및 조작 심화)
    // ==========================================

    /// <summary> 문자열을 int로 안전하게 변환합니다. 실패 시 기본값(default)을 반환합니다. </summary>
    public static int ToInt(this string str, int defaultValue = 0)
    {
        return int.TryParse(str, out int result) ? result : defaultValue;
    }

    /// <summary> 문자열을 float로 안전하게 변환합니다. </summary>
    public static float ToFloat(this string str, float defaultValue = 0f)
    {
        return float.TryParse(str, out float result) ? result : defaultValue;
    }

    /// <summary> 문자열을 Enum 타입으로 안전하게 변환합니다. 예: "Fire".ToEnum<ItemType>() </summary>
    public static T ToEnum<T>(this string str) where T : struct, Enum
    {
        if (Enum.TryParse(str, true, out T result)) return result;
        Debug.LogWarning($"{str}을 {typeof(T).Name} Enum으로 변환하는 데 실패했습니다.");
        return default;
    }


    // ==========================================
    // 3. bool / 컬렉션 (논리 및 배열 심화)
    // ==========================================

    /// <summary> 상태를 반대로 뒤집고 바뀐 값을 반환합니다. 예: isOpen.Toggle(); (true->false) </summary>
    public static bool Toggle(this ref bool value)
    {
        value = !value;
        return value;
    }

    /// <summary> 배열이나 리스트에서 무작위로 아이템 하나를 뽑아 반환합니다. (가장 많이 씀) </summary>
    public static T GetRandom<T>(this IList<T> list)
    {
        if (list == null || list.Count == 0) return default;
        return list[UnityEngine.Random.Range(0, list.Count)];
    }


    // ==========================================
    // 4. Vector (유니티 구조체 보완 숏컷)
    // ==========================================

    /// <summary> 두 좌표 사이의 거리를 계산합니다. 기존 Vector3.Distance(a, b)의 숏컷 버전 </summary>
    public static float DistanceTo(this Vector3 origin, Vector3 destination)
    {
        return Vector3.Distance(origin, destination);
    }

    /// <summary> Vector3의 X, Y값만 남기고 Z를 0으로 만듭니다. (2D 게임 좌표 연산용) </summary>
    public static Vector3 ToVector2D(this Vector3 vector)
    {
        return new Vector3(vector.x, vector.y, 0f);
    }
}