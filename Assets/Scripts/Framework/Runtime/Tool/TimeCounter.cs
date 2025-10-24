using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeCounter : MonoBehaviour
{
    private Action onTimeEnd;

    public bool UseUnscaledDeltaTime { get; set; } = false;

    public bool IsStartCounter { get; private set; }

    public float TimeCount { get; private set; }

    public float TimeLimit { get; private set; }
    public void StartCounter(float timeLimit, Action onTimeEnd)
    {
        if (IsStartCounter)
            return;
        TimeCount = 0;
        TimeLimit = timeLimit;
        IsStartCounter = true;
        this.onTimeEnd = onTimeEnd;
    }

    public void StopCounter()
    {
        TimeCount = 0;
        TimeLimit = 0;
        IsStartCounter = false;
        onTimeEnd = null;
    }

    public void Update()
    {
        if (!IsStartCounter)
        {
            return;
        }

        TimeCount += UseUnscaledDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime;

        if (TimeCount >= TimeLimit)
        {
            TimeCount = 0;
            IsStartCounter = false;
            onTimeEnd?.Invoke();
        }
    }
}
