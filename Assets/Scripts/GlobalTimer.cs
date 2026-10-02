using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalTimer : MonoBehaviour
{
    public float Timer { get; private set; }
    public bool IsPaused { get; private set; } = true;
    
    void Update()
    {
        if (!IsPaused)
        {
            Timer += Time.deltaTime;
        }
    }

    public void StartTimer()
    {
        IsPaused = false;
    }

    public void StopTimer()
    {
        Timer = 0;
        IsPaused = true;
    }

}
