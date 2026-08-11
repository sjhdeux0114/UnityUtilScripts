using UnityEngine;
using System;

/// <summary>
/// 인스펙터에서 Enum 필드나 변수에 커스텀 라벨을 지정할 수 있게 해주는 속성입니다.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class EnumLabelAttribute : PropertyAttribute
{
    public string label;

    public EnumLabelAttribute(string label)
    {
        this.label = label;
    }
}
