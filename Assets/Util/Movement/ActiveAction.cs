using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActiveAction : MonoBehaviour
{
    public UnityEvent _OnEvent;
    public UnityEvent _DisableEvent;
    private void OnEnable()
    {
        _OnEvent.Invoke();


    }

    private void OnDisable()
    {
        _DisableEvent.Invoke();
    }
}
