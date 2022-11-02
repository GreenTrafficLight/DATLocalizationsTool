using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATLocalizationsTool.Formats
{
    public class DAT
    {
        public List<string> Strings;

        public DAT(string filepath)
        {
            Strings = new List<string>();
            byte[] data = File.ReadAllBytes(filepath);
            uint size = (uint)data.Length + Path.GetFileNameWithoutExtension(filepath)[0] - 65;
            Crypt(data, size);
            
        }

        public byte[] Crypt(byte[] data, uint size)
        {
            uint ebx = 0;
            uint edi = size;
            uint position = 0;
            ulong r15 = 0;

            byte[] array = new byte[data.Length];

            while (position < data.Length)
            {
                uint ecx = ebx * 8;
                position++;
                ecx ^= ebx;
                uint eax = ebx + ebx;
                ecx = ~ecx;
                edi = edi + edi * 4;
                ecx >>= 7;
                r15++;
                ecx &= 1;
                edi++;
                ebx = ecx;
                ebx |= eax;
                eax = (byte)ebx;
                eax += (byte)edi;
                array[r15 - 1] ^= (byte)eax;
            }

            return array;
        }
    
        public void ReadStrings(BinaryReader reader, uint file_size)
        {
            while (reader.BaseStream.Position < file_size)
                Strings.Add(reader.ReadString());
        }
    }
}
