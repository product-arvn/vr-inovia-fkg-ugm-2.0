using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EyeTrackingDebug : MonoBehaviour
{

    [SerializeField] private TMP_Text debugTex;
    private bool isTextChange;


    public void DebugThis(string message)
    {
#if UNITY_EDITOR
        Debug.Log(message);
#endif

        debugTex.text = message;


        Invoke(nameof(CallTextChange), 5f);
    }

    private void CallTextChange()
    {
        isTextChange = true;
    }

    private void Update() 
    {
        if (isTextChange)
        {
            debugTex.text = ".....";
            isTextChange = false;
        }
    }


    
    
}
