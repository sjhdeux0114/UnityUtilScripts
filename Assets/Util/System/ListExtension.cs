using System;
using System.Collections.Generic;

public static class ListExtensions
{
    private static Random rng = new Random(); // 랜덤 인스턴스

    /// <summary>
    /// Fisher-Yates 셔플 알고리즘을 사용하여 리스트를 랜덤하게 섞습니다.
    /// </summary>
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1); // 0부터 n까지의 랜덤 인덱스

            // 현재 요소(n)와 랜덤 요소(k)를 교환 (Swap)
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}