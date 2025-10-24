using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class ScheduleDispatch
{
    private class ScheduleTaskItem
    {
        public Func<object[], Task> callback;
        public object[] param;
    }

    private class Channel
    {
        private bool _isRunning = false;
        private object _lock = new object();
        private PriorityQueue<ScheduleTaskItem, int> _scheduleTaskQueue = new PriorityQueue<ScheduleTaskItem, int>();



        public void ScheduleTask(Func<object[], Task> callback, int priority = -1, params object[] param)
        {
            var item = new ScheduleTaskItem();
            item.callback = callback;
            item.param = param;
            _scheduleTaskQueue.Enqueue(item, priority);
            StartNext();
        }

        private void StartNext()
        {
            lock (_lock)
            {
                if (_isRunning) return;
                _isRunning = true;
            }
            _ = RunQueue();
        }

        private async Task RunQueue()
        {
            await Task.Yield();
            while (_scheduleTaskQueue.TryDequeue(out var taskItem))
            {
                try
                {
                    await taskItem.callback.Invoke(taskItem.param);
                }
                catch (Exception ex)
                {
                    this.SendCommand((ushort)FrameworksMsg.LogException, param: ex);
                    Debug.LogException(ex);
                }
            }

            lock (_lock)
            {
                _isRunning = false;
                if (!_scheduleTaskQueue.IsEmpty)
                {
                    _isRunning = true;
                    _ = RunQueue();
                }
            }


        }
    }
    private static Dictionary<int, Channel> Channels = new Dictionary<int, Channel>();

    public static void ScheduleTask(Func<object[], Task> taskFunc, int priority = -1, int channel = -1, params object[] param)
    {
        var channelItem = GetChanel(channel);

        channelItem.ScheduleTask(taskFunc, priority, param);

    }
    public static void ScheduleTask(Task<object[]> callback, int priority = -1, int channel = -1, params object[] param)
    {
        // 将 Task<object[]> callback 包装为 Func<object[], Task>
        Func<object[], Task> wrapper = async (args) =>
        {
            await callback;
        };
        ScheduleTask(wrapper, priority, channel, param);

    }


    private static Channel GetChanel(int channel)
    {
        if (!Channels.TryGetValue(channel, out var resultChannel))
        {
            resultChannel = new Channel();
            Channels.Add(channel, resultChannel);
        }
        return resultChannel;
    }
}

public static class ScheduleDispatchUtil
{
    public static void ScheduleTask(this object obj, Func<object[], Task> taskFunc, int priority = -1, int channel = -1, params object[] param)
    {
        ScheduleDispatch.ScheduleTask(taskFunc, priority, channel, param);
    }

    public static void ScheduleTask(Task<object[]> callback, int priority = -1, int channel = -1, params object[] param)
    {
        ScheduleDispatch.ScheduleTask(callback, priority, channel, param);
    }
}
