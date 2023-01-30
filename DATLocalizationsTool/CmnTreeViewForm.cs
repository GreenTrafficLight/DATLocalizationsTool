using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DATLocalizationsTool.Formats;

namespace DATLocalizationsTool
{
    public partial class CmnTreeViewForm : Form
    {
        public string choosenNode;
        public CmnTreeViewForm(CMN Cmn)
        {
            InitializeComponent();

            var notAssignedNode = treeView1.Nodes.Add("N/A");
            notAssignedNode.ForeColor = Color.Gray;

            foreach (CMN.CmnTreeNode children in Cmn.Root.childrens)
                AddCmnTreeNodeToTreeView(children);
        }
        private void AddCmnTreeNodeToTreeView(CMN.CmnTreeNode cmnTreeNode)
        {
            if (cmnTreeNode == null)
                return;

            if (cmnTreeNode.StringNumber == -1 && (cmnTreeNode.Nodes.Count == 0))
                treeView1.Nodes.Add((CMN.CmnTreeNode)cmnTreeNode.Clone());

            foreach (CMN.CmnTreeNode children in cmnTreeNode.childrens)
                AddCmnTreeNodeToTreeView(children);
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            choosenNode = e.Node.Text;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
