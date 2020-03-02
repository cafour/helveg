using System;
using System.Runtime.InteropServices;

namespace VkInterop
{
    public class Program
    {
        [DllImport("vki")]
        public static extern int helloTriangle();

        public static void Main(string[] args)
        {
            var value = helloTriangle();
            Console.WriteLine(value);
        }
    }
}
