using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpriteGroup))]
public class SpriteGroupEditor : Editor
{

    [MenuItem("Assets/Create/Create SpriteGroup from Selection", false, 10)]
    public static void CreateSpriteGroupFromSelection()
    {
        Object[] selectedObjects = Selection.objects;
        List<Sprite> sprites = new List<Sprite>();

        foreach (var obj in selectedObjects)
        {
            if (obj is Sprite sprite)
            {
                sprites.Add(sprite);
            }
            else if (obj is Texture2D texture)
            {
                // 슬라이스된 스프라이트의 경우 텍스처 하위의 모든 스프라이트를 가져옴
                string assetPath = AssetDatabase.GetAssetPath(texture);
                Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                foreach (var sub in subAssets)
                {
                    if (sub is Sprite subSprite)
                    {
                        sprites.Add(subSprite);
                    }
                }
            }
        }

        if (sprites.Count == 0)
        {
            EditorUtility.DisplayDialog("경고", "선택된 스프라이트가 없습니다. 프로젝트 창에서 하나 이상의 스프라이트나 텍스처를 선택해 주세요.", "확인");
            return;
        }

        // 알반화된 숫자 정렬(Natural Sort) 적용 (예: sprite_2가 sprite_10보다 먼저 오도록)
        sprites.Sort((a, b) => NaturalSortComparer.Compare(a.name, b.name));

        // SpriteGroup 인스턴스 생성 및 설정
        SpriteGroup spriteGroup = ScriptableObject.CreateInstance<SpriteGroup>();
        spriteGroup.Sprites = sprites.ToArray();
        spriteGroup._fps = 30f; // 기본 FPS

        // 저장 경로 결정 (현재 선택한 폴더 혹은 파일이 있는 폴더)
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path))
        {
            path = "Assets";
        }
        else if (File.Exists(path))
        {
            path = Path.GetDirectoryName(path);
        }

        // 고유 파일 이름 생성 및 파일 저장
        string defaultName = sprites[0].name + "_Group.asset";
        string fullPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, defaultName));

        AssetDatabase.CreateAsset(spriteGroup, fullPath);
        AssetDatabase.SaveAssets();

        // 생성된 에셋 포커싱
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = spriteGroup;
    }

    [MenuItem("Assets/Create/Create SpriteGroup from Selection", true)]
    public static bool CreateSpriteGroupFromSelectionValidate()
    {
        // 스프라이트 또는 텍스처가 하나 이상 선택되어 있을 때만 메뉴 활성화
        foreach (var obj in Selection.objects)
        {
            if (obj is Sprite || obj is Texture2D)
                return true;
        }
        return false;
    }
}

/// <summary>
/// 파일명의 숫자와 문자를 고려하여 자연스러운 순서로 정렬하는 헬퍼 클래스 (Natural Sort)
/// </summary>
public static class NaturalSortComparer
{
    public static int Compare(string x, string y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;

        int ix = 0;
        int iy = 0;

        while (ix < x.Length && iy < y.Length)
        {
            if (char.IsDigit(x[ix]) && char.IsDigit(y[iy]))
            {
                string numX = "";
                while (ix < x.Length && char.IsDigit(x[ix])) numX += x[ix++];
                string numY = "";
                while (iy < y.Length && char.IsDigit(y[iy])) numY += y[iy++];

                if (long.TryParse(numX, out long nX) && long.TryParse(numY, out long nY))
                {
                    int comp = nX.CompareTo(nY);
                    if (comp != 0) return comp;
                }
                else
                {
                    int comp = string.Compare(numX, numY, System.StringComparison.Ordinal);
                    if (comp != 0) return comp;
                }
            }
            else
            {
                int comp = x[ix].CompareTo(y[iy]);
                if (comp != 0) return comp;
                ix++;
                iy++;
            }
        }

        return x.Length.CompareTo(y.Length);
    }
}