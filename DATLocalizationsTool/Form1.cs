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
        public Form1()
        {
            InitializeComponent();
        }

        private List<string> ValidStrings = new List<string> {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "Cmn"
        };

        DAT dat;
        List<string> ListString;
        

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

        private void LoadFile(string filepath)
        {
            try
            {
                if (ValidStrings.Contains(Path.GetFileNameWithoutExtension(filepath)))
                {
                    if (Path.GetFileNameWithoutExtension(filepath) != "Cmn")
                    {
                        dat = new DAT(filepath);
                        AddToDataGridView();
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }

        private void AddToDataGridView()
        {
            ListString = dat.Strings;

            int index = 0;
            foreach (string s in ListString)
            {
                dataGridView1.Rows.Add(index, null, s);

                index++;
            }
            dataGridView1.AutoResizeRows();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null && !dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly)
            {
                string cellText = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                var textForm = new TextForm(cellText);
                textForm.Show();
            }
        }
    }
}
