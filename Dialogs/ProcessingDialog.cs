using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace RAPIS_FIMC
{
    public partial class ProcessingDialog : Form
    {
        public delegate void CancelProcessing(object sender, EventArgs args);
        public event CancelProcessing ProcessCancellingRequested;
        Thread thread;

        public ProcessingDialog()
        {
            InitializeComponent();
            CustomInitialization();
        }
        private void CustomInitialization()
        {
            MessageLabel.Text = "Processing the removal of specified columns.\nPlease wait...";
        }
        private void CancelButton_Click(object sender, EventArgs e)
        {
            RequestCancellingOfProcessing();
            // Maybe thread cant be destroyed
            try
            {
                this.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }
        protected virtual void RequestCancellingOfProcessing()
        {
            ProcessCancellingRequested?.Invoke(this, EventArgs.Empty);
        }

        // This method hides the base ShowDialog and starts new thread to show the dialog in.
        public new System.Windows.Forms.DialogResult ShowDialog()
        {
            DialogResult result = DialogResult.Cancel;
            try
            {
                thread = new Thread(() => { result = base.ShowDialog(); });
                thread.Start();
            }
            catch (Exception)
            {
                // Ignore exception
            }

            return result;
        }
        public new void Close()
        {
            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke((MethodInvoker)delegate { Close(); });
                }
                catch (ThreadAbortException)
                {
                    // Ignore
                }
            }
            else
            {

                if (thread != null)
                {
                    try
                    {
                        thread.Abort();
                        thread = null;
                    }
                    catch (ThreadAbortException)
                    {
                        // Ignore
                        //throw;
                    }
                }

                base.Close();
            }
        }
    }
}
