using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATLocalizationsTool.Commons
{
    public class DATBinaryWriter
    {
        public List<byte> DATBinaryWriterData = new List<byte>();

        public int Position = 0;
        public int Length = 0;
        public void WriteUByte(byte value)
        {
            DATBinaryWriterData.Add(value);
            Position++;
            Length++;
        }

        public void WriteByte(sbyte value)
        {
            DATBinaryWriterData.Add((byte)value);
            Position++;
            Length++;
        }

        public void WriteInt()
        {
            Position += 4;
            Length += 4;
        }

        public void WriteUInt()
        {
            Position += 4;
            Length += 4;
        }
        public void WriteString(string value)
        {
            //int length = Encoding.Unicode.GetByteCount(value);

            //Console.WriteLine(length);
            /*for (int i = 0; i < value.Length; i++)
            {
                DATBinaryWriterData.Add((byte)value[i]);
            }
            Position += value.Length;
            Length += value.Length;*/
        }
    }
}
