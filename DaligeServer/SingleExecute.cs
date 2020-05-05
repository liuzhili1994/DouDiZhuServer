using System.Threading;

namespace DaligeServer
{
    public delegate void ExecuteDel();

    /// <summary>
    /// 单线程池
    /// </summary>
    public class SingleExecute
    {
        private static object thisLock = new object();

        private static SingleExecute instance = null;
        /// <summary>
        /// 单例
        /// </summary>
        public static SingleExecute Instance {
            get {
                lock (thisLock)
                {
                    if (instance == null)
                        instance = new SingleExecute();
                    return instance;
                }
            }
        }

        

        /// <summary>
        /// 互斥锁
        /// </summary>
        public Mutex mutex;

        private SingleExecute() {
            mutex = new Mutex();
        }
        /// <summary>
        /// 单线程处理逻辑
        /// </summary>
        /// <param name="executeDel"></param>
        public void Execute(ExecuteDel executeDel) {
            lock (this)
            {
                mutex.WaitOne();
                executeDel();
                mutex.ReleaseMutex();
            }
        }
    }
}