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
        private bool _newOption = false;

        public StringGrid StringGridEditor;

        public Form1()
        {
            InitializeComponent();

            StringGridEditor = new StringGrid(dataGridView1, treeView1, comboBox1);
        }

        private void IDModications(int RowIndex, int stringNumber)
        {
            using (CmnTreeViewForm cmnTreeView = new CmnTreeViewForm(StringGridEditor.Cmn))
            {
                if (cmnTreeView.ShowDialog() == DialogResult.OK)
                {
                    var returnNode = treeView1.Nodes.Find(cmnTreeView.choosenNode, true);
                    if (returnNode.Length == 1)
                    {
                        CMN.CmnTreeNode cmnNode = (CMN.CmnTreeNode)returnNode[0];
                        // If text and string number has been assigned
                        if (dataGridView1.Rows[RowIndex].Cells[2].Value != null)
                        {
                            // Refresh to current string number value
                            stringNumber = Convert.ToInt32(dataGridView1.Rows[RowIndex].Cells[0].Value);
                            dataGridView1.Rows[RowIndex].Cells[0].Value = stringNumber.ToString();

                            dataGridView1.Rows[RowIndex].Cells[1].Value = cmnNode.Text;
                        }
                        // If text and string number has not been assigned
                        else if (dataGridView1.Rows[RowIndex].Cells[0].Value == null && dataGridView1.Rows[RowIndex].Cells[2].Value == null)
                        {
                            StringGridEditor.Dats[comboBox1.SelectedIndex].Item1.Strings.Add("\0");
                            dataGridView1.Rows.Add(stringNumber, cmnNode.Text, "\0");
                        }
                        // If text has not been assigned and string number has been assigned
                        else
                        {
                            dataGridView1.Rows[RowIndex].Cells[1].Value = cmnNode.Text;
                        }

                        cmnNode.StringNumber = stringNumber;
                    }
                    else
                    {
                        // Modifying ID value that has been assigned to N/A
                        if (dataGridView1.Rows[RowIndex].Cells[1].Value != null)
                        {
                            returnNode = treeView1.Nodes.Find(dataGridView1.Rows[RowIndex].Cells[1].Value.ToString(), true);
                            if (returnNode.Length == 1)
                            {
                                CMN.CmnTreeNode cmnNode = (CMN.CmnTreeNode)returnNode[0];
                                cmnNode.StringNumber = -1;
                                dataGridView1.Rows[RowIndex].Cells[1].Value = null;
                            }
                        }
                    }

                }
            }
        }
        
        private void StringModifications(object value, int RowIndex, int ColumnIndex, int stringNumber)
        {
            switch (value)
            {
                case null: // Add new string
                    using (var textForm = new TextForm("", RowIndex, ColumnIndex))
                    {
                        if (textForm.ShowDialog() == DialogResult.Cancel)
                        {
                            if (textForm.DatText != "")
                            {
                                if (dataGridView1.Rows[RowIndex].Cells[1].Value != null)
                                    dataGridView1.Rows[RowIndex].Cells[ColumnIndex].Value = textForm.DatText + "\0";
                                else
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
                    break;

                default: // Modify existing string
                    string cellText = dataGridView1.Rows[RowIndex].Cells[ColumnIndex].Value.ToString();

                    stringNumber = Convert.ToInt32(dataGridView1.Rows[RowIndex].Cells[0].Value);
                    using (var textForm = new TextForm(cellText, RowIndex, ColumnIndex))
                    {
                        if (textForm.ShowDialog() == DialogResult.Cancel)
                        {
                            dataGridView1.Rows[RowIndex].Cells[ColumnIndex].Value = textForm.DatText + "\0";
                            StringGridEditor.Dats[comboBox1.SelectedIndex].Item1.Strings[stringNumber] = textForm.DatText + "\0";
                        }
                    }
                    break;
            }
        }
        
        // Helper function to get the index of the common substring between two strings
        private int GetCommonSubstringIndex(string str1, string str2, int startIndex = 0)
        {
            int index = -1;

            for (int i = startIndex; i < Math.Min(str1.Length, str2.Length); i++)
            {
                if (str1[i] != str2[i])
                {
                    return index;
                }
                index++;
            }
            return index;
        }

        private void addTreeNodeBySorted(CMN.CmnTreeNode treeNode, CMN.CmnTreeNode treeNodeParent)
        {
            int sortIndex = 0;

            // Find the correct position to insert the new node
            while (sortIndex < treeNodeParent.Nodes.Count && string.Compare(treeNode.Text, treeNodeParent.Nodes[sortIndex].Text) > 0)
            {
                sortIndex++;
            }

            if (sortIndex < treeNodeParent.Nodes.Count)
            {
                treeNodeParent.Nodes.Insert(sortIndex, treeNode);
                treeNodeParent.childrens.Insert(sortIndex, treeNode);
            }
            else
            {
                treeNodeParent.Nodes.Add(treeNode);
                treeNodeParent.childrens.Add(treeNode);
            }
                
        }

        private void MergeNodes(CMN.CmnTreeNode addedCmnTreeNode, string subString, CMN.CmnTreeNode addedCmnTreeNodeParent)
        {
            for (int i = 0; i < addedCmnTreeNodeParent.Nodes.Count; i++)
            {
                CMN.CmnTreeNode treeNode = (CMN.CmnTreeNode)addedCmnTreeNodeParent.Nodes[i];

                int index = GetCommonSubstringIndex(addedCmnTreeNode.Text, treeNode.Text, subString.Length);

                if (index != -1 && !treeNode.Text.Equals(addedCmnTreeNode.Text))
                {
                    subString = treeNode.Text.Substring(0, addedCmnTreeNodeParent.Text.Length + index + 1);
                    addedCmnTreeNodeParent.Nodes.Remove(addedCmnTreeNode);
                    addedCmnTreeNodeParent.childrens.Remove(addedCmnTreeNode);
                    if (!subString.Equals(treeNode.Text))
                    {
                        CMN.CmnTreeNode mergedCmnTreeNode = new CMN.CmnTreeNode();
                        mergedCmnTreeNode.SetProperties(subString, subString.Remove(0, addedCmnTreeNodeParent.Text.Length), -1);
                        addedCmnTreeNodeParent.Nodes.Add(mergedCmnTreeNode);
                        addedCmnTreeNodeParent.childrens.Add(mergedCmnTreeNode);
                        
                        addedCmnTreeNodeParent.Nodes.Remove(treeNode);
                        addedCmnTreeNodeParent.childrens.Remove(treeNode);
                        
                        treeNode.SetProperties(treeNode.Text, treeNode.Text.Remove(0, mergedCmnTreeNode.Text.Length), treeNode.StringNumber);
                        mergedCmnTreeNode.Nodes.Add(treeNode);
                        mergedCmnTreeNode.childrens.Add(treeNode);

                        addedCmnTreeNode.SetProperties(addedCmnTreeNode.Text, addedCmnTreeNode.Text.Remove(0, mergedCmnTreeNode.Text.Length), addedCmnTreeNode.StringNumber);
                        if (!addedCmnTreeNode.Text.Equals(mergedCmnTreeNode.Text))
                            addTreeNodeBySorted(addedCmnTreeNode, mergedCmnTreeNode);                        
                    }
                    else
                    {
                        addedCmnTreeNode.SetProperties(addedCmnTreeNode.Text, addedCmnTreeNode.Text.Remove(0, addedCmnTreeNodeParent.Text.Length), addedCmnTreeNode.StringNumber);
                        addTreeNodeBySorted(addedCmnTreeNode, treeNode);
                    }
                    
                    MergeNodes(addedCmnTreeNode, subString, treeNode);
                    break;
                }
            }
        }
        private void Search()
        {
            using (SearchForm searchForm = new SearchForm())
            {
                if (searchForm.ShowDialog() == DialogResult.OK)
                {
                    switch (StringGridEditor.Cmn)
                    {
                        case null:
                            dataGridView1.Rows.Clear();
                            int index = 0;
                            foreach (string text in StringGridEditor.Dats[comboBox1.SelectedIndex].Item1.Strings)
                            {
                                if (text.ToLower().Contains(searchForm.SearchString.ToLower()))
                                    dataGridView1.Rows.Add(index, null, text);
                                index++;
                            }
                            break;

                        default:
                            dataGridView1.Rows.Clear();
                            if (treeView1.SelectedNode is CMN.CmnTreeNode cmnTreeNode && comboBox1.SelectedIndex != -1)
                                StringGridEditor.AddCmnTreeNodeToDataGridView(cmnTreeNode, searchForm.SearchString);
                            else if (treeView1.SelectedNode == null && comboBox1.SelectedIndex != -1)
                                StringGridEditor.AddCmnTreeNodeToDataGridView(StringGridEditor.Cmn.Root, searchForm.SearchString);
                            dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Ascending);
                            break;
                    }
                }
            }
        }

        #region dataGridView1 Events
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            if (comboBox1.SelectedIndex != -1)
            {
                int stringNumber = StringGridEditor.Dats[comboBox1.SelectedIndex].Item1.Strings.Count;

                switch (dataGridView1.CurrentCell.ColumnIndex)
                {
                    case 1: // Add new ID
                        IDModications(e.RowIndex, stringNumber);
                        break;
                    case 2: // String Modifications
                        StringModifications(dataGridView1.Rows[e.RowIndex].Cells[2].Value, e.RowIndex, e.ColumnIndex, stringNumber);
                        break;
                    default:
                        break;
                }
            }
            else if (comboBox1.SelectedIndex == -1 && StringGridEditor.Dats.Any())
            {
                MessageBox.Show("Select a language .dat in the top right corner", "DAT Selection");
            }
        }
        
        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F)
            {
                Search();
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

        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)        /*If a header cell*/
                return;

            if (e.Value == null || e.Value == DBNull.Value)  /*If value is null*/
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~(DataGridViewPaintParts.ContentForeground));

                switch (e.ColumnIndex)
                {
                    case 0:
                        TextRenderer.DrawText(e.Graphics, "N/A", e.CellStyle.Font, e.CellBounds, SystemColors.GrayText, TextFormatFlags.Left);
                        break;

                    case 1:
                        TextRenderer.DrawText(e.Graphics, "Select ID ( double click )", e.CellStyle.Font, e.CellBounds, SystemColors.GrayText, TextFormatFlags.Left);
                        break;

                    case 2:
                        TextRenderer.DrawText(e.Graphics, "Enter text", e.CellStyle.Font, e.CellBounds, SystemColors.GrayText, TextFormatFlags.Left);
                        break;

                    default:
                        break;
                }

                e.Handled = true;
            }
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

        #region comboBox1
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            StringGridEditor.AddToDataGridView();
        }

        #endregion

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

                if (treeView1.SelectedNode is CMN.CmnTreeNode cmnTreeNode)
                {
                    ContextMenu contextMenu = new ContextMenu();

                    MenuItem newMenuItem = new MenuItem("New");
                    newMenuItem.Click += new EventHandler(MenuItem_Click);
                    newMenuItem.Name = "New";
                    if (cmnTreeNode.StringNumber != -1)
                        newMenuItem.Enabled = false;
                    contextMenu.MenuItems.Add(newMenuItem);

                    MenuItem renameMenuItem = new MenuItem("Rename");
                    renameMenuItem.Click += new EventHandler(MenuItem_Click);
                    renameMenuItem.Name = "Rename";
                    if (cmnTreeNode.Nodes.Count > 0)
                        renameMenuItem.Enabled = false;
                    contextMenu.MenuItems.Add(renameMenuItem);

                    MenuItem deleteMenuItem = new MenuItem("Delete");
                    deleteMenuItem.Click += new EventHandler(MenuItem_Click);
                    deleteMenuItem.Name = "Delete";
                    if (cmnTreeNode.Nodes.Count > 0)
                        deleteMenuItem.Enabled = false;
                    contextMenu.MenuItems.Add(deleteMenuItem);

                    MenuItem exitMenuItem = new MenuItem("Exit");
                    contextMenu.MenuItems.Add(exitMenuItem);

                    contextMenu.Show(treeView1, treeView1.PointToClient(Cursor.Position));
                }
            }
        }

        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            CMN.CmnTreeNode addedCmnTreeNode = (CMN.CmnTreeNode)e.Node;

            // If label has been edited
            if (e.Label != null)
            {
                // Check if the entered string contains the parent branch name
                if (!e.Label.StartsWith(addedCmnTreeNode.Parent.Text))
                {
                    e.CancelEdit = true;
                    MessageBox.Show(string.Format("Invalid branch name.\nThe parent branch name \"{0}\" should not be removed.", addedCmnTreeNode.Parent.Text),
                       "Node Label Edit");

                    // Remove branch if the user remove characters from the parent with the "New" menu item
                    if (_renameOption == false)
                    {
                        // Remove cmnTreeNode from the parent childrens
                        CMN.CmnTreeNode cmnTreeNodeParent = (CMN.CmnTreeNode)e.Node.Parent;
                        cmnTreeNodeParent.childrens.RemoveAt(cmnTreeNodeParent.childrens.Count - 1);

                        // Remove node from treeView
                        e.Node.Remove();
                    }
                }
                else if (e.Label.Equals(addedCmnTreeNode.Parent.Text))
                {
                    e.CancelEdit = true;
                    MessageBox.Show(string.Format("Invalid tree node label.\nThe label should not be the same as the parent branch name \"{0}\".", addedCmnTreeNode.Parent.Text),
                        "Node Label Edit");
                }
                else
                {
                    // Update cmnTreeNodeName when editing is finished
                    e.Node.EndEdit(false);

                    addedCmnTreeNode.SetProperties(e.Label, e.Label, -1);
                    CMN.CmnTreeNode addedCmnTreeNodeParent = (CMN.CmnTreeNode)addedCmnTreeNode.Parent;

                    addedCmnTreeNode.Remove();

                    addTreeNodeBySorted(addedCmnTreeNode, addedCmnTreeNodeParent);

                    MergeNodes(addedCmnTreeNode, addedCmnTreeNode.Parent.Text, (CMN.CmnTreeNode)addedCmnTreeNode.Parent);

                    // Refresh DataGridView with updated cmnTreeNode name
                    StringGridEditor.AddToDataGridView();
                }

            }
            // If label has not been edited
            else
            {
                // If it's the rename option, don't show error
                if (_renameOption == true)
                {
                    e.Node.EndEdit(false);
                    _renameOption = false;
                }
                // Show error if the user didn't enter anything for the new branch name
                else if (_newOption == true)
                {
                    e.CancelEdit = true;
                    MessageBox.Show("Invalid tree node label.\nThe label cannot be blank.",
                       "Node Label Edit");

                    // Remove cmnTreeNode from the parent childrens
                    CMN.CmnTreeNode cmnTreeNodeParent = (CMN.CmnTreeNode)e.Node.Parent;
                    cmnTreeNodeParent.childrens.RemoveAt(cmnTreeNodeParent.childrens.Count - 1);

                    // Remove node from treeView
                    e.Node.Remove();

                    _newOption = false;
                }
            }

            treeView1.LabelEdit = false;
            treeView1.SelectedNode = null;
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
            else if (e.Control && e.KeyCode == Keys.S)
            {
                StringGridEditor.SaveFile();
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

            CMN.CmnTreeNode cmnTreeNode;

            switch (menuItem.Name)
            {
                case "New":
                    if (treeView1.SelectedNode is CMN.CmnTreeNode)
                    {
                        cmnTreeNode = (CMN.CmnTreeNode)treeView1.SelectedNode;
                        _newOption = true;
                        CMN.CmnTreeNode newCmnTreeNode = new CMN.CmnTreeNode();
                        newCmnTreeNode.SetProperties(cmnTreeNode.Text, cmnTreeNode.Text, -1);

                        // Add newCmnTreeNode to treeView1
                        cmnTreeNode.Nodes.Add(newCmnTreeNode);

                        // Begin editing of newCmnTreeNode name
                        treeView1.SelectedNode = newCmnTreeNode;
                        treeView1.LabelEdit = true;
                        treeView1.SelectedNode.BeginEdit();
                    }
                    break;
                case "Rename":
                    cmnTreeNode = (CMN.CmnTreeNode)treeView1.SelectedNode;
                    _renameOption = true;

                    if (treeView1.SelectedNode is CMN.CmnTreeNode)
                    {
                        if (cmnTreeNode.Nodes.Count > 0)
                        {
                            MessageBox.Show("Cannot rename node because of subranches", "Node Rename");
                        }
                        else
                        {
                            treeView1.LabelEdit = true;
                            treeView1.SelectedNode.BeginEdit();
                        }
                    }
                    break;
                case "Delete":
                    cmnTreeNode = (CMN.CmnTreeNode)treeView1.SelectedNode;
                    if (treeView1.SelectedNode is CMN.CmnTreeNode)
                    {
                        if (cmnTreeNode.Nodes.Count > 0)
                        {
                            MessageBox.Show("Cannot delete node because of subranches", "Node Delete");
                        }
                        else
                        {
                            var result = MessageBox.Show("Do you want to delete this branch ?", "Node Delete Confirmation", MessageBoxButtons.YesNo);
                            if (result == DialogResult.Yes)
                            {

                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
