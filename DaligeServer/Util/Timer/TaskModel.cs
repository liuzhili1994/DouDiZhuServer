using System;
public class TaskTimeModel
{
    public int id;
    public double desTime;
    public float delay;
    public Action<int> taskCall;
    public int executeCount;
    public bool isExecuteInMainThread;//是否在主线程中执行
    public Action<Action<int>, int> taskCallInMain;
    public TaskTimeModel()
    {
    }
    public TaskTimeModel(int id, double desTime, float delay, int executeCount, Action<int> taskCall, bool isExecuteInMain = false, Action<Action<int>, int> exeInMain = null)
    {
        this.id = id;
        this.desTime = desTime;
        this.delay = delay;
        this.executeCount = executeCount;
        this.taskCall = taskCall;
        this.isExecuteInMainThread = isExecuteInMain;
        this.taskCallInMain = exeInMain;
    }

    public void Set(double desTime, float delay, int executeCount, Action<int> taskCall, bool isExecuteInMain = false, Action<Action<int>, int> exeInMain = null)
    {
        this.desTime = desTime;
        this.delay = delay;
        this.executeCount = executeCount;
        this.taskCall = taskCall;
        this.isExecuteInMainThread = isExecuteInMain;
        this.taskCallInMain = exeInMain;
    }

    public void Execute(int id)
    {
        if (isExecuteInMainThread)
        {
            if (taskCallInMain != null)
                taskCallInMain.Invoke(taskCall,this.id);
            return;
        }

        if (taskCall != null)
        {
            taskCall.Invoke(id);
        }
    }
}

public class TaskFrameModel
{
    public int id;
    public int desFrame;
    public int delay;
    public Action<int> taskCall;
    public int executeCount;
    public bool isExecuteInMainThread;//是否在主线程中执行
    public TaskFrameModel()
    {
    }
    public TaskFrameModel(int id, int desFrame, int delay, int executeCount, Action<int> taskCall, bool isExecuteInMain = false)
    {
        this.id = id;
        this.desFrame = desFrame;
        this.delay = delay;
        this.executeCount = executeCount;
        this.taskCall = taskCall;
        this.isExecuteInMainThread = isExecuteInMain;
    }

    public void Set(int desFrame, int delay, int executeCount, Action<int> taskCall, bool isExecuteInMain = false)
    {
        this.desFrame = desFrame;
        this.delay = delay;
        this.executeCount = executeCount;
        this.taskCall = taskCall;
        this.isExecuteInMainThread = isExecuteInMain;
    }

    public Action<int> Execute(int id)
    {
        if (isExecuteInMainThread)
        {
            return taskCall;
        }
        if (taskCall != null)
        {
            taskCall.Invoke(id);
        }
        return null;
    }
}



public enum TimerUnit
{
    MilliSecond,
    Second,
    Minute,
    Hour,
    Day,
}


public class ExecuteInMain
{
    public int intPara;
    public Action<int> exeDel;
    public ExecuteInMain(Action<int> exeDel, int intPara)
    {
        this.intPara = intPara;
        this.exeDel = exeDel;
    }

    public void Execute()
    {
        if (exeDel != null)
            exeDel.Invoke(intPara);
    }
}
