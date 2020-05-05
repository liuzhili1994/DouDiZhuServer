namespace DaligeServer.Util.Timer
{
    /// <summary>
    /// 到达规定时间后出发
    /// </summary>
    public delegate void TimeDelegate();
    /// <summary>
    /// 定时器类
    /// </summary>
    public class TimeModel
    {
        /// <summary>
        /// 
        /// </summary>
        public int id;

        /// <summary>
        /// 任务执行的时间
        /// </summary>
        public long time;

        private TimeDelegate timeDel;

        public TimeModel(int id,long time,TimeDelegate timeDel) {
            this.id = id;
            this.time = time;
            this.timeDel = timeDel;
        }

        public void Run() {
            timeDel();
        }
    }
}
