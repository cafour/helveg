using System;
using System.Runtime.InteropServices;

namespace VkInterop
{
    public class Program
    {
        [DllImport("vki")]
        public static extern int calculateValue();

        public static void Main(string[] args)
        {
            var value = calculateValue();
            Console.WriteLine(value);
        }
    }
}
