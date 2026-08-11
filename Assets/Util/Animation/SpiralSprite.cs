using UnityEngine;

public class SpiralSprite : MonoBehaviour
{
    [Header("Center")]
    public Transform centerTarget;

    [Header("Spiral")]
    public Vector2 V2_radius = new Vector2(1f,1f);
    public float radius = 0f;

    public float rotationSpeed = 360f; // 초당 회전 각도
    public Vector2 V2_Speed = new Vector2(1f,1f);
    public float moveUpSpeed = 2f;
    public Vector2 V2_UpSpeed = new Vector2(1f,1f);
    public float AddScale=0.1f;
    public float AddR_Speed=0.1f;
    public float AddU_Speed=0.1f;

    [Header("Life")]
    public Vector2 V2_LifeTime = new Vector2(1f,1f);
    public float lifeTime = 3f;

    public Vector2 V2_DelayOn = new Vector2(1f,1f);
    float delayTimes=0;

    private float angle;
    private float height;
    public bool isPlaying=false;

    public Vector3 Org_Pos;
    public Vector3 Org_Scale;

    public bool Loop=false;
    public bool PlayOnAwake=false;

    private void Awake()
    {
        Org_Pos = transform.localPosition;
        Org_Scale = transform.localScale;
    }

    private void OnEnable()
    {
        if(PlayOnAwake)
        {
            Init(Org_Pos,Org_Scale);
        }
    }

    public void Init(Vector3 pos,Vector3 scale)
    {
        delayTimes = Random.Range(V2_DelayOn.x,V2_DelayOn.y);
        
        radius = Random.Range(V2_radius.x,V2_radius.y);
        rotationSpeed = Random.Range(V2_Speed.x,V2_Speed.y);
        moveUpSpeed = Random.Range(V2_UpSpeed.x,V2_UpSpeed.y);
        
        transform.localPosition = pos;
        transform.localScale = scale;
        angle = Random.Range(0f, 360f);
        height=0;
        isPlaying=true;
        lifeTime=Random.Range(V2_LifeTime.x,V2_LifeTime.y);
    }

    private void Update()
    {
        if(delayTimes > 0)
        {
            transform.localScale=Vector3.zero;
            delayTimes-=Time.deltaTime;

            if(delayTimes <= 0)
            {
                transform.localScale=Org_Scale;
            }
            return;
        }
        if(!isPlaying)
            return;
        if (centerTarget == null)
            return;

        angle += rotationSpeed * Time.deltaTime;
        height += moveUpSpeed * Time.deltaTime;

        radius += AddR_Speed*Time.deltaTime;
        moveUpSpeed += AddU_Speed*Time.deltaTime;
        
        float rad = angle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            Mathf.Cos(rad) * radius,
            height,
            Mathf.Sin(rad) * radius
        );

        // 위치 적용: 부모-자식 관계에 구애받지 않도록 절대좌표(position) 사용
        transform.position = centerTarget.position + offset;

        // 상승할 때 아래를 보지 않도록, 바라볼 타겟의 Y(높이)값을 현재 오브젝트와 동일하게 맞춰줍니다.
        Vector3 lookPos = centerTarget.position;
        lookPos.y = transform.position.y;
        
        // 중앙 기둥을 바라보도록 회전
        transform.LookAt(lookPos);

        // 기존 Z축 회전(드릴 형태) 대신 Y축 기준으로 제자리 회전(동전/카드처럼 도는 형태)을 추가합니다.
        // LookAt이 매 프레임 방향을 고정시키므로, 계속 증가하는 angle 값을 사용해 누적 회전시킵니다.
        // (자전 속도를 조절하고 싶다면 angle 뒤의 배수(예: 3f)를 변경하세요)
        transform.Rotate(0f, 0f, angle * 3f);
        transform.localScale += Vector3.one*AddScale*Time.deltaTime;

        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0)
        {
            if(!Loop)
            {
                gameObject.SetActive(false);
            }
            else
            {
                Init(Org_Pos,Org_Scale);
            }
        }
    }
}