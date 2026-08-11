using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public delegate void EndEvent();

/// <summary>
/// ZoomOn
/// - 곡선(Curve/alpha)에 따라 Scale/Color/Move/Rotate를 애니메이션.
/// - 지연 시작/자동 숨김/루프/역재생/로테이션 루프/사운드 호출 등 원본 기능 유지.
/// - 외부에서 Play/PlayOnce/PlayBackward/Stop/_Reset/_Update 호출 패턴 그대로 사용.
/// 
/// ⚠️주의:
///  - Org_Pos/Org_Rot은 local 기준으로 저장되지만, _Reset()에서는 transform.position(월드)와 EulerAngles(월드 로테이션)를 사용합니다. (원본 동작 보존)
///  - RotatePower != Vector3.zero && !bRotateLoop 시, Quaternion.Lerp(..., 1)은 사실상 TargetRot*p로 직접 설정하는 효과입니다. (원본 그대로 유지)
/// </summary>
public class ZoomOn : MonoBehaviour
{
    // ====== Inspector Fields (Headers/Tooltips만 추가, 동작 불변) ======

    [Header("Timeline & Curves")]
    [Tooltip("애니메이션 총 시간(초)")]
    public float OnTime;

    [Tooltip("Scale 변화 곡선 (x:0~1 시간, y:스케일)")]
    public AnimationCurve Curve = new AnimationCurve();

    [Tooltip("Alpha 변화 곡선 (x:0~1 시간, y:알파)")]
    public AnimationCurve alpha = new AnimationCurve();

    [Header("Transform Offsets (Local)")]
    [Tooltip("로컬 기준 이동량")]
    public Vector3 MovePower;

    [Tooltip("로컬 기준 회전량(도)")]
    public Vector3 RotatePower;

    [Header("Play & Loop")]
    [Tooltip("오브젝트 활성화 시 자동 재생")]
    public bool bStart = false;

    [Tooltip("애니메이션 종료 후 다시 처음부터 반복")]
    public bool bLoop = false;

    [Tooltip("현재 재생 중 플래그(런타임 상태 표시)")]
    public bool bOn = false;

    [Header("Action & Events")]
    [Tooltip("Play 호출 시 비교되는 액션 이름")]
    public string _ActionName = "Zoom";

    [Tooltip("애니메이션 종료 시 호출되는 이벤트")]
    public EndEvent _Event;

    [Header("Delays")]
    [Tooltip("시작 지연(초)")]
    public float delay = 0;

    [Tooltip("이동(Move) 시작 지연(초)")]
    public float Movedelay = 0;

    [Tooltip("지연 동안 강제 투명 처리")]
    public bool b_delayHide = false;

    [Header("Audio")]
    [Tooltip("시작 시 재생할 사운드")]
    public SndClass OnSound = null;

    [Tooltip("종료 시 재생할 사운드")]
    public SndClass OffSound = null;

    [Header("Start/Hide Options")]
    [Tooltip("시작 시 Scale 0으로 숨김")]
    public bool b_Start_Hide = false;

    [Tooltip("끝나면 GameObject 비활성화")]
    public bool b_AutoHide = false;

    [Tooltip("끝나면 Renderer들만 비활성화(Image/Text/SpriteRenderer/TMP)")]
    public bool b_AutoHideImg = false;

    [Header("Positioning & Update")]
    [Tooltip("시작 시 원점 복원 및 목적지 계산 사용(localPosition 기반)")]
    public bool b_SavePos = true;

    [Tooltip("Unity Update 대신 내부 60fps 코루틴으로 업데이트")]
    public bool b_None_Timer = false;

    [Header("Effect Toggles")]
    [Tooltip("색/알파 애니메이션 비활성화")]
    public bool bNoColor = false;

    [Tooltip("애니메이션 종료 후 숨김까지 지연(초)")]
    public float HideDelay;

    [Tooltip("역재생(시간은 순방향, p만 반전)")]
    public bool bBackward = false;

    [Tooltip("스케일 애니메이션 비활성화")]
    public bool bDontSize = false;

    [Tooltip("회전 루프(초당 RotatePower 누적 회전)")]
    public bool bRotateLoop = false;

    [Header("Color (Runtime)")]
    [Tooltip("현재 색상(런타임 상태)")]
    public Color col;

    // ====== Internal State (원본 변수/동작 유지) ======
    public Vector3 Org_Pos;                             // 시작 localPosition (초기화 시 저장)
    public Vector3 Org_Rot;                             // 시작 localEulerAngles (초기화 시 저장)
    Vector3 TargetPos;                           // 목적지 localPosition = 시작 + MovePower
    Vector3 TargetRot;                           // 목적지 localEulerAngles = 시작 + RotatePower
    public Vector3 OrgScale = Vector3.one;

