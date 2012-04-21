using System;
using IDI.Framework;

namespace IDI.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            var bootstrap = Bootstrap.Instance;

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
