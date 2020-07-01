using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DaligeServer.Util.Timer
{
    /// <summary>
    /// 计时器管理类
    /// </summary>
    public class TimeManager
    {
        private static TimeManager instance = null;
        private static object lockObj = new object();
        public static TimeManager Instance {
            get {
                lock (lockObj) {
                    if (instance == null)
                        instance = new TimeManager();
                    return instance;
                }
            }
        }

        /// <summary>
        /// 使用该类 实现定时器的主要功能
        /// </summary>
        private System.Timers.Timer timer;

        /// <summary>
        /// 存储 任务id 和 任务模型
        /// </summary>
        private ConcurrentDictionary<int, TimeModel> idModelDic = new ConcurrentDictionary<int, TimeModel>();

        /// <summary>
        /// 要溢出的任务ID列表
        /// </summary>
        private List<int> removeList = new List<int>();

        /// <summary>
        /// 用来表示ID
        /// </summary>
        private Concurrent.ConcurrentInt id = new Concurrent.ConcurrentInt(-1);

        public TimeManager() {
            //设置时间间隔
            timer = new System.Timers.Timer(20);
            //到达时间出发
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        /// <summary>
        /// 达到时间间隔时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (removeList) {
                TimeModel temModel = null;
                foreach (var id in removeList)
                {
                    idModelDic.TryRemove(id, out temModel);
                }
                removeList.Clear();
            }
            //Console.WriteLine(DateTime.Now.Ticks);
            //Console.WriteLine("执行了一次");
            foreach (var model in idModelDic.Values)
            {
                //DateTime.Now.Ticks
                if (model.time <= DateTime.Now.Ticks) {
                    model.Run();
                    removeList.Add(model.id);
                }
                    
            }
        }

        /// <summary>
        /// 添加定时任务 指定触发的时间 一个确定的时间比如   12:10
        /// </summary>
        public void AddTimeEvent(DateTime dateTime,TimeDelegate timeDel) {
            //延迟时间
            long delayTime = dateTime.Ticks - DateTime.Now.Ticks;
            if (delayTime <= 0)
            {
                return;
            }
            AddTimeEvent(delayTime,timeDel);
        }

        /// <summary>
        /// 添加定时任务  指定延迟时间  比如延迟  30s  执行
        /// </summary>
        public void AddTimeEvent(long delayTime,TimeDelegate timDel) {
            TimeModel model = new TimeModel(id.Add_Get(), DateTime.Now.Ticks + delayTime, timDel);
            idModelDic.TryAdd(model.id,model);
        }
    }
}
