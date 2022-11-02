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
        public string text;
        public TextForm(string text)
        {
            InitializeComponent();
            textBox1.Text = text;
        }
    }
}
