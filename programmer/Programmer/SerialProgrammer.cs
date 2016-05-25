using System.Threading;
using OpenNETCF.IO.Serial;

namespace Programmer.Programmer
{
    internal sealed class SerialProgrammer : AbstractProgrammer
    {
        private readonly Port _serialPort;
        private readonly ManualResetEventSlim _resetEvent;

        public SerialProgrammer(string com, int baud = 9600)
        {
            _resetEvent = new ManualResetEventSlim(false);
            var portSettings = new HandshakeNone
            {
                BasicSettings = {BaudRate = (BaudRates)baud }
            };
            
            _serialPort = new Port(com, portSettings);
            _serialPort.DataReceived += () => _resetEvent.Set();
            _serialPort.Open();
        }
        
        protected override void Send(byte[] bytes)
        {
            _resetEvent.Reset();
            _serialPort.Output = bytes;
        }

        protected override byte ReceiveByte()
        {
            _resetEvent.Wait();

            return _serialPort.Input[0];
        }
    }
}