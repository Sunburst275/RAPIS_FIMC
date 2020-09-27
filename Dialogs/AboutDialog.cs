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
            string tmpTextBoxText = ("RAPIS FIMC\n" + "Version" + Program.Version.ToString() + "\n\n" + TextBox.Text);
            TextBox.Text = tmpTextBoxText;
        }

        private void Sunburst275Button_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Program.WebsiteLink);
        }
    }
}
