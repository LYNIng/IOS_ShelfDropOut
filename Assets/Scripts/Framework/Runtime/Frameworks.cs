using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum FrameworksMsg : ushort
{
    FrameworkInited = 0,
    Log,
    LogWarning,
    LogError,
    LogException,

    Max
}

public class Frameworks : MonoSingleton<Frameworks>, IMsgObj
{
    public override bool DontDestory => true;

    public bool Inited { get; private set; }

    public static async Task<bool> AsyncInit()
    {
        if (Instance.Inited) return true;

        bool flag = await Managers.Instance.AsyncInit();

        MessageDispatch.BindMessage(Instance);
        Instance.Inited = true;

        MessageDispatch.CallMessageCommand((ushort)FrameworksMsg.FrameworkInited, Instance);
        return true;
    }

    public void Update()
    {
        if (!Inited) return;

        MessageDispatch.UpdateCall();

    }

    [MsgCallback((ushort)FrameworksMsg.Log)]
    private void Log(object sender, object[] param)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(sender != null ? sender.ToString() : "");
        stringBuilder.Append(" -> ");
        stringBuilder.Append(param != null && param.Length > 0 ? param[0].ToString() : "");
        Debug.Log(stringBuilder.ToString());
    }

    [MsgCallback((ushort)FrameworksMsg.LogError)]
    private void LogError(object sender, object[] param)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(sender != null ? sender.ToString() : "");
        stringBuilder.Append(" -> ");
        stringBuilder.Append(param != null && param.Length > 0 ? param[0].ToString() : "");
        Debug.LogError(stringBuilder.ToString());
    }

    [MsgCallback((ushort)FrameworksMsg.LogWarning)]
    private void LogWarning(object sender, object[] param)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(sender != null ? sender.ToString() : "");
        stringBuilder.Append(" -> ");
        stringBuilder.Append(param != null && param.Length > 0 ? param[0].ToString() : "");
        Debug.LogWarning(stringBuilder.ToString());
    }

    [MsgCallback((ushort)FrameworksMsg.LogException)]
    private void LogException(object sender, object[] param)
    {
        Debug.LogException(param.To<Exception>());
    }
}




