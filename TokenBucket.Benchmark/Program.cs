using BenchmarkDotNet.Running;
using System;

namespace TokenBucket.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
#endif
#if RELEASE
            BenchmarkRunner.Run<TokenBucketBenchmark>();
#endif
        }
    }
}
