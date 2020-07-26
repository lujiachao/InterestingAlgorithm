using System;
using System.Threading;

namespace TokenBucket
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = LimitingFactory.Build(LimitingType.TokenBucket, 50, 50);

            while (true)
            {
                var result = service.Request();
                //如果返回true，说明可以进行业务处理，否则需要继续等待
                if (result)
                {
                    //业务处理......
                    Console.WriteLine("1");
                }
                else
                { 
                    Console.WriteLine("2");
                }
            }

            Console.WriteLine("Hello World!");
        }
    }
}
