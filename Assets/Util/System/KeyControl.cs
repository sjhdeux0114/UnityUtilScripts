using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KeyControl : Singleton<KeyControl>
{
    public KeyCode Key_Coin = KeyCode.C;
    public KeyCode Key_Auto = KeyCode.A;
    public KeyCode Key_Bank = KeyCode.B;
    public KeyCode Key_Bet = KeyCode.F;
    public KeyCode Key_Setting = KeyCode.R;

    public UnityAction Act_Coin;
    public UnityAction Act_Auto;
    public UnityAction Act_Bank;
    public UnityAction Act_Bet;
    public UnityAction Act_Set;
    public UnityAction Act_Exit;

    float MouseTime = 0;

    public void SetKeyCoin(string keyName)
    {
        keyName = keyName.ToUpper();

        switch (keyName)
        {
            case "COIN":
                Act_Coin?.Invoke();
                break;
            case "AUTO":
                Act_Auto?.Invoke();
                break;
            case "BANK":
                Act_Bank?.Invoke();
                break;
            case "BET":
                Act_Bet?.Invoke();
                break;
            case "SET":
                Act_Set?.Invoke();
                break;
            case "EXIT":
                Act_Exit?.Invoke();
                break;

        }

    }
    Vector3 MouseOld;
    private void Update()
    {
        MouseTime += Time.deltaTime;
        if (MouseTime > 10)
        {
            Cursor.visible = false;
        }
        if (MouseOld != Input.mousePosition)
        {
            MouseOld = Input.mousePosition;
            MouseTime = 0;
            Cursor.visible = true;
        }


        if (Input.GetKeyDown(Key_Coin))
        {
            SetKeyCoin("coin");


        }
        if (Input.GetKeyDown(Key_Auto))
        {
            SetKeyCoin("auto");
        }
        if (Input.GetKeyDown(Key_Bank))
        {
            SetKeyCoin("bank");
        }
        if (Input.GetKeyDown(Key_Bet))
        {
            SetKeyCoin("bet");
        }
        if (Input.GetKeyDown(Key_Setting))
        {
            SetKeyCoin("set");
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F10))
        {
            SetKeyCoin("exit");
        }
    }
}
