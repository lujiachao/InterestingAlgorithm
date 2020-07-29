using System;
using System.Collections.Generic;
using System.Text;

namespace TokenBucket
{
    public class LimitingFactory
    {
        /// <summary>
        /// 创建限流服务对象
        /// </summary>
        /// <param name="limitingType">限流模型</param>
        /// <param name="maxQPS">最大QPS</param>
        /// <param name="limitSize">最大可用票据数</param>
        public static ILimitingService Build(LimitingType limitingType = LimitingType.TokenBucket, int maxQPS = 100, int limitSize = 100, string rate = "100/s")
        {
            switch (limitingType)
            {
                case LimitingType.TokenBucket:
                    return new TokenBucketLimitingService(maxQPS, limitSize);
                case LimitingType.TokenBucketByRate:
                    return new TokenBucketLimitingService(rate, limitSize);
                case LimitingType.LeakageBucket:
                    return new LeakageBucketLimitingService(maxQPS, limitSize);
                default:
                    throw new Exception("limit factory error");
            }
        }
    }
}
