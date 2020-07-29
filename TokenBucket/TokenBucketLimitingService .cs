using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TokenBucket
{
    public class TokenBucketLimitingService : ILimitingService
    {
        private LimitedQueue<object> limitedQueue = null;
        Dictionary<string, LimitedQueue<object>> routeLimitedDic = new Dictionary<string, LimitedQueue<object>>();
        private CancellationTokenSource cancelToken;
        private Task task = null;
        private int maxTPS;
        private int limitSize;
        private string unit;
        private object lckObj = new object();
        public TokenBucketLimitingService(int maxTPS, int limitSize)
        {
            this.limitSize = limitSize;
            this.maxTPS = maxTPS;

            if (this.limitSize <= 0)
                this.limitSize = 100;
            if (this.maxTPS <= 0)
                this.maxTPS = 1;

            limitedQueue = new LimitedQueue<object>(limitSize);
            for (int i = 0; i < limitSize; i++)
            {
                limitedQueue.Enqueue(new object());
            }
            cancelToken = new CancellationTokenSource();
            task = Task.Factory.StartNew(new Action(TokenProcess), cancelToken.Token);
        }

        public TokenBucketLimitingService(string rate, int limitSize)
        {
            var rateSplits = rate.Split('/');
            this.limitSize = limitSize;
            this.maxTPS = Convert.ToInt32(rateSplits[0]);
            unit = rateSplits[1];
            if (this.limitSize <= 0)
                this.limitSize = 100;
            if (this.maxTPS <= 0)
                this.maxTPS = 1;

            limitedQueue = new LimitedQueue<object>(limitSize);

            for (int i = 0; i < limitSize; i++)
            {
                limitedQueue.Enqueue(new object());
            }

            cancelToken = new CancellationTokenSource();
            //Task.Factory.StartNew不是直接创建线程，创建的是任务，它有一个任务队列，然后通过任务调度器把任务分配到线程池中的空闲线程中，如果任务的数量比线程池中的线程多，线程池的线程数量还没有到达上限，就会创建新线程执行任务。如果线程池的线程已到达上限，没有分配到线程的任务需要等待有线程空闲的时候才执行。
            task = Task.Factory.StartNew(new Action(TokenProcessByRate), cancelToken.Token);
        }

        /// <summary>
        /// 使用字典来存储
        /// </summary>
        /// <param name="key"></param>
        /// <param name="rate"></param>
        /// <param name="limitSize"></param>
        public TokenBucketLimitingService(string key, string rate, int limitSize)
        {
            var rateSplits = rate.Split('/');
            this.limitSize = limitSize;
            this.maxTPS = Convert.ToInt32(rateSplits[0]);
            unit = rateSplits[1];
            if (this.limitSize <= 0)
                this.limitSize = 100;
            if (this.maxTPS <= 0)
                this.maxTPS = 1;

            limitedQueue = new LimitedQueue<object>(limitSize);

            for (int i = 0; i < limitSize; i++)
            {
                limitedQueue.Enqueue(new object());
            }

            routeLimitedDic.Add(key, limitedQueue);
            cancelToken = new CancellationTokenSource();
            //Task.Factory.StartNew不是直接创建线程，创建的是任务，它有一个任务队列，然后通过任务调度器把任务分配到线程池中的空闲线程中，如果任务的数量比线程池中的线程多，线程池的线程数量还没有到达上限，就会创建新线程执行任务。如果线程池的线程已到达上限，没有分配到线程的任务需要等待有线程空闲的时候才执行。
            task = Task.Factory.StartNew(new Action(() => TokenProcessByRateInDic(key)), cancelToken.Token);
        }

        /// <summary>
        /// 使用字典，定时消息令牌
        /// </summary>
        private void TokenProcessByRateInDic(string key)
        {
            int sleepTime = BuildSleepTime();
            while (cancelToken.Token.IsCancellationRequested == false)
            {
                Thread.Sleep(sleepTime);
                lock (lckObj)
                {
                    int countAdd = this.limitSize - limitedQueue.Count;
                    for (int i = 0; i < countAdd; i++)
                    {
                        limitedQueue.Enqueue(new object());
                    }
                    routeLimitedDic[key] = limitedQueue;
                }
            }
        }

        /// <summary>
        /// 定时消息令牌
        /// </summary>
        private void TokenProcessByRate()
        {
            int sleepTime = BuildSleepTime();
            while (cancelToken.Token.IsCancellationRequested == false)
            {
                Thread.Sleep(sleepTime);
                lock (lckObj)
                {
                    int countAdd = this.limitSize - limitedQueue.Count;
                    for (int i = 0; i < countAdd; i++)
                    {
                        limitedQueue.Enqueue(new object());
                    }
                }
            }
        }

        /// <summary>
        /// 定时消息令牌
        /// </summary>
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
                    lock (lckObj)
                    {
                        limitedQueue.Enqueue(new object());
                    }
                }
                catch(Exception ex)
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

        /// <summary>
        /// 请求令牌
        /// </summary>
        /// <returns>true：获取成功，false：获取失败</returns>
        public bool Request()
        {
            if (limitedQueue.Count <= 0)
                return false;
            lock (lckObj)
            {
                if (limitedQueue.Count <= 0)
                    return false;

                object data = limitedQueue.Dequeue();
                if (data == null)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 请求令牌
        /// </summary>
        /// <returns>true：获取成功，false：获取失败</returns>
        public bool Request(string key)
        {
            if (routeLimitedDic[key].Count <= 0)
                return false;
            lock (lckObj)
            { 
                if (routeLimitedDic[key].Count <= 0)
                    return false;

                object data = routeLimitedDic[key].Dequeue();
                if (data == null)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 计算休眠时间
        /// </summary>

        public int BuildSleepTime()
        {
            switch (unit)
            {
                case "d": return 1000 * 60 * 60 * 24;
                case "h": return 1000 * 60 * 60;
                case "m": return 1000 * 60;
                case "s": return 1000;
                default: throw new FormatException($" can't be converted to TimeSpan, unknown type ");
            }
        }
    }
}
