using System;
using System.IO;
using System.Threading.Tasks;
using Programmer.Util;

namespace Programmer.Programmer
{
    internal enum Commands
    {
        Ready = 0x0A,
        Resent = 0x0B,
        Ack = 0x0C
    }

    internal abstract class AbstractProgrammer : IProgrammer
    {
        private const byte LAST_RECORD = 1;

        public async Task ProgramHexFile(Stream stream)
        {
            Send(new [] { (byte)Commands.Ready});
            
            HexRecord record = null;
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream && record?.Type != LAST_RECORD)
                {
                    var command = (Commands) ReceiveByte();
                    Console.WriteLine(command);
                    switch (command)
                    {
                        case Commands.Ready:
                        {
                            var line = await reader.ReadLineAsync();
                            if (line == null)
                                return;

                            record = new HexRecord(line);
                            Console.WriteLine($"Sending {line}");
                            SendRecord(record);
                            break;
                        }
                        case Commands.Resent:
                        {
                            if (record == null)
                                throw new InvalidOperationException("Received a resent command however not even one record has been sent yet.");

                            SendRecord(record);
                            break;
                        }
                    }
                }

                Console.WriteLine($"Done");
            }
        }

        private void SendRecord(HexRecord hexRecord)
        {
            using (var memoryStream = new MemoryStream())
            {
                hexRecord.Serialize(memoryStream);
                Send(memoryStream.ToArray());
            }
        }

        protected abstract void Send(byte[] bytes);
        protected abstract byte ReceiveByte();
    }
}