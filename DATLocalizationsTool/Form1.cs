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

namespace DATLocalizationsTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

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
                dat = new DAT(filepath);
                /*ListString = dat.Strings;

                int index = 0;
                foreach (string s in ListString)
                {
                    dataGridView1.Rows.Add(index, "", s);

                    index++;
                }*/
            }
            catch (Exception ex)
            {

            }
        }
    }
}
