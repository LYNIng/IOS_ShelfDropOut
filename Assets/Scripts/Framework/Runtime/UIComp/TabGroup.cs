using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TabGroup : MonoBehaviour
{
    public int defaultOnIndex = 0;

    public List<Tab> registedTab;

    /// <summary>
    ///  CurIDX, lastIDX
    /// </summary>
    public event Action<int, int> onClickOn;

    public bool initOnStart = true;

    public int CurIndex { get; set; }

    private void Start()
    {
        if (initOnStart)
        {
            InitTab(defaultOnIndex);
        }
    }

    public void InitTab(int index = 0)
    {
        for (int i = 0; i < registedTab.Count; ++i)
        {
            var tab = registedTab[i];
            tab.Init(this, i, OnTabClick);
            if (i == index)
            {
                CurIndex = i;
                tab.CallOn();
            }
            else
            {
                tab.CallOff();
            }
        }
    }

    private void OnTabClick(Tab tab)
    {
        if (CurIndex == tab.TabIndex)
            return;

        registedTab[CurIndex].CallOff();
        var lastIndex = CurIndex;
        CurIndex = tab.TabIndex;
        tab.CallOn();
        onClickOn?.Invoke(CurIndex, lastIndex);
    }
}
