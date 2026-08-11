using UnityEngine;
using System;
using System.Collections;

// [마킹용] 이 어트리뷰트가 붙은 필드는 UniversalDrawer를 통해 그려집니다.
public class ExtendedInspectorAttribute : PropertyAttribute { }

// 1. 탭 그룹 정의
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class TabGroupAttribute : PropertyAttribute
{
    public string tabName;
    public TabGroupAttribute(string name) { this.tabName = name; }
}

// 2. 박스 그룹 정의
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class BoxGroupAttribute : PropertyAttribute
{
    public string groupName;
    public bool showLabel;
    public BoxGroupAttribute(string name, bool showLabel = true)
    {
        this.groupName = name;
        this.showLabel = showLabel;
    }
}

// 3. 가로 배치 정의
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class HorizontalGroupAttribute : PropertyAttribute
{
    public HorizontalGroupAttribute() { }
}

// 4. 뷰어 (이미지/오디오/프리팹/텍스트)
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class ViewerAttribute : PropertyAttribute
{
    public float Width;
    public float Height;
    public ViewerAttribute(float width = 100, float height = 100)
    {
        Width = width;
        Height = height;
    }
}

// 5. 버튼
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class InspectorButtonAttribute : Attribute
{
    public string Label;
    public float SpaceBefore;
    public InspectorButtonAttribute(string label = null, float spaceBefore = 0)
    {
        Label = label;
        SpaceBefore = spaceBefore;
    }
}

// 6. 뷰어 숨기기
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class NoViewAttribute : Attribute { }

// =========================================================
// [NEW] 새로 추가된 3대장 기능
// =========================================================

// 7. 인라인 에디터 (ScriptableObject 등을 펼쳐서 수정)
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class InlineAttribute : PropertyAttribute { }

// 8. 필수값 체크 (Null이면 경고)
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class RequiredAttribute : PropertyAttribute
{
    public string Message;
    public RequiredAttribute(string message = "This field is required!")
    {
        Message = message;
    }
}

// 9. 동적 드롭다운 (함수 이름을 문자열로 넣으면 목록으로 표시)
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class ValueDropdownAttribute : PropertyAttribute
{
    public string MethodName;
    public ValueDropdownAttribute(string methodName)
    {
        MethodName = methodName;
    }
}

// [NEW] GameObject지만 연결 안 해도 빨간불 안 뜨게 하고 싶을 때 사용
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class OptionalAttribute : Attribute { }

// [NEW] 조건부 표시 (이 변수가 true일 때만 보임)
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class ShowIfAttribute : Attribute
{
    public string ConditionName;
    public ShowIfAttribute(string conditionName)
    {
        ConditionName = conditionName;
    }
}

// [NEW] 읽기 전용 (수정 불가, 보기만 가능)
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class ReadOnlyAttribute : Attribute { }

// [NEW] 최소-최대 범위 슬라이더 (Vector2에 사용)
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class MinMaxSliderAttribute : PropertyAttribute
{
    public float Min;
    public float Max;
    public MinMaxSliderAttribute(float min, float max)
    {
        Min = min;
        Max = max;
    }
}

// [NEW] 진행바 표시 (숫자형 변수에 사용)
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class ProgressBarAttribute : PropertyAttribute
{
    public float MaxValue;     // 고정 최대값 (예: 100)
    public string MaxValueName; // 변수명으로 최대값 지정 (예: "maxHp")
    public float R, G, B;      // 바 색상 (기본값 없음 = 초록/파랑)
    public bool HasColor;

    // 고정값 사용 (0 ~ 100)
    public ProgressBarAttribute(float maxValue = 100f, float r = -1, float g = -1, float b = -1)
    {
        MaxValue = maxValue;
        if (r >= 0) { R = r; G = g; B = b; HasColor = true; }
    }
    // 변수명으로 최대값 가져오기
    public ProgressBarAttribute(string maxValueName, float r = -1, float g = -1, float b = -1)
    {
        MaxValueName = maxValueName;
        if (r >= 0) { R = r; G = g; B = b; HasColor = true; }
    }
}

// [NEW] 씬 이름 선택 (Build Settings 목록)
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class SceneNameAttribute : PropertyAttribute { }


// [NEW] 태그 선택 (String 변수에 사용)
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class TagAttribute : PropertyAttribute { }

// [NEW] 레이어 선택 (Int 변수에 사용 -> 레이어 인덱스 저장)
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class LayerAttribute : PropertyAttribute { }

// [NEW] 소팅 레이어 선택 (Int 변수에 사용 -> SortingLayer ID 저장)
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class SortingLayerAttribute : PropertyAttribute { }

// [NEW] 자식 오브젝트 자동 찾기 (이름 기반)
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class FindChildAttribute : PropertyAttribute
{
    public string ChildName; // 비워두면 변수 이름으로 찾음
    public FindChildAttribute(string childName = null)
    {
        ChildName = childName;
    }
}

// [NEW] 컬러 프리셋 (지정된 색상 버튼 제공)
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class ColorPresetAttribute : PropertyAttribute
{
    public string[] Names;
    public Color[] Colors;

    // 기본 생성자 (자주 쓰는 UI 컬러 예시)
    public ColorPresetAttribute()
    {
        Names = new string[] { "White", "Black", "Red", "Green", "Blue" };
        Colors = new Color[] { Color.white, Color.black, new Color(1f, 0.4f, 0.4f), new Color(0.4f, 1f, 0.4f), new Color(0.4f, 0.6f, 1f) };
    }

    // 커스텀 생성자 (이름, R, G, B 순서로 나열)
    public ColorPresetAttribute(string name1, float r1, float g1, float b1, string name2, float r2, float g2, float b2)
    {
        Names = new string[] { name1, name2 };
        Colors = new Color[] { new Color(r1, g1, b1), new Color(r2, g2, b2) };
    }
}

// [NEW] 값이 바뀌면 함수 실행 (인스펙터 수정 시 즉시 반영용)
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class OnValueChangedAttribute : PropertyAttribute
{
    public string MethodName;
    public OnValueChangedAttribute(string methodName)
    {
        MethodName = methodName;
    }
}
// [NEW] 배열/리스트에 사용. 폴더를 선택하면 해당 타입의 에셋을 모두 불러옴
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class AssetListAttribute : PropertyAttribute { }

// [NEW] 접미사 (단위 표시)
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class SuffixAttribute : PropertyAttribute
{
    public string Label;
    public SuffixAttribute(string label) { Label = label; }
}

// [NEW] 타이틀 (제목 + 구분선)
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class TitleAttribute : PropertyAttribute
{
    public string Title;
    public bool ShowLine;
    public TitleAttribute(string title, bool showLine = true)
    {
        Title = title;
        ShowLine = showLine;
    }
}

// [NEW] 에셋(파일)만 허용
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class AssetsOnlyAttribute : PropertyAttribute { }

// [NEW] 씬 오브젝트만 허용
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class SceneObjectsOnlyAttribute : PropertyAttribute { }

// [NEW] 애니메이션 파라미터 선택 (Animator 변수 이름을 인자로 넣어야 함)
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class AnimatorParamAttribute : PropertyAttribute
{
    public string AnimatorName;
    public AnimatorParamAttribute(string animatorName)
    {
        AnimatorName = animatorName;
    }
}

// [NEW] Input Manager 축 이름 선택
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class InputAxisAttribute : PropertyAttribute { }

// [NEW] 폴더 경로 선택 (String 변수에 사용)
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class FolderPathAttribute : PropertyAttribute { }