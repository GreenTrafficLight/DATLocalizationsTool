using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using DATLocalizationsTool.Formats;
using System.ComponentModel;

namespace DATLocalizationsTool
{
    public class StringGrid
    {
        public TreeView treeView1;
        public DataGridView dataGridView1;

        private ComboBox comboBox1;

        private List<string> _validStrings = new List<string> {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "Cmn"
        };
        private string _filePath = "";

        public List<Tuple<DAT, string>> Dats = new List<Tuple<DAT, string>>();
        public CMN Cmn = null;

        public StringGrid(DataGridView dataGridView, TreeView treeView, ComboBox comboBox)
        {
            dataGridView1 = dataGridView;
            treeView1 = treeView;
            comboBox1 = comboBox;

            LoadDataGridView();
        }
        public void LoadFile(string filepath)
        {
            try
            {
                if (_validStrings.Contains(Path.GetFileNameWithoutExtension(filepath)))
                {
                    _filePath = Path.GetDirectoryName(filepath);

                    string fileName = Path.GetFileNameWithoutExtension(filepath);

                    // Load letter .dat
                    if (fileName != "Cmn")
                    {
                        LoadDat(filepath);
                    }
                    // Load Cmn.dat
                    else if (fileName == "Cmn")
                    {
                        LoadCmn(filepath);
                    }
                }
                else
                {
                    MessageBox.Show(".dat name has been modified`.\nIt should keep the original name", "Modified .dat name Error");
                }

            }
            catch (Exception ex)
            {

            }
        }

        public void LoadDat(string filepath)
        {
            DAT dat = new DAT();
            dat.Read(filepath);

            string fileName = Path.GetFileNameWithoutExtension(filepath);

            // Check if .dat is already loaded
            int index = Dats.FindIndex(d => d.Item2 == fileName);
            if (index == -1)
            {
                // Add to comboBox if it isn't loaded
                comboBox1.Items.Add(fileName);
                Tuple<DAT, string> datComboBox = new Tuple<DAT, string>(dat, fileName);
                Dats.Add(datComboBox);
                index = Dats.IndexOf(datComboBox);
            }
            else
            {
                // Replace the .dat loaded in the comboBox
                comboBox1.Items[index] = fileName;
                Tuple<DAT, string> datComboBox = new Tuple<DAT, string>(dat, fileName);
                Dats[index] = datComboBox;
            }

            if (Cmn != null)
            {
                int toAdd = 0;
                foreach (CMN.CmnTreeNode child in Cmn.Root.childrens)
                {
                    AddCmnTreeNodeToCMN(child, index, ref toAdd);
                }

                Dats[index].Item1.Strings.AddRange(Enumerable.Repeat("\0", toAdd));
            }

            comboBox1.SelectedIndex = Dats.FindIndex(d => d.Item2 == fileName);


        }

        public void LoadCmn(string filepath)
        {
            if (Cmn == null)
            {
                Cmn = new CMN();
                Cmn.Read(filepath);
                AddToTreeView();
            }
        }

        public void SaveFile()
        {
            if (!string.IsNullOrEmpty(_filePath))
            {
                if (Cmn != null)
                {
                    if (File.Exists(_filePath + "\\Cmn.dat"))
                        File.Copy(_filePath + "\\Cmn.dat", _filePath + "\\Cmn.dat" + ".bak", true);

                    Cmn.Write(_filePath + "\\Cmn.dat");
                }

                SaveDats(_filePath);
            }
        }

        public void SaveDats(string filepath)
        {
            foreach ((DAT dat, string datName) in Dats)
            {
                if (File.Exists(filepath + "\\" + datName + ".dat"))
                    File.Copy(filepath + "\\" + datName + ".dat", filepath + "\\" + datName + ".dat" + ".bak", true);

                dat.Write(filepath + "\\" + datName + ".dat");
            }
        }

        public bool SearchTreeView(string searchString, TreeNode node)
        {
            if (node.Text.ToLower().Contains(searchString.ToLower()))
            {
                treeView1.SelectedNode = node;
                return true;
            }

            // Traverse child nodes recursively
            foreach (TreeNode childNode in node.Nodes)
            {
                if (SearchTreeView(searchString, childNode))
                {
                    return true;
                }
            }

            return false;
        }

        #region dataGridView
        public void LoadDataGridView()
        {
            dataGridView1.Columns.Clear();
            dataGridView1.Columns.Add("designNumber", "Number");
            dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns[0].ReadOnly = true;
            dataGridView1.Columns.Add("designID", "ID");
            dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns[1].ReadOnly = true;
            dataGridView1.Columns.Add("designText", "Text");
            dataGridView1.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns[2].ReadOnly = true;

            dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Ascending);
        }
        
        public void AddToDataGridView()
        {
            dataGridView1.Rows.Clear();

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
            else if (comboBox1.SelectedIndex != -1 && Cmn != null)
            {
                if (treeView1.SelectedNode is CMN.CmnTreeNode cmnTreeNode)
                {
                    AddCmnTreeNodeToDataGridView(cmnTreeNode);
                    dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Ascending);
                }
            }
        }

        // Add the strings in the CMN from the Root
        public void AddCmnTreeNodeToCMN(CMN.CmnTreeNode cmnTreeNode, int datIndex, ref int toAdd)
        {
            if (cmnTreeNode.StringNumber != -1)
            {
                // If the string in the CMN isn't in the DAT, add it
                if (Dats[datIndex].Item1.Strings.Count <= cmnTreeNode.StringNumber)
                {
                    toAdd++;
                    //Dats[datIndex].Item1.Strings.Add("\0");
                }
            }

            foreach (CMN.CmnTreeNode children in cmnTreeNode.childrens)
                AddCmnTreeNodeToCMN(children, datIndex, ref toAdd);
        }

        // Add the strings in the StringGrid from the selected TreeNode
        public void AddCmnTreeNodeToDataGridView(CMN.CmnTreeNode cmnTreeNode)
        {
            if (cmnTreeNode.StringNumber != -1)
            {
                string text = Dats[comboBox1.SelectedIndex].Item1.Strings[cmnTreeNode.StringNumber];
                dataGridView1.Rows.Add(cmnTreeNode.StringNumber, cmnTreeNode.Text, text);
            }

            foreach (CMN.CmnTreeNode children in cmnTreeNode.childrens)
                AddCmnTreeNodeToDataGridView(children);
        }

        public void AddCmnTreeNodeToDataGridView(CMN.CmnTreeNode cmnTreeNode, string searchString)
        {
            if (cmnTreeNode.StringNumber != -1)
            {
                string text = Dats[comboBox1.SelectedIndex].Item1.Strings[cmnTreeNode.StringNumber];
                if (text.ToLower().Contains(searchString.ToLower()))
                    dataGridView1.Rows.Add(cmnTreeNode.StringNumber, cmnTreeNode.Text, text);
            }

            foreach (CMN.CmnTreeNode children in cmnTreeNode.childrens)
                AddCmnTreeNodeToDataGridView(children, searchString);
        }

        #endregion

        #region TreeView
        private void AddToTreeView()
        {
            foreach (CMN.CmnTreeNode children in Cmn.Root.childrens)
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

    }
}
