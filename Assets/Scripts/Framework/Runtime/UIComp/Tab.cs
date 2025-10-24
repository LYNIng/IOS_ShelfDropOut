using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tab : MonoBehaviour, IPointerClickHandler
{
    public GameObject on;
    public GameObject off;

    public int TabIndex { get; private set; }

    private TabGroup _tabGroup;
    private Action<Tab> onTabClick;

    public Action<Tab> onStatusChanged;
    public void OnPointerClick(PointerEventData eventData)
    {
        CallClick();
    }

    public void Init(TabGroup tabGroup, int index, Action<Tab> onClick)
    {
        _tabGroup = tabGroup;
        onTabClick = onClick;
        TabIndex = index;

    }
    private bool isClicked;
    public void CallClick()
    {
        SoundPlayer.Instance.PlaySound(SoundName.Click);
        this.ClickScaleAni(
            flag => isClicked = flag,
            () => isClicked,
            (t) =>
            {
                onTabClick?.Invoke(this);
            });
    }

    public void CallOn()
    {
        on?.gameObject.SetActive(true);
        off?.gameObject.SetActive(false);
        onStatusChanged?.Invoke(this);
    }

    public void CallOff()
    {
        on?.gameObject.SetActive(false);
        off?.gameObject.SetActive(true);
        onStatusChanged?.Invoke(this);
    }

}
