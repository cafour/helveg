using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace VkInterop
{
    public class Program
    {
        [DllImport("vki")]
        public static extern int helloTriangle();

        public static void Main(string[] args)
        {
            Console.WriteLine(Process.GetCurrentProcess().Id);
            //Task.Delay(60000).GetAwaiter().GetResult();
            var value = helloTriangle();
            Console.WriteLine(value);
        }
    }
}
