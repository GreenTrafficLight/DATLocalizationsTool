using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        /// <summary>
        /// Crypt and decrypt a DAT file
        /// </summary>
        /// <param name="data">The data of the DAT file</param>
        /// <param name="size">The size of the DAT file + the letter converted to int subtracted by 65( except for CMN )</param>
        /// <returns></returns>
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
        
        /// <summary>
        /// Read the DAT file
        /// </summary>
        /// <param name="filepath">The path of the DAT file</param>
        public void Read(string filepath)
        {
            byte[] data = File.ReadAllBytes(filepath);

            uint size = (uint)data.Length + Path.GetFileNameWithoutExtension(filepath)[0] - 65;
            data = Crypt(data, size);
            data = DATCompression.Decompress(data);

            DATBinaryReader br = new DATBinaryReader(data);

            ReadStrings(br);
        }

        /// <summary>
        /// Write a DAT file
        /// </summary>
        /// <param name="filepath">The path of the new DAT file</param>
        public void Write(string filepath)
        {
            DATBinaryWriter bw = new DATBinaryWriter();

            WriteStrings(bw);

            byte[] data = bw.DATBinaryWriterData.ToArray();

            data = DATCompression.Compress(data);
            uint size = (uint)data.Length + Path.GetFileNameWithoutExtension(filepath)[0] - 65;
            data = Crypt(data, size);

            File.WriteAllBytes(filepath, data);
        }
        
        /// <summary>
        /// Read the strings contained in a DAT file
        /// </summary>
        /// <param name="reader"></param>
        private void ReadStrings(DATBinaryReader reader)
        {
            while (reader.Position < reader.Length)
            {
                Strings.Add(reader.ReadString());
            }
                
        }

        /// <summary>
        /// Write strings in a new DAT file
        /// </summary>
        /// <param name="writer"></param>
        private void WriteStrings(DATBinaryWriter writer)
        {
            foreach (string s in Strings)
                writer.WriteString(s);
        }
    }
}
