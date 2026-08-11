using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class _RND_COUNT
{
    public int min;
    public int max;
    public int TargetCnt;
    public int Setup()
    {
        TargetCnt = UnityEngine.Random.Range(min, max);
        return TargetCnt;
    }
    public int Count(int n = 1)
    {
        TargetCnt -= n;
        return TargetCnt;
    }
    public bool Check_End()
    {
        if (TargetCnt <= 0)
            return true;
        else
            return false;
    }
    public _RND_COUNT(int mn, int mx)
    {
        min = mn;
        max = mx;
    }
}
[System.Serializable]
public class _RND_COUNT_F
{
    public float min;
    public float max;
    public float TargetCnt;
    public float Setup()
    {
        TargetCnt = UnityEngine.Random.Range(min, max);
        return TargetCnt;
    }
    public float StartSetup()
    {
        TargetCnt = UnityEngine.Random.Range(0, max - min);
        return TargetCnt;
    }
    public float Count(float f = 1)
    {
        TargetCnt -= f;
        return TargetCnt;
    }
    public bool Check_End()
    {
        if (TargetCnt < 0)
            return true;
        else
            return false;
    }
    public _RND_COUNT_F(float mn, float mx)
    {
        min = mn;
        max = mx;
    }
}
[System.Serializable]
public class _RND_TIME
{
    public float min;
    public float max;
    public float TargetTime;
    public bool bAct;
    public float Setup()
    {
        bAct = true;
        float rnd = UnityEngine.Random.Range(min, max);
        TargetTime = Time.time + rnd;
        return rnd;
    }
    public bool Check_End()
    {
        if (TargetTime <= Time.time && bAct)
        {
            bAct = false;
            return true;
        }
        else
            return false;
    }
    public _RND_TIME(float mn, float mx)
    {
        min = mn;
        max = mx;
    }
}

public class RandomPure
{
    private List<int> _unusedIndices = new List<int>();
    private int _size;

    public void Init(int size)
    {
        _size = size;
        ResetAndShuffleIndices();
    }

    public int Rand()
    {
        if (_size <= 0) return -1;

        if (_unusedIndices.Count == 0)
        {
            ResetAndShuffleIndices();
        }

        if (_unusedIndices.Count == 0) return -1;

        int index = _unusedIndices[_unusedIndices.Count - 1];
        _unusedIndices.RemoveAt(_unusedIndices.Count - 1);
        return index;
    }

    private void ResetAndShuffleIndices()
    {
        _unusedIndices.Clear();
        for (int i = 0; i < _size; i++)
        {
            _unusedIndices.Add(i);
        }

        for (int i = _unusedIndices.Count - 1; i > 0; i--)
        {
            int r = UnityEngine.Random.Range(0, i + 1);
            int temp = _unusedIndices[i];
            _unusedIndices[i] = _unusedIndices[r];
            _unusedIndices[r] = temp;
        }
    }
}

public static class RandomUtil
{
    public static int GetRandomInt(int min, int max)
    {
        return Random.Range(min, max + 1); // max ����
    }

    public static float GetRandomFloat(float min, float max)
    {
        return Random.Range(min, max); // max ������
    }

    public static Vector2 GetRandomVector2(Vector2 min, Vector2 max)
    {
        return new Vector2(GetRandomFloat(min.x, max.x), GetRandomFloat(min.y, max.y));
    }

    public static Vector3 GetRandomVector3(Vector3 min, Vector3 max)
    {
        return new Vector3(GetRandomFloat(min.x, max.x), GetRandomFloat(min.y, max.y), GetRandomFloat(min.z, max.z));
    }

    public static T GetRandomEnum<T>(T min, T max) where T : System.Enum
    {
        int minValue = System.Convert.ToInt32(min);
        int maxValue = System.Convert.ToInt32(max);
        return (T)(object)Random.Range(minValue, maxValue + 1);
    }

    public static int GetRandomIndexByWeight(int[] weights)
    {
        if (weights == null || weights.Length == 0)
            return -1;

        int total = weights.Sum();
        int rnd = Random.Range(0, total);

        int accum = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            accum += weights[i];
            if (rnd < accum)
                return i;
        }

        return -1;
    }

    public static T GetRandomIndexByWeight<T>(int[] weights)
    {
        if (weights == null || weights.Length == 0)
            return default(T);

        int total = weights.Sum();
        int rnd = Random.Range(0, total);

        int accum = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            accum += weights[i];
            if (rnd < accum)
                return (T)(object)i;
        }

        return default(T);
    }

    /// <summary>
    /// Enum ���� �׿� �ش��ϴ� ����ġ �迭�� ������� Enum �� �ϳ��� ���� ����
    /// </summary>
    public static T GetRandomEnumByWeight<T>(T[] enumValues, int[] weights) where T : System.Enum
    {
        if (enumValues == null || weights == null || enumValues.Length != weights.Length || enumValues.Length == 0)
            throw new System.ArgumentException("Enum values and weights must be non-null and of the same non-zero length");

        int index = GetRandomIndexByWeight(weights);
        return enumValues[index];
    }
}

public static class MathUtil
{
    public static float Get_Per(float t, float max) => Mathf.Approximately(max, 0f) ? 0f : t / max;
    public static float Get_PerClamped(float t, float max) => Mathf.Approximately(max, 0f) ? 0f : Mathf.Clamp01(t / max);
    public static bool Approximately(float a, float b, float tolerance = 0.0001f) => Mathf.Abs(a - b) < tolerance;
    public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax) => Mathf.Lerp(toMin, toMax, Mathf.InverseLerp(fromMin, fromMax, value));
    public static Vector3 ClampVector(Vector3 value, Vector3 min, Vector3 max) => Vector3.Min(Vector3.Max(value, min), max);
    public static float NormalizeAngle(float angle) => (angle % 360 + 360) % 360;

    public static float Snap(float value, float step) => Mathf.Round(value / step) * step;
    public static Vector3 Snap(Vector3 value, float step) => new Vector3(Snap(value.x, step), Snap(value.y, step), Snap(value.z, step));
    public static float Repeat(float value, float length) => value - Mathf.Floor(value / length) * length;
    public static float PingPong(float value, float length) => length - Mathf.Abs(Repeat(value, length * 2) - length);
    public static float SmoothApproach(float current, float target, float speed) => Mathf.MoveTowards(current, target, speed * Time.deltaTime);
    public static float Oscillate(float speed, float amplitude) => Mathf.Sin(Time.time * speed) * amplitude;
    public static Vector3 RandomInsideRange(Vector3 center, Vector3 range) => center + new Vector3(Random.Range(-range.x, range.x), Random.Range(-range.y, range.y), Random.Range(-range.z, range.z));
    public static float DeltaAngle(float from, float to) => Mathf.Repeat((to - from) + 180f, 360f) - 180f;
    public static float LerpAngle(float from, float to, float t) => from + DeltaAngle(from, to) * t;
    public static float SignZeroSafe(float value) => Mathf.Approximately(value, 0f) ? 0f : Mathf.Sign(value);
    public static float Mid(float a, float b) => (a + b) * 0.5f;
    public static Vector3 Mid(Vector3 a, Vector3 b) => (a + b) * 0.5f;
}
