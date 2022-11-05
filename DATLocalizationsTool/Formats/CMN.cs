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
            public string VariableName = "";
            public int StringNumber = -1;
            public List<CmnTreeNode> childrens = new List<CmnTreeNode>();

            public CmnTreeNode(string nodeText, string name, int stringNumber)
            {
                Text = nodeText;
                Name = nodeText;
                VariableName = name;
                StringNumber = stringNumber;
            }
        }

        public CmnTreeNode root = new CmnTreeNode("", "", -1);
        public void Read(string filepath)
        {
            byte[] data = File.ReadAllBytes(filepath);

            data = DAT.Crypt(data, (uint)data.Length);
            data = DATCompression.Decompress(data);

            DATBinaryReader br = new DATBinaryReader(data);

            root = ReadVariables(br, root);
        }
        public void Write(string filepath)
        {
            DATBinaryWriter bw = new DATBinaryWriter();

            bw.WriteInt(root.childrens.Count);
            foreach(CmnTreeNode children in root.childrens)
                WriteVariables(bw, children);

            byte[] data = bw.DATBinaryWriterData.ToArray();

            data = DATCompression.Compress(data);
            uint size = (uint)data.Length;
            data = DAT.Crypt(data, size);

            File.WriteAllBytes(filepath, data);
        }

        private CmnTreeNode ReadVariables(DATBinaryReader br, CmnTreeNode parent)
        {

            int count = br.ReadInt();
            for (int i = 0; i < count; i++)
            {
                
                int nameLength = br.ReadInt();
                string name = br.ReadString(nameLength);
                int stringNumber = br.ReadInt();
                CmnTreeNode node = new CmnTreeNode(parent.Text + name, name, stringNumber);
                parent.childrens.Add(ReadVariables(br, node));
            }
            return parent;
        }

        private void WriteVariables(DATBinaryWriter bw, CmnTreeNode node)
        {
            if (node.Text == "AircraftSkin_Description_zoef_0")
                Console.WriteLine("test");

            bw.WriteInt(node.VariableName.Length);
            bw.WriteString(node.VariableName);
            bw.WriteInt(node.StringNumber);
            bw.WriteInt(node.childrens.Count);
            foreach (CmnTreeNode children in node.childrens)
                WriteVariables(bw, children);
        }
    }
}
