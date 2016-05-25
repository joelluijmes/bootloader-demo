using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Programmer.Util
{
    [Serializable]
    internal sealed class HexRecord
    {
        private readonly string _line;

        public byte Code { get; set; }          // uint8_t
        public byte Length { get; set; }        // uint8_t
        public short Address { get; set; }      // uint16_t
        public byte Type { get; set; }          // uint8_t
        public byte[] Data { get; set; }        // uint8_t*
        public byte Checksum { get; set; }      // uint8_t 

        public HexRecord()
        { }

        public HexRecord(string line)
        {
            _line = line;
            var stringReader = new StringReader(line);

            Code = (byte) stringReader.Read();
            Length = stringReader.ReadHexByte();
            Address = stringReader.ReadHexShort();
            Type = stringReader.ReadHexByte();
            Data = stringReader.ReadHexBytes(Length).ToArray();
            Checksum = stringReader.ReadHexByte();

            var computedChecksum = (byte) (Length + (byte) (Address >> 8) + (byte) Address);
            computedChecksum = Data.Aggregate(computedChecksum, (a, b) => (byte) (a + b));
            computedChecksum = (byte)(-computedChecksum);

            if (Type == 1 && Checksum != 0xFF)
                throw new ArgumentException("Line is invalid, type is EOF but checksum is not 0xFF");
            if (Type != 1 && Checksum != computedChecksum)
                throw new ArgumentException($"Line has invalid checksum. Got {Checksum:X2} expected {computedChecksum:X2}");
        }

        public void Serialize(Stream stream)
        {
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(Code);
                writer.Write(Length);
                writer.Write(Address);
                writer.Write(Type);
                writer.Write(Data);
                writer.Write(Checksum);
            }
        }

        public override string ToString()
            => _line;
    }
}