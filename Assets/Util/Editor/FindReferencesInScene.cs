using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FindReferencesInScene : EditorWindow
{
    [MenuItem("GameObject/Find References In Scene", false, 0)]
    public static void FindReferences()
    {
        GameObject targetGo = Selection.activeGameObject;
        if (targetGo == null)
        {
            Debug.LogWarning("선택된 GameObject가 없습니다.");
            return;
        }

        Debug.Log($"<b><color=cyan>[FindReferences]</color></b> '<color=yellow>{targetGo.name}</color>'를 참조하는 오브젝트 검색 시작...");

        // 대상 게임오브젝트와 여기에 붙어있는 모든 컴포넌트를 탐색 대상에 포함
        HashSet<Object> targetObjects = new HashSet<Object>();
        targetObjects.Add(targetGo);
        foreach (Component comp in targetGo.GetComponents<Component>())
        {
            if (comp != null)
                targetObjects.Add(comp);
        }

        // 현재 로드된 모든 씬의 컴포넌트 가져오기 (비활성화된 컴포넌트 포함)
        List<Component> allComponents = new List<Component>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (GameObject root in rootObjects)
            {
                allComponents.AddRange(root.GetComponentsInChildren<Component>(true));
            }
        }

        int matchCount = 0;

        foreach (Component curComp in allComponents)
        {
            if (curComp == null) continue;
            // 자기 자신에 대한 참조는 일단 건너뜀
            if (curComp.gameObject == targetGo) continue;

            SerializedObject so = new SerializedObject(curComp);
            SerializedProperty sp = so.GetIterator();

            // 모든 프로퍼티를 순회하며 참조를 검사
            while (sp.NextVisible(true))
            {
                if (sp.propertyType == SerializedPropertyType.ObjectReference)
                {
                    if (sp.objectReferenceValue != null && targetObjects.Contains(sp.objectReferenceValue))
                    {
                        Debug.Log($"[참조 발견] <color=lime>{curComp.gameObject.name}</color>의 <b>{curComp.GetType().Name}</b> 컴포넌트 (프로퍼티 명: {sp.displayName})", curComp.gameObject);
                        matchCount++;
                    }
                }
            }
        }

        if (matchCount == 0)
        {
            Debug.Log($"'<color=yellow>{targetGo.name}</color>'를 참조하고 있는 오브젝트를 씬에서 찾지 못했습니다.");
        }
        else
        {
            Debug.Log($"<color=cyan>총 {matchCount}개의 참조를 찾았습니다.</color>");
        }
    }
}
