using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TokenBucket.Benchmark
{
    public class TokenBucketBenchmark
    {
        ILimitingService service = LimitingFactory.Build(LimitingType.TokenBucketByRate, 50, 500, "500/s");
        [Benchmark]
        public void GetToken()
        {
            var result = service.Request();
        }
    }
}
