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

            public CmnTreeNode() : base()
            {
                Text = "";
                Name = "";

                VariableName = "";
                StringNumber = -1;
            }

            public override object Clone()
            {
                var obj = (CmnTreeNode)base.Clone();
                obj.VariableName = this.VariableName;
                obj.StringNumber = this.StringNumber;
                obj.childrens = this.childrens;
                return obj;
            }

            public void SetProperties(string nodeText, string name, int stringNumber)
            {
                // Node properties
                Text = nodeText;
                Name = nodeText;

                // CmnTreeNode properties
                VariableName = name;
                StringNumber = stringNumber;
            }
        }

        public CmnTreeNode Root = new CmnTreeNode();
        public int stringsCount = 0;

        public void Read(string filepath)
        {
            byte[] data = File.ReadAllBytes(filepath);

            data = DAT.Crypt(data, (uint)data.Length);
            data = DATCompression.Decompress(data);

            DATBinaryReader br = new DATBinaryReader(data);

            Root = ReadVariables(br, Root);
        }
        
        public void Write(string filepath)
        {
            DATBinaryWriter bw = new DATBinaryWriter();

            bw.WriteInt(Root.childrens.Count);
            foreach(CmnTreeNode children in Root.childrens)
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
                if (stringsCount < stringNumber)
                {
                    stringsCount = stringNumber;
                }
                CmnTreeNode node = new CmnTreeNode();
                node.SetProperties(parent.Text + name, name, stringNumber);

                parent.childrens.Add(ReadVariables(br, node));
            }
            return parent;
        }

        private void WriteVariables(DATBinaryWriter bw, CmnTreeNode node)
        {
            //if (node.Text == "Aircraft_Name_a130_test")
                //Console.WriteLine("test");

            bw.WriteInt(node.VariableName.Length);
            bw.WriteString(node.VariableName);
            bw.WriteInt(node.StringNumber);
            bw.WriteInt(node.childrens.Count);
            foreach (CmnTreeNode children in node.childrens)
                WriteVariables(bw, children);
        }
    }
}
