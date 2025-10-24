using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class RedPointMgr : Singleton<RedPointMgr>
{
    public class IDS
    {
        public const string RP_Remove = "rpRemove";
        public const string RP_Eliminate = "rpEliminate";
        public const string RP_Task = "rpTask";
        public const string RP_RewardCoin = "rpRewardCoin";

        //public const string C_RP_SignIn = "rpSignIn";
    }
    //这里如果使用外部注册的模式可以避免一些可能出现的生存周期的问题 或者 改为注册静态函数
    //目前先这样
    private Dictionary<string, Func<bool>> CheckShouldShowFunc = new Dictionary<string, Func<bool>>
    {
        {IDS.RP_Task,()=>{ return TaskManager.Instance.HasLvMissionCanComplete(); } },

    };

    private Dictionary<string, Func<int>> GetNumFunc = new Dictionary<string, Func<int>>
    {
        //{IDS.RP_Remove, ()=>{ return GameGlobal.Instance.GT_Remove; } },
        //{IDS.RP_Eliminate,()=>{ return GameGlobal.Instance.GT_Eliminate; } },

    };

    protected override void OnAwake()
    {
        base.OnAwake();

    }

    private Dictionary<string, List<RedPoint>> dict = new Dictionary<string, List<RedPoint>>();

    public void RefreshRedPoint(string tag)
    {
        if (dict.TryGetValue(tag, out var list))
        {
            if (CheckShouldShow(tag))
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    var num = GetNum(tag);
                    list[i].EnableRedPoint(num > 0 ? num : null);
                }
            }
            else
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    list[i].DisableRedPoint();
                }
            }
        }
    }

    public void EnableRedPoint(string tag)
    {
        if (dict.TryGetValue(tag, out var list) && CheckShouldShow(tag))
        {
            for (int i = 0; i < list.Count; ++i)
            {
                var num = GetNum(tag);

                list[i].EnableRedPoint(num > 0 ? num : null);
            }
        }
    }

    public void DisableRedPoint(string tag)
    {
        if (dict.TryGetValue(tag, out var list))
        {
            for (int i = 0; i < list.Count; ++i)
            {
                list[i].DisableRedPoint();
            }
        }
    }

    public void RegistRP(RedPoint redPoint)
    {
        if (!dict.TryGetValue(redPoint.redPointTag, out var resultList))
        {
            resultList = new List<RedPoint>();

            dict.Add(redPoint.redPointTag, resultList);
        }
        resultList.Add(redPoint);

    }

    public void UnRegistRP(RedPoint redPoint)
    {
        if (dict.TryGetValue(redPoint.redPointTag, out var resultList)
            && resultList.Remove(redPoint))
        {

        }
    }

    public bool CheckShouldShow(string tag)
    {
        if (CheckShouldShowFunc.TryGetValue(tag, out var func))
        {
            var result = func?.Invoke();
            return result.Value;
        }
        return false;
    }

    public int GetNum(string tag)
    {
        if (GetNumFunc.TryGetValue(tag, out var func))
        {
            var result = func?.Invoke();
            return result.Value;
        }
        return 0;
    }
}


public class RedPoint : MonoBehaviour
{
    public string redPointTag;

    public Image redPoint;
    public Image nonePoint;
    public TMPro.TextMeshProUGUI txtNum;


    public void EnableRedPoint(int? num)
    {
        if (redPoint != null)
        {
            redPoint.gameObject.SetActive(true);
        }
        if (txtNum != null && num.HasValue)
        {
            var result = num.Value;
            txtNum.gameObject.SetActive(true);
            txtNum.text = result.ToString();
        }
        if (nonePoint != null)
        {
            nonePoint.gameObject.SetActive(false);
        }
    }

    public void DisableRedPoint()
    {
        if (redPoint != null)
        {
            redPoint.gameObject.SetActive(false);
        }
        if (txtNum != null)
        {
            txtNum.gameObject.SetActive(false);
        }
        if (nonePoint != null)
        {
            nonePoint.gameObject.SetActive(true);
        }
    }

    private void Awake()
    {
        if (!string.IsNullOrEmpty(redPointTag))
            RedPointMgr.Instance.RegistRP(this);
    }

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(redPointTag))
            RedPointMgr.Instance.UnRegistRP(this);
    }

    private void Start()
    {
        if (redPoint != null)
        {
            redPoint.gameObject.SetActive(false);
        }
        if (txtNum != null)
        {
            txtNum.gameObject.SetActive(false);
        }
        if (nonePoint != null)
        {
            nonePoint.gameObject.SetActive(false);
        }
    }
}
