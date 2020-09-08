using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace RAPIS_FIMC.Dialogs
{

    public partial class LoadingDialog : Form
    {
        #region Events / Delegates
        public delegate void CancelLoading(object sender, EventArgs args);
        public event CancelLoading LoadingCancellingRequested;
        #endregion
        #region Constants
        private static readonly string ContinuationDots = "...";
        private static readonly int MaxFileNameLength = 60;
        #endregion
        #region Variables
        private readonly string fileName;
        Thread thread;
        #endregion
        public LoadingDialog(string fileName)
        {
            InitializeComponent();
            this.fileName = fileName;
            CustomInitialization();
        }
        private void CustomInitialization()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Loading file into program:");

            if (fileName.Length <= MaxFileNameLength)
            {
                sb.AppendLine(fileName);
            }
            else
            {
                // Too long for dialog, so truncate it and display dots
                var tmpStr = fileName.Substring(0, MaxFileNameLength - ContinuationDots.Length - 1);
                sb.AppendLine(tmpStr + " " + ContinuationDots);
            }


            sb.AppendLine("Please wait...");
            MessageLabel.Text = sb.ToString();
        }
        private void CancelButton_Click(object sender, EventArgs e)
        {
            RequestCancellingOfLoading();
            try
            {
                this.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }
        protected virtual void RequestCancellingOfLoading()
        {
            LoadingCancellingRequested?.Invoke(this, EventArgs.Empty);
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
                        this.thread.Interrupt();
                        this.thread = null;
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
