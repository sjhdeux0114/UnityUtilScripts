using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class CoEventGroup
{
    public float delay;
    public UnityEvent Event;

}
[System.Serializable]
public class CoEventGroupList
{
    public List<CoEventGroup> eventList;
}


public class CoroutineList : MonoBehaviour
{
    public List<CoEventGroupList> Events;
    public CoEventGroupList OnEvent;
    public CoEventGroupList OffEvent;

    public int TestIndex;

    // Start is called before the first frame update
    void Start()
    {

    }
    public void EventCall(int n)
    {
        if (n >= 0 && n < Events.Count)
            StartCoroutine(EventProc(Events[n]));
        else
        {
            Debug.Log($"Wrong Index : {n}");
        }
    }

    IEnumerator EventProc(CoEventGroupList _ev)
    {
        foreach (CoEventGroup rg in _ev.eventList)
        {
            yield return new WaitForSeconds(rg.delay);
            rg.Event.Invoke();

        }
    }

    private void OnEnable()
    {
        StartCoroutine(EventProc(OnEvent));
    }
    private void OnDisable()
    {
        foreach (CoEventGroup rg in OffEvent.eventList)
        {
            rg.Event.Invoke();
        }
    }

    [ContextMenu("Test")]
    public void Test()
    {
        EventCall(TestIndex);

    }

    // Update is called once per frame
    void Update()
    {

    }
}
