using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ICSharpCode.SharpZipLib.Zip.Compression;

using DATLocalizationsTool.Commons;

namespace DATLocalizationsTool.Formats
{
    public class DAT
    {
        public List<string> Strings;

        public DAT()
        {
            Strings = new List<string>();

        }
        public static byte[] Crypt(byte[] data, uint size)
        {
            uint ebx = 0;
            uint edi = size;
            uint position = 0;
            ulong r15 = 0;

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
                data[r15 - 1] ^= (byte)eax;
            }

            return data;
        }
    
        public void Read(string filepath)
        {
            byte[] data = File.ReadAllBytes(filepath);

            uint size = (uint)data.Length + Path.GetFileNameWithoutExtension(filepath)[0] - 65;
            data = Crypt(data, size);
            data = DATCompression.Decompress(data);

            DATBinaryReader br = new DATBinaryReader(data);

            ReadStrings(br);
        }

        public void Write(string filepath)
        {
            DATBinaryWriter bw = new DATBinaryWriter();

            WriteStrings(bw);

            byte[] data = bw.DATBinaryWriterData.ToArray();

            data = DATCompression.Compress(data);
            uint size = (uint)data.Length + Path.GetFileNameWithoutExtension(filepath)[0] - 65;
            data = Crypt(data, size);
        }
        private void ReadStrings(DATBinaryReader reader)
        {
            while (reader.Position < reader.Length)
                Strings.Add(reader.ReadString());
        }

        private void WriteStrings(DATBinaryWriter writer)
        {
            foreach (string s in Strings)
                writer.WriteString(s);
        }
    }
}
