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
    public partial class TextForm : Form
    {
        public int RowIndex = -1;
        public int ColumnIndex = -1;
        public TextForm(string text, int rowIndex, int columnIndex)
        {
            InitializeComponent();
            textBox1.Text = text;
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            textBox1.ScrollBars = ScrollBars.Vertical;
        }
    }
}
