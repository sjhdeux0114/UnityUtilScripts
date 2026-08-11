using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class SpriteGroupHierarchyDragHandler
{
    static SpriteGroupHierarchyDragHandler()
    {
        DragAndDrop.AddDropHandler(OnHierarchyDrop);
    }

    private static DragAndDropVisualMode OnHierarchyDrop(
        int dropTargetInstanceID, 
        HierarchyDropFlags dropMode, 
        Transform parentForDraggedObjects, 
        bool perform)
    {
        if (DragAndDrop.objectReferences == null || DragAndDrop.objectReferences.Length == 0)
            return DragAndDropVisualMode.None;

        bool hasSpriteGroup = false;
        foreach (var obj in DragAndDrop.objectReferences)
        {
            if (obj is SpriteGroup)
            {
                hasSpriteGroup = true;
                break;
            }
        }

        if (!hasSpriteGroup)
            return DragAndDropVisualMode.None;

        if (perform)
        {
            GameObject parentObject = EditorUtility.InstanceIDToObject(dropTargetInstanceID) as GameObject;
            Transform parentTransform = parentObject != null ? parentObject.transform : parentForDraggedObjects;

            // If parenting under a canvas is needed for UI.Image and parent is null, look for or create a Canvas
            if (parentTransform == null)
            {
                Canvas canvas = Object.FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    parentTransform = canvas.transform;
                }
                else
                {
                    // Create Canvas
                    GameObject canvasGo = new GameObject("Canvas");
                    canvas = canvasGo.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasGo.AddComponent<CanvasScaler>();
                    canvasGo.AddComponent<GraphicRaycaster>();
                    Undo.RegisterCreatedObjectUndo(canvasGo, "Create Canvas");
                    
                    parentTransform = canvasGo.transform;

                    // Create EventSystem
                    if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                    {
                        GameObject esGo = new GameObject("EventSystem");
                        esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                        esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                        Undo.RegisterCreatedObjectUndo(esGo, "Create EventSystem");
                    }
                }
            }

            System.Collections.Generic.List<GameObject> createdObjects = new System.Collections.Generic.List<GameObject>();

            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is SpriteGroup spriteGroup)
                {
                    GameObject go = new GameObject(spriteGroup.name);
                    
                    // Add components
                    Image uiImage = go.AddComponent<Image>();
                    SpriteGroupAnimator animator = go.AddComponent<SpriteGroupAnimator>();
                    
                    // Setup UI Image sprite if available
                    if (spriteGroup.Sprites != null && spriteGroup.Sprites.Length > 0)
                    {
                        uiImage.sprite = spriteGroup.Sprites[0];
                    }

                    // Setup SpriteGroupAnimator
                    animator.uiImage = uiImage;
                    
                    // Add 1 SpriteGroupState to animations
                    SpriteGroupState state = new SpriteGroupState();
                    state.spriteGroup = spriteGroup;
                    state.stateName = !string.IsNullOrEmpty(spriteGroup._Name) ? spriteGroup._Name : spriteGroup.name;
                    animator.animations.Add(state);
                    animator.defaultAnimation = state.stateName;

                    if (parentTransform != null)
                    {
                        go.transform.SetParent(parentTransform, false);
                    }

                    // Register Undo for GameObject creation
                    Undo.RegisterCreatedObjectUndo(go, "Create SpriteGroup Animator");
                    createdObjects.Add(go);
                }
            }

            if (createdObjects.Count > 0)
            {
                Selection.objects = createdObjects.ToArray();
            }
        }

        return DragAndDropVisualMode.Copy;
    }
}
