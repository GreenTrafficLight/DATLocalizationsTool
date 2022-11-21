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
    /*
     * Add empty string for dats that aren't loaded if a dat has more strings
     * Fix the rename option
     * Add "modify other dats" option to TextForm
     * Add unsaved changes message box for TextForm
     */
    public partial class Form1 : Form
    {
        private bool _renameOption = false;

        public StringGrid StringGridEditor;

        public Form1()
        {
            InitializeComponent();

            StringGridEditor = new StringGrid(dataGridView1, treeView1, comboBox1);
        }
        
        #region dataGridView1 Events
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            if (comboBox1.SelectedIndex != -1)
            {
                // Modify existing string
                if (dataGridView1.Rows[e.RowIndex].Cells[2].Value != null)
                {
                    string cellText = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

                    int stringNumber = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells[0].Value);
                    using (var textForm = new TextForm(cellText, e.RowIndex, e.ColumnIndex))
                    {
                        if (textForm.ShowDialog() == DialogResult.Cancel)
                        {
                            dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = textForm.DatText + "\0";
                            StringGridEditor.Dats[comboBox1.SelectedIndex].Item1.Strings[stringNumber] = textForm.DatText + "\0";
                        }
                    }
                }
                // Add new string
                else if (dataGridView1.Rows[e.RowIndex].Cells[2].Value == null)
                {
                    int stringNumber = StringGridEditor.Dats[comboBox1.SelectedIndex].Item1.Strings.Count;
                    using (var textForm = new TextForm("", e.RowIndex, e.ColumnIndex))
                    {
                        if (textForm.ShowDialog() == DialogResult.Cancel)
                        {
                            dataGridView1.Rows.Add(stringNumber, null, textForm.DatText + "\0");
                            StringGridEditor.Dats[comboBox1.SelectedIndex].Item1.Strings.Add(textForm.DatText + "\0");
                            // Add string to other Dats
                            for (int i = 0; i < comboBox1.Items.Count; i++)
                            {
                                if (i != comboBox1.SelectedIndex)
                                    StringGridEditor.Dats[i].Item1.Strings.Add("");
                            }
                        }
                    }
                }
            }
        }
        
        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F)
            {
                using (SearchForm searchForm = new SearchForm())
                {
                    if (searchForm.ShowDialog() == DialogResult.OK)
                    {
                        if (StringGridEditor.Cmn == null)
                        {
                            dataGridView1.Rows.Clear();
                            int index = 0;
                            foreach (string text in StringGridEditor.Dats[comboBox1.SelectedIndex].Item1.Strings)
                            {
                                if (text.ToLower().Contains(searchForm.SearchString.ToLower()))
                                    dataGridView1.Rows.Add(index, null, text);
                                index++;
                            }
                        }
                        else if (StringGridEditor.Cmn != null)
                        {
                            dataGridView1.Rows.Clear();
                            if (treeView1.SelectedNode is CMN.CmnTreeNode cmnTreeNode && comboBox1.SelectedIndex != -1)
                                StringGridEditor.AddCmnTreeNodeToDataGridView(cmnTreeNode, searchForm.SearchString);
                            else if (treeView1.SelectedNode == null && comboBox1.SelectedIndex != -1)
                                StringGridEditor.AddCmnTreeNodeToDataGridView(StringGridEditor.Cmn.Root, searchForm.SearchString);
                            dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Ascending);
                        }
                    }
                }
            }
        }
        
        private void dataGridView1_DragDrop(object sender, DragEventArgs e)
        {
            string[] filepaths = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            foreach (string filepath in filepaths)
                StringGridEditor.LoadFile(filepath);
        }

        private void dataGridView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        #endregion

        #region ToolStripMenuItem Events
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Dat file|*.dat";
                ofd.Title = "Open dat file";

                if (ofd.ShowDialog() == DialogResult.OK)
                    StringGridEditor.LoadFile(ofd.FileName);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringGridEditor.SaveFile();
        }
        
        private void openDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(fbd.SelectedPath))
                {
                    string[] files = Directory.GetFiles(fbd.SelectedPath);

                    foreach (string file in files)
                        StringGridEditor.LoadFile(file);
                }
            }
        }

        #endregion
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            StringGridEditor.AddToDataGridView();
        }

        #region TreeView Events
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            StringGridEditor.AddToDataGridView();
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

                MenuItem renameMenuItem = new MenuItem("Rename");
                renameMenuItem.Click += new EventHandler(MenuItem_Click);
                renameMenuItem.Name = "Rename";
                contextMenu.MenuItems.Add(renameMenuItem);

                MenuItem exitMenuItem = new MenuItem("Exit");
                contextMenu.MenuItems.Add(exitMenuItem);

                contextMenu.Show(treeView1, treeView1.PointToClient(Cursor.Position));
            }
        }

        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label != null)
            {
                // Update cmnTreeNodeName when editing is finished
                e.Node.EndEdit(false);
                CMN.CmnTreeNode cmnTreeNode = (CMN.CmnTreeNode)e.Node;
                int lengthToRemove = 0;
                if (_renameOption == true)
                {
                    lengthToRemove = cmnTreeNode.Parent.Text.Length;
                    _renameOption = false;
                }
                else
                    lengthToRemove = cmnTreeNode.Text.Length;

                cmnTreeNode.Text = e.Label;
                cmnTreeNode.Name = e.Label;
                cmnTreeNode.VariableName = cmnTreeNode.Text.Remove(0, lengthToRemove);

                // Refresh DataGridView with updated cmnTreeNode name
                StringGridEditor.AddToDataGridView();
            }
            else
            {
                if (_renameOption == true)
                {
                    e.Node.EndEdit(false);
                    _renameOption = false;
                }
                else
                {
                    e.CancelEdit = true;
                    MessageBox.Show("Invalid tree node label.\nThe label cannot be blank",
                       "Node Label Edit");

                    // Remove string from dat
                    StringGridEditor.Dats[comboBox1.SelectedIndex].Item1.Strings.RemoveAt(StringGridEditor.Dats[comboBox1.SelectedIndex].Item1.Strings.Count - 1);

                    // Remove cmnTreeNode from the parent childrens
                    CMN.CmnTreeNode cmnTreeNodeParent = (CMN.CmnTreeNode)e.Node.Parent;
                    cmnTreeNodeParent.childrens.RemoveAt(cmnTreeNodeParent.childrens.Count - 1);

                    // Remove node from treeView
                    e.Node.Remove();
                }
            }
        }
        
        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F)
            {
                if (StringGridEditor.Cmn != null)
                {
                    using (SearchForm searchForm = new SearchForm())
                    {
                        if (searchForm.ShowDialog() == DialogResult.OK)
                            StringGridEditor.SearchTreeView(searchForm.SearchString, treeView1.Nodes);
                    }
                }

            }
        }
        
        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            string[] filepaths = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            foreach (string filepath in filepaths)
                StringGridEditor.LoadFile(filepath);
        }

        private void treeView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        #endregion
        private void MenuItem_Click(object sender, EventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;

            if (menuItem.Name == "New")
            {
                if (treeView1.SelectedNode is CMN.CmnTreeNode cmnTreeNode)
                {
                    // Get string number by getting the string count
                    int stringNumber = StringGridEditor.Dats[comboBox1.SelectedIndex].Item1.Strings.Count;

                    // Add string to other Dats
                    for (int i = 0; i < comboBox1.Items.Count; i++)
                        StringGridEditor.Dats[i].Item1.Strings.Add("");

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
                _renameOption = true;

                if (treeView1.SelectedNode is CMN.CmnTreeNode cmnTreeNode)
                {
                    treeView1.LabelEdit = true;
                    treeView1.SelectedNode.BeginEdit();
                }
            }
        }
    }
}
