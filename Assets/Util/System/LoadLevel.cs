using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LoadLevel : MonoBehaviour
{
    public int Next_Scene_Number = 0;
    public bool bWake = true;
    public KeyCode NextKey;
    public UnityEvent _Event;
    
    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.5f);
        if(bWake)
            SceneManager.LoadScene(Next_Scene_Number);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(NextKey))
        {
            _Event.Invoke();
            SceneManager.LoadScene(Next_Scene_Number);
        }
    }
}
