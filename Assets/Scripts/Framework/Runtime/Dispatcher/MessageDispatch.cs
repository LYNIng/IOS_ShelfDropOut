using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
//using System.Runtime.Remoting.Channels;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;


public interface IMsgObj
{

}

public class MessageDispatch
{
    private static ObjectPool<MessageTask> taskPool = new ObjectPool<MessageTask>(
        () => new MessageTask(),
        (task) => task.Clear(),
        (task) => task.Clear(),
        (task) => task.Clear(), false, 100, 100);


    private class MessageTask : IComparer<MessageTask>
    {
        public ushort message;
        public object[] param;
        public object sender;
        public object recver;

        public int channel = -1;

        public int priority = 0;

        public TimeSpan delay;

        public DateTime enterTime;
        public void Clear()
        {
            message = 0;
            param = null;
            sender = null;
            recver = null;
            channel = -1;
            priority = 0;
            delay = TimeSpan.Zero;

        }

        public int Compare(MessageTask x, MessageTask y)
        {
            var l = x.enterTime + x.delay;
            var r = y.enterTime + y.delay;

            if (l > r) return 1;
            else if (l < r) return -1;
            else return 0;
        }
    }

    private class MessageItem
    {
        public ushort message;
        public object listener;
        public int channel;
        public Action<object, object[]> callback;
        public Func<object, object[], Task> taskCallbackFunc;
    }

    private class MessageChannel
    {
        public int channel;
        public Dictionary<ushort, List<MessageItem>> msgDict = new Dictionary<ushort, List<MessageItem>>();
        public Dictionary<object, List<MessageItem>> listenerDict = new Dictionary<object, List<MessageItem>>();
    }

    private static Dictionary<int, MessageChannel> channels = new Dictionary<int, MessageChannel>();

    private static PriorityQueue<MessageTask, int> priorityQueue = new PriorityQueue<MessageTask, int>();

    private static Queue<MessageTask> excuteTaskQueue = new Queue<MessageTask>();

    private static PriorityQueue<MessageTask, MessageTask> messageTaskTimerList = new PriorityQueue<MessageTask, MessageTask>(Comparer<MessageTask>.Create(new Comparison<MessageTask>((l, r) =>
    {
        var ls = l.enterTime + l.delay;
        var rs = r.enterTime + r.delay;
        if (ls > rs) return 1;
        else if (ls < rs) return -1;
        else return 0;

    })));

    #region Static Func
    private static void EnqueueMessageTaskToTimer(MessageTask task)
    {
        lock (messageTaskTimerList)
        {
            messageTaskTimerList.Enqueue(task, task);
        }
    }
    private static void EnqueueMessageTask(MessageTask item)
    {
        lock (priorityQueue)
        {
            priorityQueue.Enqueue(item, item.priority);
        }

    }
    public static void CallMessageCommand(ushort message, object sender = null, object recver = null, TimeSpan delay = default, int priority = 0, int channel = -1, params object[] param)
    {
        var task = taskPool.Get();
        task.message = message;
        task.sender = sender;
        task.recver = recver;
        task.param = param;
        task.channel = channel;
        task.delay = delay;
        task.priority = priority;
        task.enterTime = DateTime.Now;
        if (delay == default || delay == TimeSpan.Zero)
        {
            EnqueueMessageTask(task);
        }
        else
        {
            EnqueueMessageTaskToTimer(task);
        }
    }

    public static async Task AsyncCallMessageCommand(ushort message, object sender = null, object recver = null, int channel = -1, params object[] param)
    {
        var task = taskPool.Get();
        task.message = message;
        task.sender = sender;
        task.recver = recver;
        task.param = param;
        task.channel = channel;

        task.priority = 0;
        task.enterTime = DateTime.Now;

        if (task != null)
        {
            List<Task> tasks = new List<Task>();
            if (task.channel == -1)
            {
                foreach (var channelItem in channels.Values)
                {
                    tasks.Add(AsyncExcuteTaskByChannel(channelItem, task));
                }
            }
            else if (channels.TryGetValue(task.channel, out var resultChannel))
            {
                tasks.Add(AsyncExcuteTaskByChannel(resultChannel, task));
            }

            await Task.WhenAll(tasks);
            taskPool.Release(task);
        }

    }

