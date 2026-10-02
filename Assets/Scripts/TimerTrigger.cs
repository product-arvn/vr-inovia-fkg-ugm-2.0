using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimerTrigger : MonoBehaviour
{
    [SerializeField] float timer;
    [SerializeField] UnityEvent onTimerEnd;
    float counter;
    
    void Start()
    {
        ResetTimer();
    }

    public void AddAction(UnityAction action)
    {
        onTimerEnd.AddListener(action);
    }

    public void SetTimer(float time)
    {
        timer = time;
        ResetTimer();
    }

    private void OnEnable()
    {
        ResetTimer();
    }

    public void ResetTimer()
    {
        counter = timer;
    }
    
    void Update()
    {
        if (timer == 0)
        {
            return;
        }
        
        if(counter > 0)
        {
            counter -= Time.deltaTime;
        }
        else
        {
            counter = 0;
            onTimerEnd?.Invoke();
        }
    }
}