    float LastTime;                              // 진행 시간 누적
    Text text_t = null;                          // uGUI Text
    TextMeshProUGUI _text_m_t = null;            // TextMeshPro UGUI
    Image image_t = null;                        // uGUI Image
    SpriteRenderer SprRender = null;             // SpriteRenderer

    public bool bSub;
    public List<Image> SubImage = new List<Image>();

    float delayTime = 0;                         // 내부용 지연 타이머
    bool bStop = false;                          // Stop()으로 종료 예약 플래그
    bool b_Loop = false;                         // b_None_Timer일 때 내부 루프 플래그
    bool bInit = false;                          // _Init 1회 보장

    void Awake() { /* 원본 그대로 비워둠 */ }

    /// <summary>
    /// 내부 초기화: 렌더러 참조/초기 색/시작 플래그/원점 저장
    /// </summary>
    public void _Init()
    {
        // 호환되는 렌더러 탐색(Text -> Image -> SpriteRenderer 순)
        text_t = GetComponent<Text>();
        if (!text_t)
        {
            image_t = GetComponent<Image>();
            if (!image_t)
            {
                SprRender = GetComponent<SpriteRenderer>();
            }
        }
        _text_m_t = GetComponent<TextMeshProUGUI>();

        // 초기 색 저장 (우선순위: Text > Image > SpriteRenderer > TMP)
        if (text_t) col = text_t.color;
        if (image_t) col = image_t.color;
        if (SprRender) col = SprRender.color;
        if (_text_m_t) col = _text_m_t.color;

        // 활성화 즉시 시작 옵션
        if (bStart)
        {
            LastTime = 0;
            bOn = true;
        }

        // 시작 원점/회전(로컬) 저장
        Org_Pos = transform.localPosition - MovePower;
        Org_Rot = transform.localEulerAngles;
    }

    /// <summary>외부에서 색을 즉시 교체(렌더러 우선순위 동일)</summary>
    public void SetColor(Color c)
    {
        if (text_t) { text_t.color = c; col = text_t.color; }
        if (image_t) { image_t.color = c; col = image_t.color; }
        if (SprRender) { SprRender.color = c; col = SprRender.color; }
        if (_text_m_t) { _text_m_t.color = c; col = _text_m_t.color; }
    }

    void Start() { /* 원본 그대로 비워둠 */ }

    void GetSubData()
    {
        SubImage = transform.GetComponentsInChildren<Image>().ToList<Image>();
    }

    void OnEnable()
    {
        if (!bInit) { bInit = true; _Init(); }
        SubImage.Clear();
        if (bSub) { GetSubData(); }
        if (bStart) { Play(); }
    }

    private void OnDisable()
    {
        // 코루틴/루프 종료 예약 (기존 Stop 동작 유지)
        Stop();
    }

    /// <summary>
    /// 즉시 종료 예약: 다음 _Update에서 종료 처리되도록 플래그 설정
    /// (LastTime을 OnTime-0.1로 설정해 빠르게 끝나도록)
    /// </summary>
    public void Stop()
    {
        b_Loop = false;
        LastTime = OnTime - 0.1f;
        bStop = true;
    }

    /// <summary>
    /// 시작 프레임 설정(스케일/이동/회전/알파를 p=0.01f 상태로 세팅)
    /// </summary>
    void SetStart()
    {
        LastTime = 0.01f;
        float p = LastTime / OnTime;
        float ret_t = Curve.Evaluate(p);

        if (!bDontSize)
            transform.localScale = OrgScale * ret_t;

        transform.localEulerAngles = Org_Rot - RotatePower;

        if (b_SavePos)
            transform.localPosition += MovePower * 0.01f; // 원본 동작: world position에 더함


        if (!bNoColor)
        {
            if (text_t) { col.a = alpha.Evaluate(p); text_t.color = col; }
            else if (image_t) { col.a = alpha.Evaluate(p); image_t.color = col; }
            else if (SprRender) { col.a = alpha.Evaluate(p); SprRender.color = col; }
            else if (_text_m_t) { col.a = alpha.Evaluate(p); _text_m_t.color = col; }
        }
    }

    /// <summary>역재생 시작(기존 Play 호출 후 bBackward=true)</summary>
    public void PlayBackward(string _Name = "Zoom")
    {
        Play(_Name);
        bBackward = true;
        transform.localEulerAngles = Org_Rot;
    }

