using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityRateCallback : MonoBehaviour
{
    public event Action onDisable;
    public event Action onEnable;
    public event Action onDestory;

    private void OnDisable()
    {
        onDisable?.Invoke();
    }

    private void OnEnable()
    {
        onEnable?.Invoke();
    }

    private void OnDestroy()
    {
        onDestory?.Invoke();
    }

    private void ClearAllEvent()
    {
        onDisable = null;
        onEnable = null;
        onDestory = null;
    }
}
