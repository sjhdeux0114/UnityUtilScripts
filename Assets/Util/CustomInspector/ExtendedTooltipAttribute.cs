using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class ExtendedTooltipAttribute : PropertyAttribute
{
    public readonly string Title;
    public readonly string Message;
    public readonly string Url;
    public readonly MessageType Type;
    public readonly bool StartExpanded;
    public readonly string FoldLabel; // 폴드아웃에 표시할 라벨 (없으면 Title 사용)

    public enum MessageType
    {
        None, Info, Warning, Error
    }

    public ExtendedTooltipAttribute(
        string title,
        string message,
        MessageType type = MessageType.Info,
        string url = null,
        bool startExpanded = false,
        string foldLabel = "설명")
    {
        Title = title;
        Message = message;
        Url = url;
        Type = type;
        StartExpanded = startExpanded;
        FoldLabel = foldLabel;
    }
}
