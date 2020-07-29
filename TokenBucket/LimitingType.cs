using System;
using System.Collections.Generic;
using System.Text;

namespace TokenBucket
{
    /// <summary>
    /// 限流模式
    /// </summary>
    public enum LimitingType
    {
        TokenBucket,//令牌桶模式
        TokenBucketByRate,//新令牌桶模式
        TokenBucketByRateInDic, //多令牌桶并行模式
        LeakageBucket//漏桶模式
    }
}
