using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TokenBucket
{
    public class LeakageBucketLimitingService : ILimitingService
    {
        private LimitedQueue<object> limitedQueue = null;
        private CancellationTokenSource cancelToken;
        private Task task = null;
        private int maxTPS;
        private int limitSize;
        private object lckObj = new object();
        public LeakageBucketLimitingService(int maxTPS, int limitSize)
        {
            this.limitSize = limitSize;
            this.maxTPS = maxTPS;

            if (this.limitSize <= 0)
                this.limitSize = 100;
            if (this.maxTPS <= 0)
                this.maxTPS = 1;

            limitedQueue = new LimitedQueue<object>(limitSize);
            cancelToken = new CancellationTokenSource();
            task = Task.Factory.StartNew(new Action(TokenProcess), cancelToken.Token);
        }

        private void TokenProcess()
        {
            int sleep = 1000 / maxTPS;
            if (sleep == 0)
                sleep = 1;

            DateTime start = DateTime.Now;
            while (cancelToken.Token.IsCancellationRequested == false)
            {
                try
                {

                    if (limitedQueue.Count > 0)
                    {
                        lock (lckObj)
                        {
                            if (limitedQueue.Count > 0)
                                limitedQueue.Dequeue();
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    if (DateTime.Now - start < TimeSpan.FromMilliseconds(sleep))
                    {
                        int newSleep = sleep - (int)(DateTime.Now - start).TotalMilliseconds;
                        if (newSleep > 1)
                            Thread.Sleep(newSleep - 1); //做一下时间上的补偿
                    }
                    start = DateTime.Now;
                }
            }
        }

        public void Dispose()
        {
            cancelToken.Cancel();
        }

        public bool Request()
        {
            if (limitedQueue.Count >= limitSize)
                return false;
            lock (lckObj)
            {
                if (limitedQueue.Count >= limitSize)
                    return false;

                return limitedQueue.Enqueue(new object());
            }
        }

        public bool Request(string key)
        {
            return false;
        }
    }
}
