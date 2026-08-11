using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewOpenMove", menuName = "OpenMove/NewType", order = 1)]
public class OpenMoveScritable : ScriptableObject
{
    public float MoveTime;
    public float DelayTime;
    public Vector3 Dest_Rot;
    public Vector3 Dest_Scale;
    public AnimationCurve Curve_PosX = new AnimationCurve();
    public AnimationCurve Curve_PosY = new AnimationCurve();
    public AnimationCurve Curve_PosZ = new AnimationCurve();
    public AnimationCurve Curve_Rot = new AnimationCurve();
    public AnimationCurve Curve_Scale = new AnimationCurve();

    public SndClass snd_End;

}
