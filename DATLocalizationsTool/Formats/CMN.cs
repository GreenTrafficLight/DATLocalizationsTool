using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DATLocalizationsTool.Commons;

namespace DATLocalizationsTool.Formats
{
    public class CMN
    {
        public class CmnTreeNode : TreeNode
        {
            public int StringNumber = -1;
            public List<CmnTreeNode> childrens = new List<CmnTreeNode>();

            public CmnTreeNode(string name, int stringNumber)
            {
                Text = name;
                StringNumber = stringNumber;
            }
        }

        public CmnTreeNode root = new CmnTreeNode("", -1);
        public void Read(string filepath)
        {
            byte[] data = File.ReadAllBytes(filepath);

            data = DAT.Crypt(data, (uint)data.Length);
            data = DATCompression.Decompress(data);

            DATBinaryReader br = new DATBinaryReader(data);

            root = ReadVariables(br, root);
        }

        private CmnTreeNode ReadVariables(DATBinaryReader br, CmnTreeNode parent)
        {

            int count = br.ReadInt();
            for (int i = 0; i < count; i++)
            {
                
                int nameLength = br.ReadInt();
                string name = parent.Text + br.ReadString(nameLength);
                int stringNumber = br.ReadInt();
                CmnTreeNode node = new CmnTreeNode(name, stringNumber);
                parent.childrens.Add(ReadVariables(br, node));
            }
            return parent;
        }
    }
}
