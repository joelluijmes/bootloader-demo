using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Programmer.Programmer;

namespace Programmer.Util
{
    internal static class Extensions
    {
        public static IEnumerable<byte> ReadHexBytes(this StringReader reader, int len)
        {
            var buf = new char[len*2];
            reader.Read(buf, 0, buf.Length);

            var str = new string(buf);
            return Enumerable.Range(0, len)
                .Select(x => Convert.ToByte(str.Substring(x*2, 2), 16));
        }

        public static byte ReadHexByte(this StringReader reader)
            => ReadHexBytes(reader, 1).First();

        public static short ReadHexShort(this StringReader reader)
            => (short) reader.ReadHexNumber(sizeof (short));

        public static int ReadHexNumber(this StringReader reader, int size)
        {
            return reader.ReadHexBytes(size)
                .Select(s => (int)s)
                .Aggregate((a, b) => (a << 8) | b);
        }

        public static Task ProgramHexFile(this IProgrammer programmer, string path)
        {
            using (var fileStream = File.OpenRead(path))
                return programmer.ProgramHexFile(fileStream);
        }
    }
}