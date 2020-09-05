using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RAPIS_FIMC
{
    public partial class AboutDialog : Form
    {
        public AboutDialog()
        {
            InitializeComponent();
            TextBoxInitialization();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void TextBoxInitialization()
        {
            TextBox.BorderStyle = BorderStyle.None;
            
        }

        private void Sunburst275Button_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://sunburst275.jimdofree.com/");
        }
    }
}