    /// <summary>
    /// 애니메이션 시작(이미 켜져 있어도 매번 새로 셋업)
    /// - 위치/회전 원점 복원(b_SavePos가 true일 때)
    /// - TargetPos/TargetRot 계산
    /// - 렌더러 enable, 사운드 재생(지연 조건)
    /// - b_None_Timer면 코루틴 루프 시작
    /// </summary>
    public void Play(string _Name = "Zoom")
    {
        bBackward = false;
        gameObject.SetActive(true);

        if (_Name.Equals(_ActionName))
        {
            if (b_SavePos)
                transform.localPosition = Org_Pos;

            transform.localEulerAngles = Org_Rot - RotatePower;

            TargetPos = transform.localPosition + MovePower;
            TargetRot = Org_Rot;

            SetStart();
            bStop = false;
            delayTime = delay;
            bOn = true;
            LastTime = 0;

            if (b_Start_Hide)
                transform.localScale = Vector3.zero;

            // 렌더러 활성화
            if (image_t) image_t.enabled = true;
            if (text_t) text_t.enabled = true;
            if (SprRender) SprRender.enabled = true;
            if (_text_m_t) _text_m_t.enabled = true;


            // 즉시 시작 시점의 사운드
            if (delayTime <= 0)
            {
                if (OnSound.Clip)
                {
                    if (SoundManager.Instance)
                        SoundManager.Instance.Play_Effect(OnSound);
                }
            }

            // 타임라인 미사용 모드(자체 60fps 루프)
            if (b_None_Timer)
            {
                b_Loop = true;
                StartCoroutine(update_NoneTime());
            }
        }
    }

    /// <summary>
    /// 아직 재생 중이 아닐 때만 시작 (한 번만)
    /// </summary>
    public void PlayOnce(string _Name = "Zoom")
    {
        bBackward = false;

        if (_Name.Equals(_ActionName) && !bOn)
        {
            SetStart();
            bStop = false;
            delayTime = delay;
            bOn = true;
            LastTime = 0;

            if (b_Start_Hide)
                transform.localScale = Vector3.zero;

            if (delayTime <= 0)
            {
                if (OnSound.Clip)
                {
                    if (SoundManager.Instance)
                        SoundManager.Instance.Play_Effect(OnSound);
                }
            }

            if (b_None_Timer)
            {
                b_Loop = true;
                StartCoroutine(update_NoneTime());
            }
        }
    }

    /// <summary>
    /// b_None_Timer 모드일 때 사용하는 고정 틱 코루틴(약 60fps)
    /// </summary>
    IEnumerator update_NoneTime()
    {
        while (b_Loop)
        {
            _Update(0.01666f);
            yield return new WaitForSecondsRealtime(0.01666f);
        }
    }

    /// <summary>
    /// 값 초기화(위치/회전/스케일/알파를 0 시점으로)
    /// </summary>
    public void _Reset()
    {
        transform.localPosition = Org_Pos;       // 원본: local을 월드에 대입
        transform.localEulerAngles = Org_Rot;    // 원본: 로컬 각도를 월드각으로 대입

        float ret_t = 0;
        if (Curve.keys.Length > 0)
            ret_t = Curve.Evaluate(0);

        if (!bDontSize)
            transform.localScale = OrgScale * ret_t;

        if (!bNoColor)
        {
            col.a = alpha.Evaluate(0);
            if (text_t) text_t.color = col;
            else if (image_t) image_t.color = col;
            else if (SprRender) SprRender.color = col;
            else if (_text_m_t) _text_m_t.color = col;
        }
    }

