using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(SimpleSpriteAnimator))]
[RequireComponent(typeof(MovePathPos))]
public class Ani_Move_Control : MonoBehaviour
{
    public SimpleSpriteAnimator animator;
    public MovePathPos M_Pos;

    public string[] Start_aniName;
    public string[] End_aniName;

    public UnityAction Act_End;
    public UnityAction<string> Act_Start;
    public bool bAwake = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        _Init();
        if (bAwake)
        {
            Play();
        }
    }

    public void Init_String()
    {
        Start_aniName = new string[M_Pos.TargetPoints.Count - 1];
        End_aniName = new string[M_Pos.TargetPoints.Count - 1];


        for (int i = 0; i < Start_aniName.Length; i++)
        {
            Start_aniName[i] = animator.StartAnimation;
            End_aniName[i] = "";
        }
    }

    bool bInit = false;
    public void _Init()
    {
        if (bInit) return;
        bInit = true;
        if(animator == null)
            animator = GetComponent<SimpleSpriteAnimator>();
        if (M_Pos == null)
            M_Pos = GetComponent<MovePathPos>();
        animator._Init_Dic();

        animator.Act_End = null;
        animator.Act_Start = null;
        M_Pos.Act_Start = null;
        M_Pos.Act_Next = null;

        animator.Act_End = Ani_End;
        animator.Act_Start = Ani_Start;
        M_Pos.Act_Start = StartMove;
        M_Pos.Act_Next = NextMove;


    }

    public void Ani_Start(string aniName)
    {
        Debug.Log($"Ani End : {aniName}");
        if (Act_Start != null)
            Act_Start.Invoke(aniName);
    }

    public void Ani_End(string aniName)
    {
        Debug.Log($"Ani End : {aniName}");
        if (Act_End != null)
            Act_End.Invoke();
    }
    public void Play(int n=-1)
    {

        if(n >= 0)
        {
            M_Pos.PlayIndex(n);
        }
        else
        {
            M_Pos.Play();
        }
    }

    public void _Update(float dt)
    {
        if (animator == null) return;
        if (M_Pos == null) return;

        if (!Application.isPlaying)
        {
            animator._Update(dt);
            M_Pos._Update(dt);

        
            if (M_Pos.StartCall >= 0)
            {
                StartMove(M_Pos.StartCall);
                M_Pos.StartCall = -1;

            }
            if (M_Pos.NextCall >= 0)
            {
                NextMove(M_Pos.NextCall);
                M_Pos.NextCall = -1;
            }
        }
    }


    public void StartMove(int n)
    {
        Debug.Log($"start move : {n}");
        if (n < 0 || n >= Start_aniName.Length) return;
        animator.Play(Start_aniName[n]);
    }
    public void NextMove(int n)
    {
        Debug.Log($"next move : {n}");
        if (n < 0 || n >= End_aniName.Length) return;
        animator.Play(End_aniName[n]);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
