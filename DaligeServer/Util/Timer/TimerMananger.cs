using System;
using System.Collections.Generic;
using System.Threading;
using DaligeServer.Util.Concurrent;

public class TimerMananger
{
    private static TimerMananger instance;
    public static TimerMananger Instance { get {
            if (instance == null)
                instance = new TimerMananger(50,Console.WriteLine);
            return instance;
        }
        private set { }

    }
    
    private System.Timers.Timer serverTimer;
    private Action<string> LogDel;
    
    private ConcurrentInt id;
    private int frameCounter;
    private DateTime rawTime = new DateTime(1970,1,1,0,0,0);

    private static readonly string lockId = "lockId";
    private List<int> taskIdList = new List<int>();
    private List<int> taskIdToReduceList = new List<int>();

    private static readonly string lockTime = "lockTime";
    private List<TaskTimeModel> tempTimeTaskList = new List<TaskTimeModel>();
    private List<TaskTimeModel> timeTaskList = new List<TaskTimeModel>();

    private List<int> tempTimeTaskForDelList = new List<int>();

    private static readonly string lockFrame = "lockFrame";
    private List<TaskFrameModel> tempFrameTaskList = new List<TaskFrameModel>();
    private List<TaskFrameModel> frameTaskList = new List<TaskFrameModel>();

    private List<int> tempFrameTaskForDelList = new List<int>();

    public TimerMananger(int timerInterval = 0,Action<string> logDel = null)
    {
        this.LogDel = logDel;
        id = new ConcurrentInt(-1);
        frameCounter = 0;
        

        //服务器使用需要开启多线程计时
        if(timerInterval != 0)
        {
            serverTimer = new System.Timers.Timer(timerInterval);
            serverTimer.AutoReset = true;

            serverTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => { this.Update(); };
            serverTimer.Start();
        }
    }


    

    public void Update()
    {
        frameCounter++;

        CheckTimeTask();
        CheckFrameTask();

        //回收需要删除的时间任务
        if (tempTimeTaskForDelList.Count > 0)
        {
            
            lock (lockTime)
            {
                foreach (var id in tempTimeTaskForDelList)
                {
                    bool isDel = false;
                    for (int i = 0; i < timeTaskList.Count; i++)
                    {
                        var task = timeTaskList[i];
                        if (task.id == id)
                        {
                            //找到了
                            timeTaskList.Remove(task);
                            taskIdToReduceList.Add(task.id);
                            isDel = true;
                            Log("找到了这个任务，并且删除了 id ：" + task.id);
                            break;
                        }
                    }
                    if (isDel)
                    {
                        continue;
                    }
                    //没找到 到tempList中找
                    for (int i = 0; i < tempTimeTaskList.Count; i++)
                    {
                        var task = tempTimeTaskList[i];
                        if (task.id == id)
                        {
                            //找到了
                            tempTimeTaskList.Remove(task);
                            taskIdToReduceList.Add(task.id);
                            isDel = true;
                            break;
                        }
                    }

                    if(!isDel)
                        Log("没找到这个id啊 ：" + id);
                }

                tempTimeTaskForDelList.Clear();
            }
        }

        //回收需要删除的帧任务
        if (tempFrameTaskForDelList.Count > 0)
        {
            lock (lockFrame)
            {
                foreach (var id in tempFrameTaskForDelList)
                {
                    bool isDel = false;
                    for (int i = 0; i < frameTaskList.Count; i++)
                    {
                        var task = frameTaskList[i];
                        if (task.id == id)
                        {
                            //找到了
                            frameTaskList.Remove(task);
                            taskIdToReduceList.Add(task.id);
                            isDel = true;
                            break;
                        }
                    }
                    if (isDel)
                    {
                        continue;
                    }
                    //没找到 到tempList中找
                    for (int i = 0; i < tempFrameTaskList.Count; i++)
                    {
                        var task = tempFrameTaskList[i];
                        if (task.id == id)
                        {
                            //找到了
                            tempFrameTaskList.Remove(task);
                            taskIdToReduceList.Add(task.id);
                            isDel = true;
                            break;
                        }
                    }

                    if (!isDel)
                        Log("没找到这个id啊 ：" + id);
                }

                tempFrameTaskForDelList.Clear();
            }
        }

        //回收已完成的任务id
        if (taskIdToReduceList.Count > 0)
        {
            lock (lockId)
            {
                for (int i = 0; i < taskIdToReduceList.Count; i++)
                {
                    if (taskIdList.Contains(taskIdToReduceList[i]))
                    {
                        taskIdList.RemoveAt(i);
                    }
                }
                taskIdToReduceList.Clear();
            }
        }
    }//Update_End

