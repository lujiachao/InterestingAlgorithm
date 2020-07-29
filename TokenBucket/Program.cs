using System;
using System.Diagnostics;
using System.Threading;

namespace TokenBucket
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = LimitingFactory.Build(LimitingType.TokenBucketByRate, 50, 500, "500/s");
            int a = 0;
            int b = 0;
            for (var i = 0; i < 100; i++)
            {
                var result = service.Request();
                if (result)
                {
                    a++;
                }
                else
                {
                    b++;
                }
                Thread.Sleep(1);
            }
            Console.WriteLine(a);
            Console.WriteLine(b);
            Console.ReadKey();
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
