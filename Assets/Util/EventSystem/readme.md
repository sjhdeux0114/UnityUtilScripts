# Stage Event System (단계별 캐릭터 이벤트 시스템)

이 패키지는 캐릭터 및 스텝(단계) 기반의 게임 내 이벤트를 손쉽게 구축, 확장, 시뮬레이션할 수 있게 돕는 범용 유니티 모듈입니다. 하드코딩된 열거형(Enum)이나 딕셔너리 매핑 대신 **ScriptableObject 기반 캐릭터 정의**와 **가중치 기반 추첨 로직**을 채택하여 범용성과 확장성이 극대화되었습니다.

---

## 📂 폴더 및 스크립트 구조

```text
Assets/Script/EventSystem/
├── CharacterDefinition.cs       # 캐릭터 정보의 기본이 되는 ScriptableObject base
├── StepCharacterWeight.cs       # 특정 단계에서 캐릭터가 출현할 가중치 및 목표 단계 구조체
├── StepData.cs                  # 스텝(Step)별 후보군 캐릭터 목록 관리
├── BaseStepEvent.cs             # 공통 단계 전이 상태 머신 및 가중치 추첨 코어 클래스
├── Editor/
│   ├── BaseStepEventEditor.cs   # BaseStepEvent 인스펙터 커스텀 에디터
│   └── BaseEventSimulatorWindow.cs # 스페이스바/엔터 키를 통한 범용 로직 테스트 시뮬레이터
└── readme.md                    # 본 사용 설명서
```

---

## 🛠️ 사용 방법 (단계별 가이드)

### 1. 캐릭터 정의 ScriptableObject 제작
먼저 개별 게임에 특화된 연출 정보(예: 애니메이션 파일 이름, 프레임, 당첨 조건 등)를 정의할 수 있는 캐릭터 ScriptableObject 클래스를 정의합니다.
이때 반드시 **`CharacterDefinition`**을 상속받아야 합니다.

```csharp
using UnityEngine;
using StageEventSystem;

[CreateAssetMenu(fileName = "MyGameCharInfo", menuName = "Custom Event/Character Info")]
public class MyGameCharInfo : CharacterDefinition
{
    [Header("Bink 연출 설정")]
    public string idleVideoPath;
    public string attackVideoPath;
    
    [Header("진입 조건 설정")]
    public int minPrizeMoney; // 캐릭터가 출현하기 위한 최소 당첨금 조건
}
```
*클래스 선언 후 Unity Editor에서 `Assets -> Create -> Custom Event -> Character Info` 메뉴를 통해 캐릭터 파일(예: `Warrior.asset`, `Mage.asset` 등)을 생성하고 고유 **`Character Id`** (정수형) 및 **`Display Name`**을 입력합니다.*

### 2. 이벤트 로직 클래스 구현
게임 내에서 실제로 진행될 이벤트 매니저 클래스를 생성하고 **`BaseStepEvent`**를 상속받습니다.
이곳에서 게임별 진입 차단 조건 및 연출 제어 로직을 작성합니다.

```csharp
using System.Collections;
using UnityEngine;
using StageEventSystem;

public class MyGameEvent : BaseStepEvent
{
    // 1. 캐릭터 진입 가능 조건을 필터링하기 위해 오버라이드
    protected override bool IsCharacterValidForStep(CharacterDefinition character)
    {
        var info = character as MyGameCharInfo;
        if (info != null)
        {
            // 예시: 남은 게임 당첨 금액이 캐릭터 설정 최소 조건 이상이어야 함
            return remainingPrizeMoney >= info.minPrizeMoney;
        }
        return false;
    }

    // 2. 공격 횟수 획득 및 플레이리스트 세팅
    public override bool SetupStage()
    {
        var info = currentCharacter as MyGameCharInfo;
        if (info == null) return false;

        // 예시: 비디오 플레이어에 비디오 세팅
        // videoPlayer.Play(info.idleVideoPath);
        
        // 베이스 클래스의 minAttackCount / maxAttackCount 사이에서 공격 횟수 도출
        remainingAttacks = GetRandomAttackCount(); 
        return true;
    }

    // 3. 포커 프레임워크 또는 게임 시퀀스 실행 루프 재정의
    public override IEnumerator _Main_Proc()
    {
        // _Init 호출을 통해 초기 currentCharacter가 세팅됩니다.
        if (currentCharacter == null)
        {
            currentCharacter = SelectNextCharacter(currentStep);
        }

        while (true)
        {
            if (!SetupStage()) yield break;

            // 시뮬레이션 동작 검증을 위해 로그 표시 및 대기
            yield return new WaitForSeconds(1.0f);
            
            // ... 공격 루프 및 승리 체크
        }
    }
}
```

### 3. Unity Inspector 설정
1. 씬 내의 적절한 GameObject에 구현한 `MyGameEvent` 스크립트를 추가합니다.
2. `BaseStepEventEditor`가 적용되어 인스펙터에 **Step Configuration** 영역이 깔끔하게 표시됩니다.
3. `+ Add New Step` 버튼을 클릭하여 단계를 추가합니다.
4. 각 단계의 `Candidate Characters` 목록에 미리 만들어 둔 캐릭터 ScriptableObject 애셋들을 끌어다 놓습니다.
5. **Weight**(상대적 등장 가중치)와 **Next Step**(해당 캐릭터가 활성화되어 클리어 시 다음으로 이동할 타깃 단계 번호)을 각각 기입합니다.

---

## 🖥️ 시뮬레이터 사용법 (Space / Return)

에디터 상에서 씬을 실행하지 않고도 단계 전이 확률과 통계를 실시간으로 검증할 수 있습니다.

1. 이벤트 컴포넌트 인스펙터 최하단의 **`Open Simulator Window`** 버튼을 누르거나, 상단 메뉴바의 `Window -> Event System Simulator` 창을 엽니다.
2. 시뮬레이션할 대상 스크립트를 지정하고 테스트용 `Virtual Starting Prize`(가상 당첨금)를 입력합니다. (0 또는 비워둘 시 랜덤 금액 자동 생성)
3. 단축키 및 실행:
   - **`시뮬레이션 시작 / 초기화 (Start Simulation)`**: 단축키 **`Space`** (시뮬레이션이 활성화되지 않은 상태)
   - **`다음 공격 진행 (Next Attack)`**: 단축키 **`Space`** (시뮬레이션 진행 중) - 타격 성공(단계 전이) 혹은 일반 타격(보류)에 대해 정밀 로그를 기록합니다.
   - **`10회 자동 시뮬레이션 (Auto Play)`**: 단축키 **`Return (Enter)`** - 입력된 금액으로 전체 시퀀스를 10회 연속 자동 진행하여 최종 성공률 통계를 표시합니다.