    /// <summary>
    /// 检测时间定时任务完成情况
    /// </summary>
    private void CheckTimeTask()
    {
        //加入缓存区的定时任务  让新加入的任务 下一帧才开始执行，防止出错
        if (tempTimeTaskList.Count > 0)
        {
            lock (lockTime)
            {
                timeTaskList.AddRange(tempTimeTaskList);
                tempTimeTaskList.Clear();
            }
        }

        //遍历所有任务是否到达条件
        for (int i = 0; i < timeTaskList.Count; i++)
        {
            var task = timeTaskList[i];
            if (GetUTCSecond() < task.desTime)
                continue;
            else
            {
                try
                {
                   task.Execute(task.id);
                }
                catch (Exception ex)
                {
                    Log(ex.ToString());
                }


                if (task.executeCount == 1)
                {

                    //timeTaskList.Remove(task);
                    //taskIdToReduceList.Add(task.id);
                    //i--;
                    lock (lockTime)
                    {
                        tempTimeTaskForDelList.Add(task.id);
                    }
                }
                else
                {
                    if (task.executeCount != 0)
                        task.executeCount--;
                    task.desTime += task.delay;
                }

            }
        }
    }//CheckTimeTask_End




    /// <summary>
    /// 检测帧定时任务完成情况
    /// </summary>
    private void CheckFrameTask()
    {
        //加入缓存区的定时任务  让新加入的任务 下一帧才开始执行，防止出错
        if (tempFrameTaskList.Count > 0)
        {
            lock (lockFrame)
            {
                frameTaskList.AddRange(tempFrameTaskList);
                tempFrameTaskList.Clear();
            }
            
        }

        //遍历所有任务是否到达条件
        for (int i = 0; i < frameTaskList.Count; i++)
        {
            var task = frameTaskList[i];
            if (frameCounter < task.desFrame)
                continue;
            else
            {
                try
                {
                    task.Execute(task.id);
                }
                catch (Exception ex)
                {
                    Log(ex.ToString());
                }


                if (task.executeCount == 1)
                {
                    //frameTaskList.Remove(task);
                    //taskIdToReduceList.Add(task.id);
                    //i--;

                    lock (lockFrame)
                    {
                        tempFrameTaskForDelList.Add(task.id);
                    }
                }
                else
                {
                    if (task.executeCount != 0)
                        task.executeCount--;
                    task.desFrame += task.delay;
                }

            }
        }
    }//CheckFrameTask_End




    #region FrameTask

    /// <summary>
    /// 添加帧定时任务
    /// </summary>
    /// <param name="taskCall"></param>
    /// <param name="delay"></param>
    /// <param name="executeCount"></param>
    /// <param name="timerUnit"></param>
    /// <returns></returns>
    public int AddFrameTask(Action<int> taskCall, int delay, int executeCount = 1, bool isExeInMain = false)
    {
        //生成taskModel
        int taskId;
        //防止id重复
        while (true)
        {
            taskId = id.Add_Get();
            if (!taskIdList.Contains(taskId))
                break;
        }
        lock (lockFrame)
        {
            tempFrameTaskList.Add(new TaskFrameModel(taskId, delay + frameCounter, delay, executeCount, taskCall,isExeInMain));
        }
        
        lock (lockId)
        {
            taskIdList.Add(taskId);
        }
        return taskId;
    }

