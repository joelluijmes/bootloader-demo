using System;
using Programmer.Programmer;
using Programmer.Util;

namespace Programmer
{
    internal sealed class Program
    {
        static void Main(string[] args)
        {
            var com = "";
            var baud = 57600;
            var path = "";

            if (args.Length == 3)
            {
                path = args[1];
                com = args[2];
            }
            else if (args.Length == 4)
            {
                path = args[1];
                com = args[2];
                baud = int.Parse(args[3]);
            }
            else
            {
                Console.WriteLine("Usage:\r\n  fancy-programmer.exe [path] [com] (baud)");
                return;
            }

            var programmer = new SerialProgrammer(com, baud);
            programmer.ProgramHexFile(path).Wait();
        }
    }
}