    public static void RegistMessageCommand(ushort message, object recver, Action<object, object[]> callback, int channel = -1)
    {
        var msgItem = new MessageItem
        {
            message = message,
            listener = recver,
            callback = callback,
            channel = channel
        };

        if (!channels.TryGetValue(channel, out var resultChannel))
        {
            resultChannel = new MessageChannel();
            resultChannel.channel = channel;
            channels.Add(channel, resultChannel);
        }
        if (resultChannel.msgDict.TryGetValue(message, out var listeners))
        {
            listeners.Add(msgItem);
        }
        else
        {
            resultChannel.msgDict.Add(message, new List<MessageItem> { msgItem });
        }

        if (resultChannel.listenerDict.TryGetValue(recver, out var messageItems))
        {
            messageItems.Add(msgItem);
        }
        else
        {
            resultChannel.listenerDict.Add(recver, new List<MessageItem> { msgItem });
        }

    }
    public static void RegistMessageCommand(ushort message, object recver, Func<object, object[], Task> taskCallbackFunc, int channel = -1)
    {
        var msgItem = new MessageItem
        {
            message = message,
            listener = recver,
            taskCallbackFunc = taskCallbackFunc,
            channel = channel
        };

        if (!channels.TryGetValue(channel, out var resultChannel))
        {
            resultChannel = new MessageChannel();
            resultChannel.channel = channel;
            channels.Add(channel, resultChannel);
        }

        if (resultChannel.msgDict.TryGetValue(message, out var listeners))
        {
            listeners.Add(msgItem);
        }
        else
        {
            resultChannel.msgDict.Add(message, new List<MessageItem> { msgItem });
        }
        if (resultChannel.listenerDict.TryGetValue(recver, out var messageItems))
        {
            messageItems.Add(msgItem);
        }
        else
        {
            resultChannel.listenerDict.Add(recver, new List<MessageItem> { msgItem });
        }
    }
    public static void ClearMessageCommand(object recver)
    {
        foreach (var resultChannel in channels.Values)
        {
            if (resultChannel.listenerDict.TryGetValue(recver, out var listeners))
            {
                foreach (var item in listeners)
                {
                    if (resultChannel.msgDict.TryGetValue(item.message, out var messageItems))
                    {
                        messageItems.Remove(item);
                        if (messageItems.Count == 0)
                        {
                            resultChannel.msgDict.Remove(item.message);
                        }
                    }
                }
                resultChannel.listenerDict.Remove(recver);
            }
        }
    }
    public static void ClearMessageCommand(object recver, ushort message)
    {
        foreach (var resultChannel in channels.Values)
        {
            if (resultChannel.listenerDict.TryGetValue(recver, out var listeners))
            {
                var itemsToRemove = listeners.Where(item => item.message == message).ToList();
                foreach (var item in itemsToRemove)
                {
                    if (resultChannel.msgDict.TryGetValue(item.message, out var messageItems))
                    {
                        messageItems.Remove(item);
                        if (messageItems.Count == 0)
                        {
                            resultChannel.msgDict.Remove(item.message);
                        }
                    }
                }
                listeners.RemoveAll(item => item.message == message);
            }

        }

    }
    public static void Clear()
    {
        foreach (var resultChannel in channels.Values)
        {
            resultChannel.listenerDict.Clear();
            resultChannel.msgDict.Clear();
        }
    }
    public static void BindMessage(object obj)
    {
        if (obj is not IMsgObj) return;

        var methods = obj.GetType().GetMethods(BindingFlags.Instance |
            BindingFlags.NonPublic |
            BindingFlags.Public |
            BindingFlags.Static).Where(item => Attribute.IsDefined(item, typeof(MsgCallbackAttribute)));

        foreach (var method in methods)
        {
            var attributes = method.GetCustomAttributes<Attribute>();

            foreach (var attribute in attributes)
            {
                if (attribute is MsgCallbackAttribute messageAttribute)
                {
                    if (method.ReturnType == typeof(void))
                    {
                        Action<object, object[]> callback = null;
                        var pams = method.GetParameters();
                        if (pams.Length == 0)
                        {
                            callback = (sender, param) =>
                            {
                                try
                                {
                                    method.Invoke(obj, new object[] { });
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError($"Error invoking method {method.Name} in {obj.GetType().Name}: {ex.Message}");
                                }
                            };
                        }
                        else if (pams.Length == 1 && pams[0].ParameterType == typeof(object))
                        {
                            callback = (sender, param) =>
                            {
                                try
                                {
                                    method.Invoke(obj, new object[] { sender });
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError($"Error invoking method {method.Name} in {obj.GetType().Name}: {ex.Message}");
                                }
                            };
                        }
                        else if (pams.Length == 2 && pams[0].ParameterType == typeof(object) && pams[1].ParameterType == typeof(object[]))
                        {
                            callback = (sender, param) =>
                            {
                                try
                                {
                                    method.Invoke(obj, new object[] { sender, param });
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError($"Error invoking method {method.Name} in {obj.GetType().Name}: {ex.Message}");
                                }
                            };
                        }
                        else
                        {
                            callback = (sender, param) =>
                            {
                                try
                                {
                                    method.Invoke(obj, param);
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError($"Error invoking method {method.Name} in {obj.GetType().Name}: {ex.Message}");
                                }
                            };
                        }

                        RegistMessageCommand(messageAttribute.MessageId, obj, callback, messageAttribute.Channel);
                    }
                    else if (method.ReturnType == typeof(Task))
                    {
                        Func<object, object[], Task> callback = null;
                        var pams = method.GetParameters();
                        if (pams.Length == 0)
                        {
                            callback = async (sender, param) =>
                            {
                                try
                                {
                                    var task = method.Invoke(obj, new object[] { }) as Task;
                                    await task;
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError($"Error invoking method {method.Name} in {obj.GetType().Name}: {ex.Message}");
                                }
                            };
                        }
                        else if (pams.Length == 1 && pams[0].ParameterType == typeof(object))
                        {
                            callback = async (sender, param) =>
                            {
                                try
                                {
                                    var task = method.Invoke(obj, new object[] { sender }) as Task;
                                    await task;
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError($"Error invoking method {method.Name} in {obj.GetType().Name}: {ex.Message}");
                                }
                            };
                        }
                        else if (pams.Length == 2 && pams[0].ParameterType == typeof(object) && pams[1].ParameterType == typeof(object[]))
                        {
                            callback = async (sender, param) =>
                            {
                                try
                                {
                                    var task = method.Invoke(obj, new object[] { sender, param }) as Task;
                                    await task;
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError($"Error invoking method {method.Name} in {obj.GetType().Name}: {ex.Message}");
                                }
                            };
                        }
                        else
                        {
                            callback = async (sender, param) =>
                            {
                                try
                                {
                                    var task = method.Invoke(obj, param) as Task;
                                    await task;
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError($"Error invoking method {method.Name} ( ParamLenght:{param.Length} / {method.GetParameters().Length} ) in {obj.GetType().Name}: {ex.Message}");

                                    Debug.LogException(ex);
                                }
                            };
                        }

                        RegistMessageCommand(messageAttribute.MessageId, obj, callback, messageAttribute.Channel);
                    }
                    else
                    {
                        Debug.LogError($"Method {method.Name} in {obj.GetType().Name} must return void or Task.");
                        continue;
                    }
                }
            }

        }


    }
    public static void UnBindMessage(object obj)
    {
        ClearMessageCommand(obj);
    }
    #endregion

