using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATLocalizationsTool.Commons
{
    public class DATBinaryReader
    {

        public List<byte> DATBinaryReaderData;

        public int Position = 0;
        public int Length = 0;

        public DATBinaryReader(byte[] data)
        {
            DATBinaryReaderData = data.ToList();
            Length = data.Length;
        }

        public void Seek(int position)
        {
            if (position >= 0)
                Position = position;
            else
                Position = Length - position;
        }

        public byte ReadUByte()
        {
            byte value = DATBinaryReaderData[Position];
            Position++;
            return value;
        }

        public sbyte ReadByte()
        {
            sbyte value = (sbyte)DATBinaryReaderData[Position];
            Position++;
            return value;
        }

        public string ReadString()
        {
            List<byte> StringData = new List<byte>();

            while (true)
            {
                StringData.Add(ReadUByte());
                if (StringData[StringData.Count - 1] == 0)
                    break;
            }

            return Encoding.UTF8.GetString(StringData.ToArray());
        }
    }
}
