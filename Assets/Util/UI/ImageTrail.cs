using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageTrail : MonoBehaviour
{
    public Transform TrailRoot;
    public List<Image> imgList = new List<Image>();
    //트레일 이미지 생성할 갯수
    public int TrailCount;
    //트레일 이동 간격
    public float TrailDeltaTime;

    private Image targetImage;
    private RectTransform targetRect;
    bool bInit = false;

    private void Awake()
    {
        if (!bInit)
            InitSetup();
    }

    /// <summary>
    /// OnEnable 시 진행되는 1회 자동 초기 셋팅
    /// </summary>
    public void InitSetup()
    {
        bInit = true;
        targetImage = GetComponent<Image>();
        targetRect = GetComponent<RectTransform>();

        if (TrailRoot == null)
        {
            TrailRoot = transform.parent != null ? transform.parent : transform;
        }

        if (imgList == null)
        {
            imgList = new List<Image>();
        }

        // 유효하지 않은 Null 이미지 제거
        for (int i = imgList.Count - 1; i >= 0; i--)
        {
            if (imgList[i] == null)
            {
                imgList.RemoveAt(i);
            }
        }

        int targetIndex = transform.GetSiblingIndex();

        // TrailCount 개수만큼 Trail Image 오브젝트 생성 및 수량 맞춤
        while (imgList.Count < TrailCount)
        {
            GameObject newObj = new GameObject($"TrailImage_{imgList.Count}");
            newObj.transform.SetParent(TrailRoot, false);

            if (TrailRoot == transform.parent)
            {
                newObj.transform.SetSiblingIndex(Mathf.Max(0, targetIndex));
            }

            Image newImg = newObj.AddComponent<Image>();
            imgList.Add(newImg);
        }

        // 트레일 이미지 속성 설정 및 초기 위치 맞춤
        for (int i = 0; i < imgList.Count; i++)
        {
            if (i < TrailCount)
            {
                imgList[i].gameObject.SetActive(true);
                SetupTrailImage(imgList[i], i);
            }
            else
            {
                imgList[i].gameObject.SetActive(false);
            }
        }
    }

    private void SetupTrailImage(Image img, int index)
    {
        if (img == null) return;

        // 원본 Target UI Image의 Sprite 및 옵션 복사
        if (targetImage != null)
        {
            img.sprite = targetImage.sprite;
            img.type = targetImage.type;
            img.preserveAspect = targetImage.preserveAspect;


        }

        img.raycastTarget = false;

        // RectTransform 위치, 크기, 앵커 정보 초기화
        RectTransform imgRect = img.rectTransform;
        if (targetRect != null)
        {
            imgRect.sizeDelta = targetRect.sizeDelta;
            imgRect.pivot = targetRect.pivot;
            imgRect.anchorMin = targetRect.anchorMin;
            imgRect.anchorMax = targetRect.anchorMax;
        }

        img.transform.position = transform.position;
        img.transform.rotation = transform.rotation;
        img.transform.localScale = transform.localScale;
    }

    private void Update()
    {
        if (imgList == null || imgList.Count == 0 || TrailCount <= 0)
            return;

        // Sprite 변경 감지 시 동기화
        Sprite currentSprite = targetImage != null ? targetImage.sprite : null;

        // TrailDeltaTime 기반의 이동 보간 속도 (TrailDeltaTime이 0 이하일 경우 즉시 이동)
        float t = TrailDeltaTime > 0f ? Mathf.Clamp01(Time.deltaTime / TrailDeltaTime) : 1f;

        Vector3 prevPos = transform.position;
        Quaternion prevRot = transform.rotation;
        Vector3 prevScale = transform.localScale;

        for (int i = 0; i < TrailCount && i < imgList.Count; i++)
        {
            Image img = imgList[i];
            if (img == null || !img.gameObject.activeSelf) continue;

            if (currentSprite != null && img.sprite != currentSprite)
            {
                img.sprite = currentSprite;
            }

            Vector3 currentPos = img.transform.position;
            Quaternion currentRot = img.transform.rotation;
            Vector3 currentScale = img.transform.localScale;

            // 전단계 위치/회전/크기를 추종
            img.transform.position = Vector3.Lerp(currentPos, prevPos, t);
            img.transform.rotation = Quaternion.Lerp(currentRot, prevRot, t);
            img.transform.localScale = Vector3.Lerp(currentScale, prevScale, t);
            // 순서에 따른 투명도(Alpha) 페이딩 효과 적용
            Color col = targetImage.color;
            float alphaFactor = 1f - ((float)(i + 1) / (TrailCount + 1));
            col.a *= alphaFactor;
            img.color = col;



            prevPos = currentPos;
            prevRot = currentRot;
            prevScale = currentScale;
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < imgList.Count; i++)
        {
            imgList[i].enabled = false;
        }
    }
    private void OnEnable()
    {
        for (int i = 0; i < imgList.Count; i++)
        {
            imgList[i].enabled = true;
            SetupTrailImage(imgList[i], i);
        }
    }

}