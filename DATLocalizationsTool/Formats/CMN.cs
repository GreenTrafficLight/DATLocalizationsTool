using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DATLocalizationsTool.Commons;

namespace DATLocalizationsTool.Formats
{
    public class CMN
    {
        public class CmnTreeNode
        {
            public string name;
            public int stringNumber;
            public List<CmnTreeNode> childrens = new List<CmnTreeNode>();

            public CmnTreeNode()
            {
                this.name = "";
                this.stringNumber = -1;
            }
        }

        public CmnTreeNode root = new CmnTreeNode();
        public CMN(string filepath)
        {
            byte[] data = File.ReadAllBytes(filepath);

            data = DAT.Crypt(data, (uint)data.Length);
            data = DATCompression.Decompress(data);

            DATBinaryReader br = new DATBinaryReader(data);

            root = ReadVariables(br, root);
        }

        public CmnTreeNode ReadVariables(DATBinaryReader br, CmnTreeNode parent)
        {

            int count = br.ReadInt();
            for (int i = 0; i < count; i++)
            {
                CmnTreeNode node = new CmnTreeNode();
                int nameLength = br.ReadInt();
                node.name = parent.name + br.ReadString(nameLength);
                node.stringNumber = br.ReadInt();
                Console.WriteLine(node.name);
                parent.childrens.Add(ReadVariables(br, node));
            }
            return parent;
        }
    }
}
