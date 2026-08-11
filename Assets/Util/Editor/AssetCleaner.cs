
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

/// <summary>
/// Unity 프로젝트 내에서 사용되지 않는 에셋을 찾아 삭제하는 에디터 윈도우 클래스입니다.
/// </summary>
public class AssetCleaner : EditorWindow
{
    // 멤버 변수 선언
    private List<string> unusedAssets = new List<string>();                 // 사용되지 않는 에셋의 경로 목록
    private Dictionary<string, bool> selectedAssets = new Dictionary<string, bool>(); // 선택 상태를 관리하는 딕셔너리 (경로, 선택여부)
    private Vector2 scrollPosition;                                         // 스크롤 뷰의 위치
    private bool searchCompleted = false;                                   // 검색 완료 여부 플래그

    /// <summary>
    /// Unity 에디터의 메뉴에 "Tools/Find Unused Assets" 항목을 추가하고, 클릭 시 윈도우를 엽니다.
    /// </summary>
    [MenuItem("Tools/Find Unused Assets")]
    public static void ShowWindow()
    {
        GetWindow<AssetCleaner>("Asset Cleaner");
    }

    /// <summary>
    /// 에디터 윈도우의 GUI를 렌더링하는 메서드입니다.
    /// </summary>
    void OnGUI()
    {
        GUILayout.Label("Find and Delete Unused Assets", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // "에셋 찾기" 버튼
        if (GUILayout.Button("1. Find Unused Assets"))
        {
            FindAndListUnusedAssets();
            searchCompleted = true;
        }

        // 검색이 완료된 후에만 아래 UI를 표시
        if (searchCompleted)
        {
            GUILayout.Space(10);
            if (unusedAssets.Count > 0)
            {
                EditorGUILayout.HelpBox($"Found {unusedAssets.Count} unused assets.", MessageType.Info);

                // 전체 선택 / 해제 버튼
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Select All")) SelectAll(true);
                if (GUILayout.Button("Deselect All")) SelectAll(false);
                EditorGUILayout.EndHorizontal();

                // 체크박스와 함께 에셋 목록을 스크롤 뷰에 표시
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(400));
                var tempSelectedAssets = new Dictionary<string, bool>(selectedAssets);
                foreach (string assetPath in unusedAssets)
                {
                    if (tempSelectedAssets.ContainsKey(assetPath))
                    {
                        EditorGUILayout.BeginHorizontal();
                        // 체크박스
                        tempSelectedAssets[assetPath] = EditorGUILayout.Toggle(tempSelectedAssets[assetPath], GUILayout.Width(20));
                                                // "보기" 버튼
                        if (GUILayout.Button("View", GUILayout.Width(60)))
                        {
                            // 경로를 이용해 에셋을 로드하고 프로젝트 창에서 해당 에셋을 하이라이트(ping)
                            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                            EditorGUIUtility.PingObject(obj);
                        }

                        // 에셋 경로 레이블
                        EditorGUILayout.LabelField(assetPath);
                        EditorGUILayout.EndHorizontal();
                    }
                }
                selectedAssets = tempSelectedAssets;
                EditorGUILayout.EndScrollView();

                // 선택된 에셋 삭제 버튼
                GUILayout.Space(10);
                int selectedCount = selectedAssets.Count(kvp => kvp.Value);
                GUI.enabled = selectedCount > 0; // 1개 이상 선택됐을 때만 버튼 활성화

                EditorGUILayout.HelpBox("경고: 에셋 삭제는 되돌릴 수 없습니다. 반드시 프로젝트를 백업하세요.", MessageType.Warning);
                if (GUILayout.Button($"2. Delete {selectedCount} Selected Assets (Backup Recommended!)"))
                {
                    // 삭제 전 최종 확인 대화상자
                    if (EditorUtility.DisplayDialog("Confirm Deletion",
                        $"정말로 선택된 {selectedCount}개의 에셋을 삭제하시겠습니까? 이 작업은 되돌릴 수 없습니다.",
                        "Yes, Delete", "Cancel"))
                    {
                        DeleteSelectedAssets();
                    }
                }
                GUI.enabled = true; // 다른 UI를 위해 GUI 활성화 상태 복원
            }
            else
            {
                EditorGUILayout.HelpBox("사용되지 않는 에셋을 찾지 못했습니다.", MessageType.Info);
            }
        }
    }