    /// <summary>
    /// 删除帧定时任务
    /// </summary>
    /// <param name="id"></param>
    public void DeleteFrameTaskByID(int id)
    {
        lock (lockFrame)
        {
            tempFrameTaskForDelList.Add(id);
        }
        //for (int i = 0; i < frameTaskList.Count; i++)
        //{
        //    var task = frameTaskList[i];
        //    if (task.id == id)
        //    {
        //        frameTaskList.Remove(task);//del一般调用比较少，就没用缓存
        //        if (taskIdList.Contains(id))
        //            taskIdList.Remove(id);//del一般调用比较少，就没用缓存

        //        return true;
        //    }
        //}


        //for (int j = 0; j < tempFrameTaskList.Count; j++)
        //{
        //    var task = tempFrameTaskList[j];
        //    if (task.id == id)
        //    {
        //        tempFrameTaskList.Remove(task);
        //        if (taskIdList.Contains(id))
        //            taskIdList.Remove(id);

        //        return true;
        //    }
        //}

        //return false;
    }

    /// <summary>
    /// 替换帧定时任务
    /// </summary>
    /// <param name="id"></param>
    /// <param name="taskCall"></param>
    /// <param name="delay"></param>
    /// <param name="executeCount"></param>
    /// <param name="timerUnit"></param>
    /// <returns></returns>
    public bool ReplaceFrameTask(int id, Action<int> taskCall, int delay, int executeCount = 1, bool isExeInMain = false)
    {

        //生成taskModel
        foreach (var task in frameTaskList)
        {
            if (task.id == id)
            {
                //找到了
                task.Set(delay + frameCounter, delay, executeCount, taskCall,isExeInMain);
                return true;
            }
        }

        foreach (var task in tempFrameTaskList)
        {
            if (task.id == id)
            {
                //找到了
                task.Set(delay + frameCounter, delay, executeCount, taskCall,isExeInMain);
                return true;
            }
        }

        return false;
    }

    #endregion




    #region TimeTask

    private void CalculateDelay(ref float delay, TimerUnit timerUnit)
    {
        //转换时间单位 
        switch (timerUnit)
        {
            case TimerUnit.MilliSecond:
                delay *= 0.001f;
                break;
            case TimerUnit.Minute:
                delay *= 60;
                break;
            case TimerUnit.Hour:
                delay *= 60 * 60;
                break;
            case TimerUnit.Day:
                delay *= 60 * 60 * 24;
                break;
        }
    }


    /// <summary>
    /// 删除时间定时任务
    /// </summary>
    /// <param name="id"></param>
    public void DeleteTimeTaskByID(int id)
    {
        lock (lockTime)
        {
            tempTimeTaskForDelList.Add(id);
        }
        //for (int i = 0; i < timeTaskList.Count; i++)
        //{
        //    var task = timeTaskList[i];
        //    if (task.id == id)
        //    {
        //        timeTaskList.Remove(task);
        //        if (taskIdList.Contains(id))
        //            taskIdList.Remove(id);

        //        return true;
        //    }
        //}


        //for (int j = 0; j < tempTimeTaskList.Count; j++)
        //{
        //    var task = timeTaskList[j];
        //    if (task.id == id)
        //    {
        //        tempTimeTaskList.Remove(task);
        //        if (taskIdList.Contains(id))
        //            taskIdList.Remove(id);

        //        return true;
        //    }
        //}

        //return false;
    }


