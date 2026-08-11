# Unity Util Scripts (com.sjh.unityutilscripts)

유니티 프로젝트 개발을 위한 유틸리티 스크립트, 트윈 애니메이션, 에디터 확장 도구 모음 패키지입니다.

---

## 📦 설치 방법 (Installation)

Unity Package Manager (UPM)를 이용하여 Git URL로 손쉽게 프로젝트에 추가할 수 있습니다.

1. Unity Editor 메뉴에서 **`Window` → `Package Manager`** 선택
2. 좌측 상단의 **`+`** 버튼 클릭 후 **`Add package from git URL...`** 선택
3. 아래 URL 입력:

```text
https://github.com/sjhdeux0114/UnityUtilScripts.git?path=/Assets/Util
```

---

## ⚠️ 필수 프로젝트 설정 (Required Project Settings)

> [!IMPORTANT]
> **API Compatibility Level 설정**  
> 유니티 기본 설정인 `.NET Standard 2.1` 환경에서는 라이브러리 및 플러그인 호환성 오류가 발생할 수 있습니다.  
> 반드시 프로젝트 설정에서 **API Compatibility Level**을 **`.NET Framework`**로 변경해 주세요.
>
> **설정 방법:**
> 1. Unity Editor 상단 메뉴: **`Edit` → `Project Settings...`** 선택
> 2. **`Player`** 탭 → **`Other Settings`** 섹션 이동
> 3. **`Configuration`** 그룹 내 **`Api Compatibility Level`**을 **`.NET Framework`** (또는 `.NET Framework 4.x`)로 변경

---

## ✨ 주요 기능 (Key Features)

### 1. 🎬 Animation & Sprite
- **SimpleSpriteAnimator / SpriteGroupAnimator**: 스프라이트 시퀀스 애니메이션 제어 및 그룹 애니메이터
- **Ani_Move_Control**: 애니메이션과 이동 경로 연동 제어

### 2. 🚚 Movement & Tween
- **MovePathPos / MoveTween / ScaleTween**: 경로 이동 및 트위닝 애니메이션
- **OpenMove**: 방향 및 속도 기반 개별 이동 제어
- **RandomXYMove / SimpleFollow**: 무작위 좌표 이동 및 대상 추적

### 3. 🔊 Audio Management
- **SoundManager / PlaySound**: 사운드 효과 및 BGM 채널 관리 및 재생 컴포넌트

### 4. 🖼️ UI & UITween
- **UITween**: UI 애니메이션 및 트윈 효과 (Fade, Scale, Position)
- **SmoothGridLayout**: 유연한 가이드 레이아웃 및 폰트 컴포넌트

### 5. 🛠️ Editor Extensions (에디터 도구)
- **DefineManagerWindow**: `#define` 전처리기 상수를 에디터 창에서 손쉽게 관리
- **EnumManagerWindow**: 글로벌 Enum 생성 및 관리 도구
- **AnimationEventCopier**: 애니메이션 클립 간 이벤트 복사 도구
- **AssetCleaner / FindReferencesInScene**: 미사용 리소스 정리 및 씬 내 참조 검색

---

## 📝 라이선스 (License)

MIT License
