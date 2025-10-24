using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PassEvent : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{ 
    public enum PassMode
    {
        Block,
        Through,
    }

    public PassMode passMode = PassMode.Block;

    public event Action<List<GameObject>> onPassed;

    //设定了穿透目标后 passMode 只会是 Block 不会额外穿透其他目标
    public GameObject passEventTarget;
    //点击事件
    public void OnPointerClick(PointerEventData eventData)
    {
        //Psss(eventData, ExecuteEvents.pointerClickHandler);
    }
    //按下事件
    public void OnPointerDown(PointerEventData eventData)
    {
        Psss(eventData, ExecuteEvents.pointerClickHandler);
    }
    //弹起事件
    public void OnPointerUp(PointerEventData eventData)
    {
        //Psss(eventData, ExecuteEvents.pointerUpHandler);
    }

    public bool hasPassedEvent = false;
    public void Psss<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function)
        where T : IEventSystemHandler
    {
        if (hasPassedEvent) return;
        hasPassedEvent = true;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);
        GameObject current = data.pointerCurrentRaycast.gameObject;

        List<GameObject> callbackResult = new List<GameObject>();
        //遍历 RayCastResult   
        for (int i = 0; i < results.Count; i++)
        {
            //Debug.Log(results[i].gameObject.name);
            //剔除穿透脚本所在对象
            if (current != results[i].gameObject)
            {

                if (passEventTarget == null)
                {
                    if (passMode == PassMode.Through)
                    {
                        //执行多层点击穿透
                        if (ExecuteEvents.Execute(results[i].gameObject, data, function))
                        {
                            callbackResult.Add(results[i].gameObject);
                        }
                    }
                    else if (passMode == PassMode.Block)
                    {
                        if (ExecuteEvents.Execute(results[i].gameObject, data, function))
                        {
                            callbackResult.Add(results[i].gameObject);
                            break;
                        }

                    }
                }
                else
                {
                    //只执行单层层穿透 点击事件传递成功break
                    if (results[i].gameObject == passEventTarget)
                    {
                        if (ExecuteEvents.Execute(results[i].gameObject, data, function))
                        {
                            callbackResult.Add(results[i].gameObject);
                            break;
                        }
                    }
                }

            }
        }

        onPassed?.Invoke(callbackResult);

        results.Clear();
        hasPassedEvent = false;

    }
}