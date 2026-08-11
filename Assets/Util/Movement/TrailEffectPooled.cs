using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrailEffectPooled : MonoBehaviour
{
    public Transform TrailRoot;
    public float trailInterval = 0.05f;
    public float fadeOutTime = 0.4f;
    public int poolSize = 20;
    public Color TrailColor = Color.white;

    private float nextTrailTime;
    public List<Image> trailPool;
    private int poolIndex = 0;
    Image myImg;

    // 이전 프레임의 위치를 저장할 변수
    private Vector3 lastPosition;
    public bool bTrailOn = true;

    void Start()
    {
        myImg = GetComponent<Image>();
        //InitializePool();
        // 초기 위치를 저장합니다.
        lastPosition = transform.position;
    }
    [ContextMenu("Delete_ChildScript Pool")]
    public void Delete_ChildScript()
    {
        foreach(Image child in trailPool.ToArray())
        {
            DestroyImmediate(child.gameObject); // Remove the TrailEffectPooled component from the trail object
        }

        trailPool.Clear();



    }
    [ContextMenu("Initialize Pool")]
    public void InitializePool()
    {

        trailPool = new List<Image>();

        Image originalImage = GetComponent<Image>();
        Sprite originalSprite = originalImage.sprite;


        for (int i = 0; i < poolSize; i++)
        {
            GameObject trailObject = new GameObject($"{gameObject.name}_Trail_" + i);

            trailObject.transform.SetParent(TrailRoot, false);

            // 3. Add the Image component and configure it.
            Image trailImage = trailObject.AddComponent<Image>();
            trailImage.sprite = originalSprite;
            trailImage.color = TrailColor;

            // Make sure the RectTransform is properly set up.
            RectTransform originalRect = GetComponent<RectTransform>();
            RectTransform trailRect = trailObject.GetComponent<RectTransform>();

            trailRect.sizeDelta = originalRect.sizeDelta;
            trailRect.anchorMin = originalRect.anchorMin;
            trailRect.anchorMax = originalRect.anchorMax;
            trailRect.pivot = originalRect.pivot;

            trailImage.enabled = false;
            trailPool.Add(trailImage);
            
        }
    }

    void Update()
    {
        if(!bTrailOn) return;
        // 1. 현재 위치와 이전 위치를 비교하여 이동했는지 확인
        if (Vector3.Distance(transform.position, lastPosition) > 0.01f)
        {
            // 2. 이동했을 경우, 잔상 생성 로직 실행
            if (Time.time > nextTrailTime)
            {
                nextTrailTime = Time.time + trailInterval;
                CreateTrailFromPool();
            }
        }

        // 3. 현재 위치를 다음 프레임을 위해 저장
        lastPosition = transform.position;
    }

    void CreateTrailFromPool()
    {
        // Get the next object from the pool
        Image trailImage = trailPool[poolIndex];

        // Reset the object's position and rotation
        trailImage.transform.position = transform.position;
        trailImage.transform.rotation = transform.rotation;
        trailImage.sprite = myImg.sprite;
        // Set the color to fully visible

        trailImage.color = TrailColor;

        // Activate the object
        trailImage.enabled = true;

        // Start the fade-out coroutine
        StartCoroutine(FadeOut(trailImage));

        // Increment the index, wrapping around if it exceeds the pool size
        poolIndex = (poolIndex + 1) % poolSize;
    }

    System.Collections.IEnumerator FadeOut(Image image)
    {
        float startTime = Time.time;
        Color startColor = image.color;

        while (Time.time < startTime + fadeOutTime)
        {
            float t = (Time.time - startTime) / fadeOutTime;
            Color newColor = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(TrailColor.a, 0, t));
            image.color = newColor;
            yield return null;
        }

        // Deactivate the object instead of destroying it
        image.enabled = false;
    }
}