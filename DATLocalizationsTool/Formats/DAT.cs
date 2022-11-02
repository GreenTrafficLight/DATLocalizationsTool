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

        public DAT(string filepath)
        {
            Strings = new List<string>();
            byte[] data = File.ReadAllBytes(filepath);
            
            uint size = (uint)data.Length + Path.GetFileNameWithoutExtension(filepath)[0] - 65;
            data = Crypt(data, size);
            data = Decompress(data);

            DATBinaryReader br = new DATBinaryReader(data);
            ReadStrings(br);

            /*
            data = Compress(data);
            size = (uint)data.Length + Path.GetFileNameWithoutExtension(filepath)[0] - 65;
            data = Crypt(data, size);
            */


            Console.WriteLine("test");
        }

        public byte[] Decompress(byte[] data)
        {
            Inflater decompressor = new Inflater();
            decompressor.SetInput(data);

            MemoryStream bos = new MemoryStream(data.Length);

            byte[] buffer = new byte[1024];
            while (!decompressor.IsFinished)
            {
                int length = decompressor.Inflate(buffer);
                bos.Write(buffer, 0, length);
            }

            return bos.ToArray();
        }

        public byte[] Compress(byte[] data)
        {
            Deflater compressor = new Deflater();
            compressor.SetLevel(Deflater.DEFAULT_COMPRESSION);

            compressor.SetInput(data);
            compressor.Finish();

            MemoryStream bos = new MemoryStream(data.Length);

            byte[] buffer = new byte[1024];
            while (!compressor.IsFinished)
            {
                int length = compressor.Deflate(buffer);
                bos.Write(buffer, 0, length);
            }

            return bos.ToArray();
        }

        public byte[] Crypt(byte[] data, uint size)
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
    
        private void ReadStrings(DATBinaryReader reader)
        {
            while (reader.Position < reader.Length)
                Strings.Add(reader.ReadString());
        }
    }
}