    /// <summary>
    /// 모든 에셋을 선택하거나 선택 해제합니다.
    /// </summary>
    /// <param name="select">true이면 전체 선택, false이면 전체 해제</param>
    private void SelectAll(bool select)
    {
        var keys = new List<string>(selectedAssets.Keys);
        foreach (var key in keys)
        {
            selectedAssets[key] = select;
        }
    }

    /// <summary>
    /// 프로젝트 내에서 사용되지 않는 에셋을 검색하고 목록을 채웁니다.
    /// </summary>
    private void FindAndListUnusedAssets()
    {
        unusedAssets.Clear();
        selectedAssets.Clear();

        // 1. 빌드 설정에 포함된 모든 활성 씬의 경로를 가져옵니다.
        var scenePaths = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenePaths.Length == 0)
        {
            Debug.LogWarning("빌드 설정에 활성화된 씬이 없습니다. 검색이 정확하지 않을 수 있습니다.");
            return;
        }

        // 2. 씬들과 그에 종속된 모든 에셋(사용 중인 에셋)의 경로를 가져옵니다.
        var usedAssetPaths = new HashSet<string>(AssetDatabase.GetDependencies(scenePaths, true));

        // 3. "Resources" 폴더의 모든 에셋은 동적 로딩 가능성이 있으므로 사용 중인 것으로 간주합니다.
        string[] allResourceGuids = AssetDatabase.FindAssets("", new[] { "Assets/Resources" });
        foreach (string guid in allResourceGuids)
        {
            usedAssetPaths.Add(AssetDatabase.GUIDToAssetPath(guid));
        }

        // 4. 프로젝트 내의 모든 에셋 경로를 가져옵니다.
        var allAssetPaths = AssetDatabase.GetAllAssetPaths();

        // 5. 모든 에셋과 사용 중인 에셋을 비교하여 사용되지 않는 에셋을 찾습니다.
        foreach (string assetPath in allAssetPaths)
        {
            // Assets 폴더에 있고, 폴더나 스크립트가 아니며, 사용 목록에 없는 경우
            if (assetPath.StartsWith("Assets/") &&
                !AssetDatabase.IsValidFolder(assetPath) &&
                !IsScript(assetPath) &&
                !usedAssetPaths.Contains(assetPath))
            {
                // 에디터 관련 폴더는 제외
                if (!assetPath.StartsWith("Assets/Editor") && !assetPath.Contains("/Editor/"))
                {
                    unusedAssets.Add(assetPath);
                    selectedAssets[assetPath] = false; // 초기 상태는 선택되지 않음
                }
            }
        }

        Debug.Log($"검색 완료. {unusedAssets.Count}개의 사용되지 않는 에셋을 찾았습니다.");
    }

    /// <summary>
    /// 사용자가 선택한 에셋들을 삭제합니다.
    /// </summary>
    private void DeleteSelectedAssets()
    {
        var assetsToDelete = selectedAssets
            .Where(kvp => kvp.Value) // Value가 true인(선택된) 항목만 필터링
            .Select(kvp => kvp.Key)  // Key(에셋 경로)를 선택
            .ToList();

        if (assetsToDelete.Count == 0)
        {
            Debug.Log("삭제할 에셋이 선택되지 않았습니다.");
            return;
        }

        // 여러 에셋을 삭제할 때 성능 향상을 위해 사용
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (string assetPath in assetsToDelete)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh(); // 프로젝트 뷰 새로고침
            Debug.Log($"성공적으로 {assetsToDelete.Count}개의 에셋을 삭제했습니다.");
            
            // 삭제 후 목록을 새로고침
            FindAndListUnusedAssets();
        }
    }

    /// <summary>
    /// 주어진 경로가 스크립트 파일인지 확인하는 헬퍼 메서드입니다.
    /// </summary>
    private bool IsScript(string path)
    {
        string extension = Path.GetExtension(path).ToLower();
        return extension == ".cs" || extension == ".js" || extension == ".boo";
    }
}
