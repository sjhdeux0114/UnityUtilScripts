#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AudioClip))]
public class AudioClipDrawer : PropertyDrawer
{
    private const float Padding = 2f;
    private const float ButtonWidth = 24f;
    private const float ButtonHeight = 18f;
    private const float Gap = 4f;
    private const float TimeWidth = 32f;
    private const float MinVolumeWidth = 70f;
    private const float MinRowWidth = 220f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float lineHeight = EditorGUIUtility.singleLineHeight;
        Rect fieldRect = new Rect(position.x, position.y, position.width, lineHeight);
        EditorGUI.PropertyField(fieldRect, property, label);

        AudioClip clip = property.objectReferenceValue as AudioClip;
        if (clip != null)
        {
            // Find parent SndClass to check if there is a sibling 'vol' property
            int lastDot = property.propertyPath.LastIndexOf('.');
            string parentPath = lastDot >= 0 ? property.propertyPath.Substring(0, lastDot) : "";
            SerializedProperty parentProp = !string.IsNullOrEmpty(parentPath) ? property.serializedObject.FindProperty(parentPath) : null;
            SerializedProperty volProp = parentProp?.FindPropertyRelative("vol");

            DrawPreviewControls(position, lineHeight, clip, volProp);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;

        if (property.objectReferenceValue != null)
        {
            height += ButtonHeight + Padding;
        }

        return height;
    }

    private static void DrawPreviewControls(Rect position, float lineHeight, AudioClip clip, SerializedProperty volProp)
    {
        float rowX = position.x + EditorGUIUtility.labelWidth;
        float rowWidth = position.width - EditorGUIUtility.labelWidth;

        if (rowWidth < MinRowWidth)
        {
            rowX = position.x;
            rowWidth = position.width;
        }

        Rect rowRect = new Rect(rowX, position.y + lineHeight + Padding, rowWidth, ButtonHeight);
        Rect playRect = new Rect(rowRect.x, rowRect.y, ButtonWidth, ButtonHeight);
        Rect stopRect = new Rect(playRect.xMax + Gap, rowRect.y, ButtonWidth, ButtonHeight);
        Rect timeRect = new Rect(stopRect.xMax + Gap, rowRect.y, TimeWidth, ButtonHeight);

        Color oldColor = GUI.backgroundColor;

        GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
        if (GUI.Button(playRect, "▶"))
        {
            float previewVolume = volProp != null ? (volProp.floatValue / 100f) : AudioPreviewer.Volume;
            AudioPreviewer.Play(clip, previewVolume);
        }

        GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
        if (GUI.Button(stopRect, "■"))
        {
            AudioPreviewer.Stop();
        }

        GUI.backgroundColor = oldColor;

        GUI.Label(timeRect, $"{clip.length:F1}s", EditorStyles.miniLabel);

        float volumeX = timeRect.xMax + Gap;
        float volumeWidth = Mathf.Max(MinVolumeWidth, rowRect.xMax - volumeX);
        Rect volumeRect = new Rect(volumeX, rowRect.y, volumeWidth, ButtonHeight);

        float inputFieldWidth = 35f; // Set custom narrow width for input field (fits 100 or 1.0)
        float sliderBarWidth = volumeWidth - inputFieldWidth - Gap;
        Rect sliderBarRect = new Rect(volumeRect.x, volumeRect.y, sliderBarWidth, volumeRect.height);
        Rect inputFieldRect = new Rect(sliderBarRect.xMax, volumeRect.y, inputFieldWidth, volumeRect.height);

        EditorGUI.BeginChangeCheck();
        if (volProp != null)
        {
            float currentVal = volProp.floatValue;

            // Draw slider bar

            float sliderVal = GUI.HorizontalSlider(sliderBarRect, currentVal, 0f, 100f);

            // Draw numeric float field next to it

            float inputVal = EditorGUI.FloatField(inputFieldRect, currentVal, EditorStyles.miniTextField);


            if (EditorGUI.EndChangeCheck())
            {
                float finalVal = currentVal;
                if (Mathf.Abs(sliderVal - currentVal) > 0.0001f)
                {
                    finalVal = sliderVal;
                }
                else if (Mathf.Abs(inputVal - currentVal) > 0.0001f)
                {
                    finalVal = Mathf.Clamp(inputVal, 0f, 100f);
                }


                volProp.floatValue = finalVal;
                AudioPreviewer.Volume = finalVal / 100f;
            }
        }
        else
        {
            float currentVal = AudioPreviewer.Volume;

            // Draw slider bar

            float sliderVal = GUI.HorizontalSlider(sliderBarRect, currentVal, 0f, 1f);

            // Draw numeric float field next to it

            float inputVal = EditorGUI.FloatField(inputFieldRect, currentVal, EditorStyles.miniTextField);


            if (EditorGUI.EndChangeCheck())
            {
                float finalVal = currentVal;
                if (Mathf.Abs(sliderVal - currentVal) > 0.0001f)
                {
                    finalVal = sliderVal;
                }
                else if (Mathf.Abs(inputVal - currentVal) > 0.0001f)
                {
                    finalVal = Mathf.Clamp(inputVal, 0f, 1f);
                }


                AudioPreviewer.Volume = finalVal;
            }
        }
    }
}
#endif
