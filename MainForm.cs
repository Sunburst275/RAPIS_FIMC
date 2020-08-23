using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Diagnostics;

namespace Multiliner
{
    public partial class MainForm : Form
    {
        #region Extra TODO
        // - Multiline select:
        //      - Manual select:
        //          - Calculate column
        //          - Write into 
        //      - UpDownBox select:
        //          - Calculate columns
        // - Check max/min length of lines
        // + Row/Column labels in Designer
        // - 
        // - ...
        #endregion

        #region Constants
        // Messages
        const string SelectFileMsg = "Please select a file ...";
        const string SelectValidFileMsg = "Please select a valid file type ...";
        const string NoFileSelectedMsg = "No file selected ...";
        const string UnsupportedFileTypeMsg = "Unsupported file type!";

        // Other
        const int MaxCurrentlyOpenedLabelLength = 70;
        #endregion

        #region Variables
        FileInfo file;
        int from = 0;
        int to = 0;
        #endregion

        #region Constructors
        public MainForm()
        {
            InitializeComponent();
            Initialization();
        }
        private void Initialization()
        {
            FileContentBox.SelectionChanged += FileContentBox_SelectionChanged;
            FromNumericUpDown.Maximum = int.MaxValue;
            FromNumericUpDown.Minimum = int.MinValue;
            ToNumericUpDown.Maximum = int.MaxValue;
            ToNumericUpDown.Minimum = int.MinValue;

            file = new FileInfo();

            // GUI Initialization
            FileTextBox.Text = SelectFileMsg;
            FileContentBox.Text = NoFileSelectedMsg;
            SetCurrentlyOpenedLabel("None");
        }
        #endregion

        #region UI Actions
        private void MainForm_Load(object sender, EventArgs e)
        {

        }
        private void FileContentBox_SelectionChanged(object sender, EventArgs e)
        {
            // Unsubscribe NumericUpDowns
            FromNumericUpDown.ValueChanged -= FromNumericUpDown_ValueChanged;
            ToNumericUpDown.ValueChanged -= ToNumericUpDown_ValueChanged;

            // Change values acccording to selection
            FromNumericUpDown.Value = from = (int)FileContentBox.SelectionStart;
            ToNumericUpDown.Value = to = (int)(FileContentBox.SelectionStart + FileContentBox.SelectionLength);

            // Resubscribe NumericUpDowns
            FromNumericUpDown.ValueChanged += FromNumericUpDown_ValueChanged;
            ToNumericUpDown.ValueChanged += ToNumericUpDown_ValueChanged;
        }
        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                var pathAndName = file.GetPathAndName();
                if (pathAndName == string.Empty)
                    ofd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
                else
                    ofd.InitialDirectory = FileInfo.ExtractFilePath(pathAndName);
                ofd.Filter = "Supported files (*.tsv;*.txt)|*.tsv;*.txt|Tab seperated values (*.tsv)|*.tsv|Text files (*.txt)|*.txt|All files (*.*)|*.*";
                ofd.FilterIndex = 1;
                ofd.RestoreDirectory = true;
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                ofd.Multiselect = false;