    #region Rate

    public static void UpdateCall()
    {
        UpdateTimer();
        lock (priorityQueue)
        {
            while (priorityQueue.TryDequeue(out var task))
            {
                excuteTaskQueue.Enqueue(task);
            }
        }

        while (excuteTaskQueue.TryDequeue(out var task))
        {
            ExcuteTask(task);
        }

    }

    private static bool initFlag = false;
    //private static DateTime lastTime;
    private static void UpdateTimer()
    {
        if (!initFlag)
        {
            initFlag = true;
            //lastTime = DateTime.Now;
            return;
        }

        while (messageTaskTimerList.Count > 0)
        {
            MessageTask excuteTask = null;
            lock (messageTaskTimerList)
            {
                var task = messageTaskTimerList.Peek();
                DateTime now = DateTime.Now;
                bool flag = now - task.enterTime >= task.delay;
                if (flag)
                {
                    excuteTask = messageTaskTimerList.Dequeue();
                }
            }

            if (excuteTask != null)
            {
                EnqueueMessageTask(excuteTask);
            }
            else
            {
                break;
            }

        }

    }

    private static void ExcuteTask(MessageTask task)
    {
        if (task != null)
        {
            if (task.channel == -1)
            {
                foreach (var channel in channels.Values)
                {
                    ExcuteByChannel(channel, task);
                }
            }
            else if (channels.TryGetValue(task.channel, out var resultChannel))
            {
                ExcuteByChannel(resultChannel, task);
            }

            taskPool.Release(task);
        }
    }

