using DATLocalizationsTool.Formats;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace DATLocalizationsTool
{
    public partial class Form1 : Form
    {

        private List<string> ValidStrings = new List<string> {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "Cmn"
        };

        List<DAT> Dats = new List<DAT>();
        DAT dat = null;
        CMN cmn = null;
        public Form1()
        {
            InitializeComponent();
        }
        private void LoadFile(string filepath)
        {
            try
            {
                if (ValidStrings.Contains(Path.GetFileNameWithoutExtension(filepath)))
                {
                    if (Path.GetFileNameWithoutExtension(filepath) != "Cmn")
                    {
                        dat = new DAT();
                        dat.Read(filepath);
                        comboBox1.Items.Add(Path.GetFileNameWithoutExtension(filepath));
                        Dats.Add(dat);
                        //dat.Write(filepath);
                    }
                    else if (Path.GetFileNameWithoutExtension(filepath) == "Cmn")
                    {
                        cmn = new CMN();
                        cmn.Read(filepath);
                        AddToTreeView();
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }

        private void AddToDataGridView()
        {
            if (comboBox1.SelectedIndex != -1 && cmn == null)
            {
                int index = 0;
                foreach (string s in Dats[comboBox1.SelectedIndex].Strings)
                {
                    dataGridView1.Rows.Add(index, null, s);
                    index++;
                }
                dataGridView1.AutoResizeRows();
            }
            else if (comboBox1.SelectedIndex != -1)
            {
                if (treeView1.SelectedNode is CMN.CmnTreeNode cmnTreeNode)
                {
                        AddCmnTreeNodeToDataGridView(cmnTreeNode);
                }
            }
        }
        
        private void AddToTreeView()
        {
            foreach (CMN.CmnTreeNode children in cmn.root.childrens)
                AddCmnTreeNodeToTreeView(children, null);
        }

        private void AddCmnTreeNodeToTreeView(CMN.CmnTreeNode cmnTreeNode, CMN.CmnTreeNode cmnTreeNodeParent)
        {
            if (cmnTreeNode == null)
                return;

            if (cmnTreeNodeParent == null)
                treeView1.Nodes.Add(cmnTreeNode);
            else
                cmnTreeNodeParent.Nodes.Add(cmnTreeNode);

            foreach (CMN.CmnTreeNode children in cmnTreeNode.childrens)
                AddCmnTreeNodeToTreeView(children, cmnTreeNode);
        }

        private void AddCmnTreeNodeToDataGridView(CMN.CmnTreeNode cmnTreeNode)
        {
            if (cmnTreeNode.StringNumber != -1)
            {
                string text = Dats[comboBox1.SelectedIndex].Strings[cmnTreeNode.StringNumber];
                dataGridView1.Rows.Add(cmnTreeNode.StringNumber, cmnTreeNode.Text, text);
            }

            foreach (CMN.CmnTreeNode children in cmnTreeNode.childrens)
                AddCmnTreeNodeToDataGridView(children);
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null && !dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly)
            {
                string cellText = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                var textForm = new TextForm(cellText, e.RowIndex, e.ColumnIndex);
                textForm.Show();
                
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Dat file|*.dat";
                ofd.Title = "Open dat file";

                if (ofd.ShowDialog() == DialogResult.OK)
                    LoadFile(ofd.FileName);
            }
        }

        private void openDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(fbd.SelectedPath))
                {
                    string[] files = Directory.GetFiles(fbd.SelectedPath);

                    foreach (string file in files)
                        LoadFile(file);
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            AddToDataGridView();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            dataGridView1.Rows.Clear();
            AddToDataGridView();
        }
    }
}
