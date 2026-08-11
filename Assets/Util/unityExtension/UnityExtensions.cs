using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class UnityExtensions
{
    // ==========================================
    // 1. 유니티 가짜 Null 해결 및 검증 (Safety)
    // ==========================================

    /// <summary> 유니티 오브젝트가 유효한지(Null이 아니고 파괴되지 않았는지) 체크 </summary>
    public static bool IsValid(this Object obj) => obj != null;

    /// <summary> 유니티 Null을 C# 진짜 Null로 변환하여 ?. 연산자를 쓸 수 있게 만듦 </summary>
    public static T GetSafe<T>(this T component) where T : Component
        => component != null ? component : null;


    // ==========================================
    // 2. 활성화 / 비활성화 관련 (Active 상태)
    // ==========================================

    /// <summary> GameObject를 안전하게 활성화/비활성화 </summary>
    public static void SetActiveSafe(this GameObject go, bool isActive)
    {
        if (go != null && go.activeSelf != isActive) go.SetActive(isActive);
    }

    /// <summary> 컴포넌트가 속한 GameObject를 안전하게 활성화/비활성화 </summary>
    public static void SetActiveSafe(this Component comp, bool isActive)
    {
        if (comp != null && comp.gameObject != null && comp.gameObject.activeSelf != isActive)
        {
            comp.gameObject.SetActive(isActive);
        }
    }


    // ==========================================
    // 3. UI 및 텍스트 관련 (UI & Text)
    // ==========================================

    /// <summary> TextMeshPro (UI/3D 공용) 텍스트를 안전하게 변경 </summary>
    public static void SetTextSafe(this TMP_Text tmp, string content)
    {
        if (tmp != null) tmp.text = content;
    }

    /// <summary> 구형 UI Text 텍스트를 안전하게 변경 </summary>
    public static void SetTextSafe(this Text txt, string content)
    {
        if (txt != null) txt.text = content;
    }

    /// <summary> Image 컴포넌트의 Sprite를 안전하게 변경 </summary>
    public static void SetSpriteSafe(this Image img, Sprite sprite)
    {
        if (img != null) img.sprite = sprite;
    }

    /// <summary> CanvasGroup의 알파값과 상호작용 여부를 한 번에 제어 (UI 팝업 켜고 끌 때 최고) </summary>
    public static void SetFadeSafe(this CanvasGroup cg, float alpha, bool interactable)
    {
        if (cg == null) return;
        cg.alpha = alpha;
        cg.interactable = interactable;
        cg.blocksRaycasts = interactable;
    }


    // ==========================================
    // 4. 오디오 관련 (Audio)
    // ==========================================

    /// <summary> AudioSource가 오디오를 재생 중이지 않을 때만 안전하게 재생 (중복 재생 방지) </summary>
    public static void PlaySafe(this AudioSource source, AudioClip clip = null)
    {
        if (source == null) return;
        if (clip != null) source.clip = clip;
        if (!source.isPlaying && source.clip != null) source.Play();
    }


    // ==========================================
    // 5. 트랜스폼 및 좌표 꿀기능 (Transform 숏컷)
    // ==========================================
    // 유니티는 transform.position.x = 10; 처럼 x값만 직접 수정이 안 되기 때문에 
    // 이 메서드들이 있으면 코드가 획기적으로 짧아집니다.

    /// <summary> Transform의 X 좌표만 변경 </summary>
    public static void SetPositionX(this Transform t, float x)
    {
        if (t != null) t.position = new Vector3(x, t.position.y, t.position.z);
    }

    /// <summary> Transform의 Y 좌표만 변경 </summary>
    public static void SetPositionY(this Transform t, float y)
    {
        if (t != null) t.position = new Vector3(t.position.x, y, t.position.z);
    }

    /// <summary> Transform의 로컬 X 좌표만 변경 </summary>
    public static void SetLocalPositionX(this Transform t, float x)
    {
        if (t != null) t.localPosition = new Vector3(x, t.localPosition.y, t.localPosition.z);
    }

    /// <summary> Transform의 모든 자식 오브젝트를 안전하게 파괴 (풀링 안 쓰는 UI 리스트 초기화 시 유용) </summary>
    public static void DestroyAllChildren(this Transform t)
    {
        if (t == null) return;
        for (int i = t.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(t.GetChild(i).gameObject);
        }
    }

    /// <summary> Graphic(Image, Text 등) 컴포넌트의 Color를 안전하게 변경 </summary>
    public static void SetColorSafe(this Graphic graphic, Color color)
    {
        if (graphic != null) graphic.color = color;
    }

    /// <summary> 컴포넌트(Image, Animator 등)의 활성화 상태를 안전하게 제어 </summary>
    public static void SetEnabledSafe(this Behaviour behaviour, bool isEnabled)
    {
        if (behaviour != null) behaviour.enabled = isEnabled;
    }

    /// <summary> Graphic 컴포넌트의 Material을 안전하게 변경 </summary>
    public static void SetMaterialSafe(this MaskableGraphic graphic, Material material)
    {
        if (graphic != null) graphic.material = material;
    }
}