    private static void ExcuteByChannel(MessageChannel channel, MessageTask task)
    {
        if (channel.msgDict.TryGetValue(task.message, out var list))
        {
            for (int i = 0; i < list.Count; ++i)
            {
                var item = list[i];
                if (item != null && (task.recver == null || task.recver == item.listener))
                {
                    if (item.callback != null)
                    {
                        item.callback.Invoke(task.sender, task.param);
                    }
                    else if (item.taskCallbackFunc != null)
                    {
                        _ = item.taskCallbackFunc.Invoke(task.sender, task.param);
                    }
                }
            }
        }
    }

    private static async Task AsyncExcuteTaskByChannel(MessageChannel channel, MessageTask task)
    {
        if (channel.msgDict.TryGetValue(task.message, out var list))
        {
            List<Task> tasks = null;
            for (int i = 0; i < list.Count; ++i)
            {
                var item = list[i];
                if (item != null && (task.recver == null || task.recver == item.listener))
                {
                    if (item.callback != null)
                    {
                        item.callback(task.sender, task.param);
                    }
                    else if (item.taskCallbackFunc != null)
                    {
                        if (tasks == null)
                            tasks = new List<Task>();
                        tasks.Add(item.taskCallbackFunc.Invoke(task.sender, task.param));
                    }
                }
            }
            if (tasks != null)
                await Task.WhenAll(tasks);
        }
    }


    #endregion

}


public static class MessageDispatchUtils
{
    public static async Task AsyncCallCommandTo(this object sender, ushort msg, int channel = -1, params object[] param)
    {
        await MessageDispatch.AsyncCallMessageCommand(msg, sender, null, channel, param);
    }
    public static async Task AsyncCallCommandTo(this object sender, ushort msg, int channel = -1, object recver = null, params object[] param)
    {
        await MessageDispatch.AsyncCallMessageCommand(msg, sender, recver, channel, param);
    }
    public static async Task AsyncCallCommand(this object sender, ushort msg, int channel = -1, params object[] param)
    {
        await MessageDispatch.AsyncCallMessageCommand(msg, sender, null, channel, param);
    }
    public static void SendCommandTo(this object sender, ushort msg, object recver, TimeSpan delay = default, int priority = 0, int channel = -1, params object[] param)
    {
        MessageDispatch.CallMessageCommand(msg, sender, recver, delay, priority, channel, param: param);
    }
    public static void SendCommand(this object sender, ushort msg, TimeSpan delay = default, int priority = 0, int channel = -1, params object[] param)
    {
        MessageDispatch.CallMessageCommand(msg, sender, null, delay, priority, channel, param: param);
    }

    public static void RegistCommand(this object recver, ushort msg, Action<object, object[]> callback)
    {
        MessageDispatch.RegistMessageCommand(msg, recver, callback);
    }

    public static void UnRegistCommand(this object recver, ushort msg)
    {
        MessageDispatch.ClearMessageCommand(recver, msg);
    }

    public static void ClearRegistedCommand(this object recver)
    {
        MessageDispatch.ClearMessageCommand(recver);
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class MsgCallbackAttribute : Attribute
{
    public ushort MessageId { get; private set; }
    public int Channel { get; private set; }
    public MsgCallbackAttribute(ushort messageId, int channel = -1)
    {
        MessageId = messageId;
        Channel = channel;
    }

}