    /// <summary>
    /// 외부에서 DeltaTime을 넘겨 수동 업데이트할 수 있음(b_None_Timer 또는 외부 타이밍 시스템)
    /// </summary>
    public virtual void _Update(float DeltaTime)
    {
        if (bOn)
        {
            // --- 시작 지연 처리 ---
            delayTime -= DeltaTime;
            if (delayTime > 0)
            {
                if (b_delayHide)
                {
                    // 지연 동안 완전 투명 표시
                    Color hidecol = new Color(1, 1, 1, 0);
                    if (text_t) text_t.color = hidecol;
                    else if (image_t) image_t.color = hidecol;
                    else if (SprRender) SprRender.color = hidecol;
                    else if (_text_m_t) _text_m_t.color = hidecol;

                    if (bSub)
                    {
                        foreach (Image im in SubImage.ToArray())
                        {
                            im.color = hidecol;
                        }
                    }
                }

                // 지연 시작과 동시에 OnSound 재생
                if (delay <= 0)
                {
                    if (OnSound.Clip)
                    {
                        if (SoundManager.Instance)
                            SoundManager.Instance.Play_Effect(OnSound);
                    }
                }
                return;
            }

            // --- 본 애니메이션 타임라인 ---
            if (OnTime <= 0) OnTime = 1;
            LastTime += DeltaTime;

            if (LastTime >= OnTime)
            {
                if (bStop)
                {
                    // Stop() 예약으로 종료
                    b_Loop = false;
                    bStop = false;
                    if (RotatePower != Vector3.zero)
                        transform.eulerAngles = Org_Rot; // 원본 동작 유지
                    ActionEnd();
                }
                else
                {
                    if (bLoop)
                    {
                        // 다시 시작
                        LastTime = 0;
                        // (원본: 위치/회전 원점 복원 주석처리)
                    }
                    else
                    {
                        // 루프가 아니면 종료
                        ActionEnd();
                    }
                }
            }

            // 진행도 p (0~1), 역재생이면 반전
            float p = Mathf.Clamp01(LastTime / OnTime);

            if (p >= 1) p = 1;
            if (bBackward) p = 1 - p;

            float ret_t = Curve.Evaluate(p);


            // Scale
            if (!bDontSize)
                transform.localScale = OrgScale * ret_t;

            // Move (Movedelay 이후부터 비율 mp로 보간)
            if (b_SavePos)
            {
                float mp = 0;
                float MoveOnTime = OnTime - Movedelay;
                if (MoveOnTime < 0)
                {
                    Movedelay = OnTime - 0.2f;
                    MoveOnTime = 0.2f;
                }
                if (LastTime >= Movedelay)
                {
                    mp = (LastTime - Movedelay) / MoveOnTime;
                }
                if (bBackward)
                    transform.localPosition = Vector3.Lerp(TargetPos, Org_Pos, mp);
                else
                    transform.localPosition = Vector3.Lerp(Org_Pos, TargetPos, mp);
            }

            // Rotate
            if (RotatePower != Vector3.zero)
            {
                float dir = 1;
                if (bBackward) dir = -1;
                if (bRotateLoop)
                {
                    transform.localEulerAngles += RotatePower * DeltaTime * dir;   // 누적 회전
                }
                else
                {
                    if (bBackward)
                    {

                        // Lerp(..., 1) => 사실상 TargetRot * p 각도로 설정(원본 유지)
                        transform.localEulerAngles = Vector3.Lerp(TargetRot - RotatePower, TargetRot, 1 - p);

                    }
                    else
                    {

                        // Lerp(..., 1) => 사실상 TargetRot * p 각도로 설정(원본 유지)

                        transform.localEulerAngles = Vector3.Lerp(TargetRot - RotatePower, TargetRot, p);

                    }
                }
            }

            // Color/Alpha
            if (!bNoColor)
            {
                if (text_t) { col.a = alpha.Evaluate(p); text_t.color = col; }
                else if (image_t) { col.a = alpha.Evaluate(p); image_t.color = col; }
                else if (SprRender) { col.a = alpha.Evaluate(p); SprRender.color = col; }
                else if (_text_m_t) { col.a = alpha.Evaluate(p); _text_m_t.color = col; }

                if (bSub)
                {
                    col.a = alpha.Evaluate(p);
                    foreach (Image im in SubImage.ToArray())
                    {

                        im.color = col;
                    }
                }
            }
        }
        else
        {
            // 종료 후 HideDelay 카운트다운
            LastTime = 0;
            if (delayTime > 0)
            {
                delayTime -= DeltaTime;
                if (delayTime < 0)
                {
                    _HideProc();
                }
            }
        }
    }

    /// <summary>Hide 처리(렌더러 비활성/오브젝트 비활성/이벤트/오프 사운드)</summary>
    void _HideProc()
    {
        if (b_AutoHideImg)
        {
            if (image_t) image_t.enabled = false;
            if (text_t) text_t.enabled = false;
            if (SprRender) SprRender.enabled = false;
            if (_text_m_t) _text_m_t.enabled = false;
        }

        if (b_AutoHide)
        {
            if (Application.isPlaying)
                gameObject.SetActive(false);
        }

        if (_Event != null)
            _Event();

        if (OffSound.Clip)
        {
            if (SoundManager.Instance)
                SoundManager.Instance.Play_Effect(OffSound);
        }
    }

    /// <summary>애니메이션 종료 시점 처리: HideDelay 세팅 및 bOn=false</summary>
    void ActionEnd()
    {
        delayTime = HideDelay;
        if (delayTime <= 0)
            delayTime = 0.01f; // 최소 딜레이 보장
        bOn = false;
    }

    /// <summary>Unity Update: b_None_Timer가 false일 때만 내부 타임라인 갱신</summary>
    void Update()
    {
        if (!b_None_Timer)
        {
            _Update(Time.deltaTime);
        }
    }
}
