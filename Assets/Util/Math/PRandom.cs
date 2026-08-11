using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PseudoRandomChecker
/// 워크래프트 3, 도타 2 방식의 의사 무작위 분포(PRD) 확률 계산기입니다.
/// 연속 실패 시 확률이 누적 증가하여 극단적인 연패 스트레스를 방지합니다.
/// </summary>
public class PRandom
{
    private float targetProbability; // 목표 확률 (0.0 ~ 1.0)
    private float cConstant;         // 수학적 수렴 보정 상수 (C)
    private int failCount = 0;       // 현재 연속 실패 횟수

    /// <summary>
    /// 의사 무작위 분포 checker를 생성합니다.
    /// </summary>
    /// <param name="probabilityPercent">목표 확률 (예: 15.0f -> 15%)</param>
    public PRandom(float probabilityPercent)
    {
        Initialize(probabilityPercent);
    }

    /// <summary>
    /// 주사위를 굴려 성공 여부를 반환합니다. 
    /// 실패 시 다음 주사위의 성공 확률이 누적 증가합니다.
    /// </summary>
    public bool Get()
    {
        failCount++;

        // 이번 판정에 적용될 실제 확률 = 보정 상수 * 실패 횟수
        float currentChance = cConstant * failCount;

        // 0.0 ~ 1.0 사이의 무작위 값과 비교
        if (UnityEngine.Random.value < currentChance)
        {
            failCount = 0; // 성공 시 실패 카운트 리셋
            return true;
        }

        return false;
    }

    /// <summary>
    /// 장비 변경이나 버프 등으로 확률이 변경되었을 때, 새 객체 생성 없이 내부 값만 재설정합니다.
    /// </summary>
    /// <param name="newProbabilityPercent">새로운 목표 확률 (%)</param>
    /// <param name="keepFailCount">true일 경우 기존 실패 카운트를 유지, false(기본값)일 경우 0으로 초기화</param>
    public void ResetProbability(float newProbabilityPercent, bool keepFailCount = false)
    {
        Initialize(newProbabilityPercent);

        if (!keepFailCount)
        {
            failCount = 0;
        }
    }

    /// <summary>
    /// 현재까지 쌓인 연속 실패 횟수를 수동으로 초기화합니다.
    /// </summary>
    public void ClearFailCount()
    {
        failCount = 0;
    }

    // 내부 초기화 및 C 상수 계산 매커니즘
    private void Initialize(float percent)
    {
        // 0~100 사이 안전장치 후 0~1 범위로 변환
        percent = Mathf.Clamp(percent, 0f, 100f);
        this.targetProbability = percent / 100f;

        // 목표 확률에 수렴하는 이진 탐색 기반 C 상수 계산
        this.cConstant = FindCConstant(targetProbability);
    }

    private float FindCConstant(float p)
    {
        if (p <= 0f) return 0f;
        if (p >= 1f) return 1f;

        float cUpper = p;
        float cLower = 0f;
        float cMid = p;

        // 20회 반복으로 정밀한 근사치 추적
        for (int i = 0; i < 20; i++)
        {
            cMid = (cUpper + cLower) / 2f;
            float pTest = PFromC(cMid);
            if (pTest > p) cUpper = cMid;
            else cLower = cMid;
        }
        return cMid;
    }

    private float PFromC(float c)
    {
        float pProcTotal = 0f;
        float pProcAccum = 1f;
        int maxFail = Mathf.CeilToInt(1f / c);

        for (int i = 1; i <= maxFail; i++)
        {
            float pProc = Mathf.Min(1f, i * c);
            pProcTotal += i * pProcAccum * pProc;
            pProcAccum *= (1f - pProc);
        }
        return 1f / pProcTotal;
    }
}



public class WRandom
{
    private List<int> cardDeck = new List<int>(); // 가중치 비율대로 채워질 카드 주머니
    private int currentCardIndex = 0;             // 현재 몇 번째 카드를 뽑았는지 기록
    private int[] originalWeights;              // 원본 가중치 배열 저장

    public WRandom(int[] weights)
    {
        int deckScale = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            deckScale += weights[i];
        }
        this.originalWeights = weights;
        BuildDeck(deckScale);
    }

    // 1. 가중치 비율대로 가상 카드 덱 채우기
    private void BuildDeck(int deckScale)
    {
        cardDeck.Clear();

        // 가중치 총합 구하기
        float totalWeight = 0;
        foreach (var w in originalWeights) totalWeight += w;

        // 각 항목별로 전체 덱 크기(예: 100장) 대비 몇 장을 가질지 계산해서 채움
        for (int i = 0; i < originalWeights.Length; i++)
        {
            float ratio = originalWeights[i] / totalWeight;
            int cardCount = Mathf.RoundToInt(ratio * deckScale);

            for (int j = 0; j < cardCount; j++)
            {
                cardDeck.Add(i); // 아이템의 인덱스(번호)를 카드로 추가
            }
        }

        // 2. 카드 덱 섞기 (Fisher-Yates Shuffle)
        Shuffle();
        currentCardIndex = 0;
    }

    private void Shuffle()
    {
        for (int i = cardDeck.Count - 1; i > 0; i--)
        {
            int rnd = UnityEngine.Random.Range(0, i + 1);
            int temp = cardDeck[i];
            cardDeck[i] = cardDeck[rnd];
            cardDeck[rnd] = temp;
        }
    }

    // 3. 실전에서 유저가 뽑을 때 호출하는 함수
    public int Draw()
    {
        if (cardDeck.Count == 0) return 0;

        // 카드를 다 썼으면 다시 주머니 채우고 섞기
        if (currentCardIndex >= cardDeck.Count)
        {
            Shuffle();
            currentCardIndex = 0;
        }

        // 현재 순서의 카드를 유저에게 주고 인덱스 증가
        int selectedIndex = cardDeck[currentCardIndex];
        currentCardIndex++;

        return selectedIndex; // 당첨된 배열의 인덱스 반환
    }
}