                DialogResult dr = ofd.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    FileTextBox.Text = ofd.FileName;
                    file.SetPathAndName(ofd.FileName);
                }
                else
                {
                    file.SetPathAndName(string.Empty);
                }
            }
        }
        private void LoadButton_Click(object sender, EventArgs e)
        {
            var pathAndName = file.GetPathAndName();
            // Check if file is selected
            if (pathAndName == string.Empty)
            {
                var result = MessageBox.Show(NoFileSelectedMsg, SelectFileMsg, MessageBoxButtons.RetryCancel, MessageBoxIcon.Information);
                if (result == DialogResult.Retry)
                {
                    BrowseButton_Click(this, EventArgs.Empty);
                }
                if (file.GetPathAndName() == string.Empty)
                {
                    SetCurrentlyOpenedLabel("None");
                }
                return;
            }
            // Check which kind of file it is
            string fileName = FileInfo.ExtractFileName(pathAndName);
            FileInfo.FileType fileType = FileInfo.ExtractFileType(fileName);

            //Console.WriteLine("FileName = " + fileName);
            //Console.WriteLine("FileExtension = " + fileType);
            //Console.WriteLine("FilePath = " + GetFilePath(selectedFilePathAndName));

            if (fileType == FileInfo.FileType.other)
            {
                MessageBox.Show(UnsupportedFileTypeMsg, SelectValidFileMsg, MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetCurrentlyOpenedLabel("None");
                return;
            }

            // TODO: Think about "current row/column", too.
            SetCurrentlyOpenedLabel(fileName);

            file.SetContent(LoadFile());
            DisplayFile(file.GetContent());
        }
        private void StartButton_Click(object sender, EventArgs e)
        {
            List<string> contentToEdit = file.GetContent();

            Console.WriteLine("Original:\n");
            for (int item = 0; item < contentToEdit.Count; item++)
            {
                Console.WriteLine(contentToEdit[item]);
            }

            Console.WriteLine();
            Console.WriteLine("Edited:\n");

            for (int item = 0; item < contentToEdit.Count; item++)
            {
                contentToEdit[item].Remove(from, to - from);
                Console.WriteLine(contentToEdit[item]);
            }

            WriteFile(contentToEdit);
        }
        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
        private void ToNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDownBoxChanged();
        }
        private void FromNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDownBoxChanged();
        }
        #endregion

        #region Load file / Write to box
        private List<string> LoadFile()
        {
            List<string> stringyContent = new List<string>();
            using (StreamReader sR = new StreamReader(new FileStream(file.GetPathAndName(), FileMode.Open)))
            {
                try
                {
                    while (!sR.EndOfStream)
                        stringyContent.Add(sR.ReadLine());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occured during file reading:\n" + ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    stringyContent.Clear();
                    return null;
                }
            }
            if (stringyContent.Count <= 0)
            {
                MessageBox.Show("The specified file was empty...", "Attention!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                stringyContent.Add("");
            }
            return stringyContent;
        }
        private void WriteFile(List<string> content)
        {

        }
        private void DisplayFile(List<string> readLines)
        {
            if (readLines.Count <= 0 || readLines == null)
            {
                MessageBox.Show("Loaded file is empty or another error occured.\nPlease try again or choose another file.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            StringBuilder sb = new StringBuilder(readLines.Count - 1);

            for (int i = 0; i < readLines.Count; i++)
                sb.AppendLine(readLines[i]);

            FileContentBox.Text = sb.ToString();
        }
        #endregion

        #region Helper
        private void SetCurrentlyOpenedLabel(string msg)
        {
            string text = "Currently opened:    " + msg;
            if (text.Length <= MaxCurrentlyOpenedLabelLength)
            {
                CurrentlyOpenedLabel.Text = text;
            }
            else if (msg.Length <= MaxCurrentlyOpenedLabelLength)
            {
                CurrentlyOpenedLabel.Text = msg;
            }
            else
            {
                string appendix = " ...";
                CurrentlyOpenedLabel.Text = (msg.Substring(0, MaxCurrentlyOpenedLabelLength - appendix.Length) + appendix);
            }
        }
        private void NumericUpDownBoxChanged()
        {
            // TODO: Show cursor position at fromVal
            FileContentBox.HideSelection = false;
            FileContentBox.ScrollToCaret();

            int fromVal = (int)FromNumericUpDown.Value; int toVal = (int)ToNumericUpDown.Value;

            if (fromVal > toVal)
            {
                ToNumericUpDown.Value = toVal = fromVal;
            }
            // TODO: Max line length check, too

            // Selection calculation and setting
            FileContentBox.SelectionStart = fromVal;
            FileContentBox.SelectionLength = toVal - fromVal;

            // Set global from
            from = fromVal;
            to = toVal;
        }
        private void CalculateSelectedSection()
        {

        }
        #endregion
    }
}