    /// <summary>
    /// 添加时间定时任务
    /// </summary>
    /// <param name="taskCall">回调方法</param>
    /// <param name="delay">延迟多久执行</param>
    /// <param name="executeCount">执行次数</param>
    /// <param name="isExeInMain">是否在主线程中执行</param>
    /// <param name="timerUnit">设置delay参数的时间单位</param>
    /// <returns>生成的任务id</returns>
    public int AddTimeTask(Action<int> taskCall, float delay, int executeCount = 1, bool isExeInMain = false, Action<Action<int>,int> exeInMain = null, TimerUnit timerUnit = TimerUnit.Second)
    {

        CalculateDelay(ref delay, timerUnit);

        //生成taskModel
        double desTime = GetUTCSecond() + delay;
        int taskId;
        //防止id重复
        while (true)
        {
            taskId = id.Add_Get();
            if (!taskIdList.Contains(taskId))
                break;
        }
        lock (lockTime)
        {
            tempTimeTaskList.Add(new TaskTimeModel(taskId, desTime, delay, executeCount, taskCall, isExeInMain,exeInMain));
        }
        lock (lockId)
        {
            taskIdList.Add(taskId);
        }

        return taskId;
    }



    /// <summary>
    /// 替换时间定时任务
    /// </summary>
    /// <param name="id"></param>
    /// <param name="taskCall"></param>
    /// <param name="delay"></param>
    /// <param name="executeCount"></param>
    /// <param name="timerUnit"></param>
    /// <returns></returns>
    public bool ReplaceTimeTask(int id, Action<int> taskCall, float delay, int executeCount = 1, bool isExeInMain = false, Action<Action<int>, int> exeInMain = null, TimerUnit timerUnit = TimerUnit.Second)
    {

        CalculateDelay(ref delay, timerUnit);

        //生成taskModel
        double desTime = GetUTCSecond() + delay;
       
        foreach (var task in timeTaskList)
        {
            if (task.id == id)
            {
                //找到了
                task.Set(desTime, delay, executeCount, taskCall,isExeInMain,exeInMain);
                return true;
            }
        }

        foreach (var task in tempTimeTaskList)
        {
            if (task.id == id)
            {
                //找到了
                task.Set(desTime, delay, executeCount, taskCall,isExeInMain,exeInMain);
                return true;
            }
        }

        return false;
    }


    #endregion

    

    #region 工具方法
    private void Log(string content)
    {
        if (this.LogDel != null)
            this.LogDel.Invoke(content);
    }

    private double GetUTCSecond()
    {
        TimeSpan ts = DateTime.UtcNow - rawTime;
        return ts.TotalSeconds;
    }

    /// <summary>
    /// 更改打印日志方法 
    /// </summary>
    /// <param name="logDel"></param>
    public void SetLogExecute(Action<string> logDel)
    {
        this.LogDel = logDel;
    }

    

    /// <summary>
    /// 服务重启
    /// </summary>
    public void ReStart()
    {
        taskIdList.Clear();
        taskIdToReduceList.Clear();

        timeTaskList.Clear();
        tempTimeTaskList.Clear();

        frameTaskList.Clear();
        tempFrameTaskList.Clear();

        LogDel = null;
       
        frameCounter = 0;
        id = new ConcurrentInt(-1);
        serverTimer.Stop();
    }

    /// <summary>
    /// 获取当前时间
    /// </summary>
    /// <returns></returns>
    public DateTime GetLocalDateTime()
    {
        //return DateTime.Now; //这种方式在断点的时候一直在变化
        return TimeZone.CurrentTimeZone.ToLocalTime(rawTime.AddSeconds(GetUTCSecond()));
    }

    public string GetLoaclDateTimeStr()
    {
        var currentTime = GetLocalDateTime();
        return GetTimeStr(currentTime.Hour) + ":" + GetTimeStr(currentTime.Minute) + ":" + GetTimeStr(currentTime.Second);
    }
    private string GetTimeStr(int time)
    {
        if (time < 10)
            return "0" + time;
        else
            return time.ToString();
    }

    #endregion

}
