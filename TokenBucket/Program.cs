using System;
using System.Diagnostics;
using System.Threading;

namespace TokenBucket
{
    class Program
    {
        static void Main(string[] args)
        {
            var service1 = LimitingFactory.Build( "Route1", LimitingType.TokenBucketByRateInDic, 50, 10, "10/s");
            var service2 = LimitingFactory.Build("Route2", LimitingType.TokenBucketByRateInDic, 50, 10, "10/s");
            int a1 = 0;
            int b1 = 0;
            int a2 = 0;
            int b2 = 0;
            for (var i = 0; i < 100; i++)
            {
                var result1 = service1.Request("Route1");
                var result2 = service2.Request("Route2");
                if (result1)
                {
                    a1++;
                }
                else
                {
                    b1++;
                }
                if (result2)
                {
                    a2++;
                }
                else
                {
                    b2++;
                }
            }
            Console.WriteLine(a1);
            Console.WriteLine(b1);
            Console.WriteLine(a2);
            Console.WriteLine(b2);
            Console.ReadKey();

            //var service = LimitingFactory.Build(LimitingType.TokenBucketByRate, 50, 10, "10/s");
            //var service = LimitingFactory.Build(LimitingType.TokenBucketByRate, 50, 10, "10/s");
            //int a = 0;
            //int b = 0;
            //for (var i = 0; i < 100; i++)
            //{
            //    var result = service.Request();
            //    if (result)
            //    {
            //        a++;
            //    }
            //    else
            //    {
            //        b++;
            //    }
            //    Thread.Sleep(90);
            //}
            //Console.WriteLine(a);
            //Console.WriteLine(b);
            //Console.ReadKey();
            //while (true)
            //{
            //    var result = service.Request();
            //    //如果返回true，说明可以进行业务处理，否则需要继续等待
            //    if (result)
            //    {
            //        //业务处理......
            //        Console.WriteLine("1");
            //    }
            //    else
            //    {
            //        Console.WriteLine("2");
            //    }
            //}

        }
    }
}
