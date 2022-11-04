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

        List<Tuple<DAT, string>> Dats = new List<Tuple<DAT, string>>();
        CMN Cmn = null;
        string FilePath = "";
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
                    FilePath = Path.GetDirectoryName(filepath);

                    // Load letter .dat
                    if (Path.GetFileNameWithoutExtension(filepath) != "Cmn")
                    {
                        DAT dat = new DAT();
                        dat.Read(filepath);
                        comboBox1.Items.Add(Path.GetFileNameWithoutExtension(filepath));
                        Dats.Add(new Tuple<DAT, string>(dat, Path.GetFileNameWithoutExtension(filepath)));
                        //dat.Write(filepath);
                    }
                    // Load Cmn.dat
                    else if (Path.GetFileNameWithoutExtension(filepath) == "Cmn")
                    {
                        if (Cmn == null)
                        {
                            Cmn = new CMN();
                            Cmn.Read(filepath);
                            AddToTreeView();
                            //cmn.Write(filepath);
                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }

        private void SaveFile(string filepath)
        {
            if (!string.IsNullOrEmpty(filepath))
            {
                if (Cmn != null)
                {
                    if (File.Exists(filepath + "\\Cmn.dat"))
                        File.Copy(filepath + "\\Cmn.dat", filepath + "\\Cmn.dat"+ ".bak", true);

                    Cmn.Write(filepath + "\\Cmn.dat");
                }

                SaveDats(filepath);
            }
        }

        private void SaveDats(string filepath)
        {
            foreach ((DAT dat, string datName) in Dats)
            {
                if (File.Exists(filepath + "\\" + datName + ".dat"))
                    File.Copy(filepath + "\\" + datName + ".dat", filepath + "\\" + datName + ".dat" + ".bak", true);

                dat.Write(filepath + "\\" + datName + ".dat");
            }
        }
        
        #region dataGridView
        private void AddToDataGridView()
        {
            // Load all strings if Cmn isn't loaded
            if (comboBox1.SelectedIndex != -1 && Cmn == null)
            {
                int index = 0;
                foreach (string s in Dats[comboBox1.SelectedIndex].Item1.Strings)
                {
                    dataGridView1.Rows.Add(index, null, s);
                    index++;
                }
                dataGridView1.AutoResizeRows();
            }
            // Load strings according to Cmn.dat
            else if (comboBox1.SelectedIndex != -1)
            {
                if (treeView1.SelectedNode is CMN.CmnTreeNode cmnTreeNode)
                {
                    AddCmnTreeNodeToDataGridView(cmnTreeNode);
                }
            }
        }

        private void AddCmnTreeNodeToDataGridView(CMN.CmnTreeNode cmnTreeNode)
        {
            if (cmnTreeNode.StringNumber != -1)
            {
                string text = Dats[comboBox1.SelectedIndex].Item1.Strings[cmnTreeNode.StringNumber];
                dataGridView1.Rows.Add(cmnTreeNode.StringNumber, cmnTreeNode.Text, text);
            }

            foreach (CMN.CmnTreeNode children in cmnTreeNode.childrens)
                AddCmnTreeNodeToDataGridView(children);
        }

        #endregion

        #region TreeView
        private void AddToTreeView()
        {
            foreach (CMN.CmnTreeNode children in Cmn.root.childrens)
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

        #endregion

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;
            
            // Modify existing string
            if (dataGridView1.Rows[e.RowIndex].Cells[2].Value != null)
            {
                string cellText = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

                int stringNumber = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells[0].Value);
                using (var textForm = new TextForm(cellText, e.RowIndex, e.ColumnIndex))
                {
                    if (textForm.ShowDialog() == DialogResult.Cancel)
                    {
                        dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = textForm.DatText;
                        Dats[comboBox1.SelectedIndex].Item1.Strings[stringNumber] = textForm.DatText;
                    }
                }
            }
            // Add new string
            else if (dataGridView1.Rows[e.RowIndex].Cells[2].Value == null)
            {
                int stringNumber = Dats[comboBox1.SelectedIndex].Item1.Strings.Count;
                using (var textForm = new TextForm("", e.RowIndex, e.ColumnIndex))
                {
                    if (textForm.ShowDialog() == DialogResult.Cancel)
                    {
                        dataGridView1.Rows.Add(stringNumber, null, textForm.DatText);
                        Dats[comboBox1.SelectedIndex].Item1.Strings.Add(textForm.DatText);
                        // Add string to other Dats
                        for (int i = 0; i < comboBox1.Items.Count; i++)
                        {
                            if (i != comboBox1.SelectedIndex)
                                Dats[i].Item1.Strings.Add("");
                        }
                    }
                }           
            }
        }

        #region ToolStripMenuItem Events
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

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile(FilePath);
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

        #endregion
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            AddToDataGridView();
        }

        #region TreeView Events
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            dataGridView1.Rows.Clear();
            AddToDataGridView();
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeView1.SelectedNode = e.Node;

                ContextMenu contextMenu = new ContextMenu();

                if (comboBox1.SelectedIndex != -1)
                {
                    MenuItem newMenuItem = new MenuItem("New");
                    newMenuItem.Click += new EventHandler(MenuItem_Click);
                    newMenuItem.Name = "New";
                    contextMenu.MenuItems.Add(newMenuItem);
                }

                /*MenuItem renameMenuItem = new MenuItem("Rename");
                renameMenuItem.Click += new EventHandler(MenuItem_Click);
                renameMenuItem.Name = "Rename";
                contextMenu.MenuItems.Add(renameMenuItem);*/

                MenuItem exitMenuItem = new MenuItem("Exit");
                contextMenu.MenuItems.Add(exitMenuItem);

                contextMenu.Show(treeView1, MousePosition);
            }
        }

        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            // TO DO : FIX WITH MENU ITEM RENAME
            if (e.Label != null)
            {
                // Update cmnTreeNodeName when editing is finished
                e.Node.EndEdit(false);
                CMN.CmnTreeNode cmnTreeNode = (CMN.CmnTreeNode)e.Node;
                cmnTreeNode.Text = e.Label;

                // Refresh DataGridView with updated cmnTreeNode name
                dataGridView1.Rows.Clear();
                AddToDataGridView();
            }
            else
            {
                e.CancelEdit = true;
                MessageBox.Show("Invalid tree node label.\nThe label cannot be blank",
                   "Node Label Edit");
                e.Node.BeginEdit();
            }
        }

        #endregion
        private void MenuItem_Click(object sender, EventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;

            if (menuItem.Name == "New")
            {
                if (treeView1.SelectedNode is CMN.CmnTreeNode cmnTreeNode)
                {
                    int stringNumber = Dats[comboBox1.SelectedIndex].Item1.Strings.Count;

                    // Add string to other Dats
                    for (int i = 0; i < comboBox1.Items.Count; i++)
                        Dats[i].Item1.Strings.Add("");

                    //Dats[comboBox1.SelectedIndex].Strings.Add("");

                    CMN.CmnTreeNode newCmnTreeNode = new CMN.CmnTreeNode(cmnTreeNode.Text, cmnTreeNode.Text, stringNumber);
                    cmnTreeNode.childrens.Add(newCmnTreeNode);

                    // Add newCmnTreeNode to treeView1
                    cmnTreeNode.Nodes.Add(newCmnTreeNode);

                    // Begin editing of newCmnTreeNode name
                    treeView1.SelectedNode = newCmnTreeNode;
                    treeView1.LabelEdit = true;
                    treeView1.SelectedNode.BeginEdit();
                }
            }
            else if (menuItem.Name == "Rename")
            {
                if (treeView1.SelectedNode is CMN.CmnTreeNode cmnTreeNode)
                {
                    treeView1.LabelEdit = true;
                    treeView1.SelectedNode.BeginEdit();
                }
            }
        }


    }
}
