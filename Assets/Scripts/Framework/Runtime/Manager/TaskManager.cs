using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class GameGlobal
{
    public bool GetTaskState(int type, int taskID)
    {
        string key = $"TaskState_{type}_{taskID}";
        return DataManager.GetDataByBool(key, false);
    }

    public void SetTaskState(int type, int taskID)
    {
        string key = $"TaskState_{type}_{taskID}";
        DataManager.SetDataByBool(key, true);
    }

    public int StartGameCount
    {
        get
        {
            return DataManager.GetDataByInt("StartGameCount");
        }
        set
        {
            DataManager.SetDataByInt("StartGameCount", value);
        }
    }

    public int CompleteGameCount
    {
        get
        {
            return DataManager.GetDataByInt("CompleteGameCount");
        }
        set
        {
            DataManager.SetDataByInt("CompleteGameCount", value);
        }
    }
}

public class TaskManager : Singleton<TaskManager>
{
    public class IDS
    {
        public const int Sign = 0;
        public const int TaskLvMission = 1;
    }

    public static (int reward, int cnt, GameAssetType assetType)[] taskLvMissionRewardArr =
    {
        (10,1,GameAssetType.Coin),
        (30,2,GameAssetType.Coin),
        (60,4,GameAssetType.Coin),
        (100,8,GameAssetType.Coin),
        (200,16,GameAssetType.Coin),
        (400,32,GameAssetType.Coin),
        (500,48,GameAssetType.Coin),
        (600,60,GameAssetType.Coin),
        (800,80,GameAssetType.Coin),
        (1000,100,GameAssetType.Coin)
    };


    public bool GetTaskState(int type, int taskID)
    {
        return GameGlobal.Instance.GetTaskState(type, taskID);
    }

    public void SetTaskState(int type, int taskID)
    {
        GameGlobal.Instance.SetTaskState(type, taskID);
    }

    public void SetLvMissionCompleteState(int taskID)
    {
        SetTaskState(IDS.TaskLvMission, taskID);
    }

    public int GetLvMissionCompleteState(int taskID)
    {
        if (GetTaskState(IDS.TaskLvMission, taskID))
        {
            return 2;
        }
        else if (taskLvMissionRewardArr.TryGet(taskID, out var result) && GameGlobal.Instance.StartGameCount >= result.cnt)
        {
            return 1;
        }
        else
        {
            return 0;
        }

    }

    public bool HasLvMissionCanComplete()
    {
        for (int i = 0; i < taskLvMissionRewardArr.Length; ++i)
        {
            if (GetLvMissionCompleteState(i) == 1)
                return true;
        }

        return false;
    }



}
