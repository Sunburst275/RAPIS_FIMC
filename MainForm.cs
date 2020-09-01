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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RAPIS_FIMC
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
        // - Writing file in extra thread
        //      - Please wait dialog
        // + SaveFileDialog()
        // - Load in seperate thread
        // - Save in seperate thread
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
        List<LineBounds> lB;
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
            FromNumericUpDown.Minimum = 0;
            ToNumericUpDown.Maximum = int.MaxValue;
            ToNumericUpDown.Minimum = 0;

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
            // TODO: Maybe only execute this when MouseEvent.Released is executed

            // Unsubscribe NumericUpDowns
            FromNumericUpDown.ValueChanged -= FromNumericUpDown_ValueChanged;
            ToNumericUpDown.ValueChanged -= ToNumericUpDown_ValueChanged;

            var tmpFrom = (int)FileContentBox.SelectionStart;
            var tmpTo = (int)(FileContentBox.SelectionStart + FileContentBox.SelectionLength);

            // Change values acccording to selection
            if (lB != null)
            {
                if (IsSelectionValid(tmpFrom, tmpTo))
                {
                    FromNumericUpDown.Value = from = tmpFrom;
                    ToNumericUpDown.Value = to = tmpTo;
                }
                else
                {
                    FileContentBox.SelectionStart = FileContentBox.SelectionStart = from;
                    FileContentBox.SelectionLength = FileContentBox.SelectionLength = to - from;
                }
            }

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

                // Building the filter(s)
                StringBuilder sb = new StringBuilder();
                {
                    sb.Append("Supported files(");
                    foreach (KeyValuePair<FileInfo.FileType, string> ft in FileInfo.supportedFileTypes)
                    {
                        sb.Append("*.");
                        sb.Append(ft.Value);
                        sb.Append(";");
                    }
                    sb.Append(")|");
                    foreach (KeyValuePair<FileInfo.FileType, string> ft in FileInfo.supportedFileTypes)
                    {
                        sb.Append("*.");
                        sb.Append(ft.Value);
                        sb.Append(";");
                    }
                    sb.Append("|");

                    sb.Append("Delimited text files (*.tsv;*.csv)");
                    sb.Append("|");
                    sb.Append("*.tsv;*.csv");
                    sb.Append("|");

                    sb.Append("Text files (*.txt;*.log)");
                    sb.Append("|");
                    sb.Append("*.txt;*.log");
                    sb.Append("|");

                    sb.Append("HTML files (*.htm;*.html)");
                    sb.Append("|");
                    sb.Append("*.htm;*.html");
                    sb.Append("|");

                    /*
                    sb.Append("Microsoft files (*.docx;*.xlsx)");
                    sb.Append("|");
                    sb.Append("*.docx;*.xlsx");
                    sb.Append("|");
                    */

                    sb.Append("All files (*.*)");
                    sb.Append("|");
                    sb.Append("*.*");
                }
                ofd.Filter = sb.ToString();
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
            
            if (fileType == FileInfo.FileType.other)
            {
                MessageBox.Show(UnsupportedFileTypeMsg, SelectValidFileMsg, MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetCurrentlyOpenedLabel("None");
                return;
            }

            SetCurrentlyOpenedLabel(fileName);
            file.SetContent(LoadFile());
            DisplayFileContentsInTextBox(file.GetContent());
            PreCalcSelectionBounds();
        }
        private void StartButton_Click(object sender, EventArgs e)
        {
            if (!file.IsValid())
            {
                MessageBox.Show("File is not valid!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SaveFileDialog svd = new SaveFileDialog())
            {
                var pathAndName = file.GetPathAndName();
                if (pathAndName == string.Empty)
                    svd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
                else
                    svd.InitialDirectory = FileInfo.ExtractFilePath(pathAndName);
                svd.Filter = "Input file type " + "(*." + file.GetFileTypeString() + ")|*." + file.GetFileTypeString();
                svd.FilterIndex = 1;
                svd.RestoreDirectory = true;
                svd.CheckPathExists = true;

                DialogResult dr = svd.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    // Start new thread and show MessageBox "done" when done
                    WriteFile(RemoveColumSpanInContent(file.GetContent(), from, to), svd.FileName);
                    return;
                }
            }
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
        #region Read/Write file
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
                MessageBox.Show("The specified file was empty...", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                stringyContent.Add("");
            }
            return stringyContent;
        }
        private void WriteFile(List<string> content, string filePath)
        {
            var errorOccured = false;
            if (string.IsNullOrEmpty(filePath) || content == null || content.Count <= 0)
            {
                // TODO: Fehlermeldung
                return;
            }
            using (StreamWriter sW = new StreamWriter(new FileStream(filePath, FileMode.OpenOrCreate), Encoding.UTF8))
            {
                Console.WriteLine("filePath = " + filePath);

                try
                {
                    for (int index = 0; index < content.Count; index++)
                    {
                        sW.WriteLine(content.ElementAt(index));
                    }
                }
                catch (Exception)
                {
                    // TODO: Errors abfangen, "failed" anzeigen, etc.
                    throw;
                }
            }

            MessageBox.Show("Done.");
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

            if (fromVal < 0 || toVal < 0)
                return;

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
        private void PreCalcSelectionBounds()
        {
            var content = FileContentBox.Text;
            var tmp = content.Split(Environment.NewLine.ToCharArray());
            string[] splittedContent = tmp.Take(tmp.Count() - 1).ToArray();
            lB = new List<LineBounds>();

            int start = 0;
            int end = 0;
            int index = 0;
            for (int i = 0; i < splittedContent.Length; i++)
            {
                var s = splittedContent[i];

                end = start + s.Length;

                LineBounds tmpLineBounds = new LineBounds(index++, start, end, s);

                lB.Add(tmpLineBounds);

                start = end + 1;
            }
        }
        private bool IsSelectionValid(int from, int to)
        {
            // TODO: Selection is 1-Indexed, my stuff is 0-Indexed

            var content = FileContentBox.Text;
            LineBounds preceedingLineBounds = new LineBounds();
            LineBounds trailingLineBounds = new LineBounds();

            int current = 0;
            // Get the line in which the "from" is in
            for (; current < lB.Count; current++)
            {
                if (from >= lB[current].start && from <= lB[current].end)
                {
                    preceedingLineBounds = lB[current];
                    break;
                }
            }
            // Get the line in which the "to" is in
            if (to >= lB[current].start && to <= lB[current].end)
            {
                trailingLineBounds = lB[current];
            }
            else
            {
                while (current < lB.Count)
                {
                    if (to >= lB[current].start && to <= lB[current].end)
                    {
                        trailingLineBounds = lB[current];
                        break;
                    }
                    else
                    {
                        current++;
                    }
                }
            }

            Debug.WriteLine("preceedingLineBounds.Equals(trailingLineBounds) = " + preceedingLineBounds.Equals(trailingLineBounds));
            return preceedingLineBounds.Equals(trailingLineBounds);
        }

        private void CalculateSelectedSection()
        {
            // TODO: Calculate the selected "from where to where" and mark it spanning all rows 
        }
        private List<string> RemoveColumSpanInContent(List<string> content, int from, int to)
        {
            // TODO: Calculate where to remove which column/s in which line
            List<string> contentToEdit = file.GetContent();
            List<string> editedContent = contentToEdit;

            Console.WriteLine("Original:\n");
            for (int item = 0; item < contentToEdit.Count; item++)
            {
                Console.WriteLine(contentToEdit[item]);
            }

            Console.WriteLine();
            Console.WriteLine("Edited:\n");

            for (int item = 0; item < contentToEdit.Count; item++)
            {
                editedContent[item] = contentToEdit[item].Remove(from, to - from);
                Console.WriteLine(editedContent[item]);
            }

            return editedContent;
        }
        private void DisplayFileContentsInTextBox(List<string> readLines)
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
        #region Helping structures
        public struct LineBounds
        {
            public int index;
            public int start;
            public int end;
            public string content;

            public LineBounds(int index, int start, int end, string content)
            {
                this.index = index;
                this.start = start;
                this.end = end;
                this.content = content;
            }
            public override bool Equals(object obj)
            {
                if (!(obj is LineBounds))
                    return false;

                var test = (LineBounds)obj;

                return
                    (
                    this.start == test.start &&
                    this.end == test.end &&
                    this.content.Equals(test.content)
                    );
            }

        }
        #endregion
    }
}
