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
using System.Runtime.InteropServices;
using System.Threading;
using System.CodeDom;
using System.Runtime.CompilerServices;

namespace RAPIS_FIMC
{
    public partial class MainForm : Form
    {
        #region Extra TODO
        // + Commandline starting
        //      + Normal process
        //      + Help-MessageBox
        // + Recognize non-ASCII characters!
        // - Multiline select:
        //      + Manual select:
        //          + Calculate column
        //      + UpDownBox select:
        //          + Calculate columns
        //      ? Capture mouse release for better performance on calculating "IsValidSelection()"
        // ? Check max/min length of lines
        // + Row/Column labels in Designer
        // + Writing file in extra thread
        //      + Please wait dialog
        // - Check whether writing the file was successful or not
        // + SaveFileDialog()
        // x Load in seperate thread
        //      - Dialog
        // + Save in seperate thread
        // + About/Help
        //      + Write, that only one-line-selection is possible
        #endregion
        #region Constants
        // Messages
        const string SelectFileMsg = "Please select a file ...";
        const string SelectValidFileMsg = "Please select a valid file type ...";
        const string NoFileSelectedMsg = "No file selected ...";
        const string UnsupportedFileTypeMsg = "Unsupported file type!";

        // Other
        const int MaxCurrentlyOpenedLabelLength = 72;
        const int MaxCurrentlyOpenedDialogLabelLength = 100;
        #endregion
        #region Events / Delegates
        public delegate void OnProcessingDialog_Cancel_Delegate();
        #endregion
        #region Variables
        private readonly string[] cmdLineArgs;

        private FileInfo file;
        private int from = 0;
        private int to = 0;
        private List<LineBounds> lB;
        private LineBounds currentLine;

        private ProcessingDialog pd;

        private Thread writingThread;
        #endregion
        #region Constructors
        public MainForm(string[] cmdLineArgs)
        {
            InitializeComponent();
            Initialization();

            this.cmdLineArgs = cmdLineArgs;
            ProcessCmdLineArgs();
        }
        private void Initialization()
        {
            FileContentBox.SelectionChanged += FileContentBox_SelectionChanged;
            FromNumericUpDown.Maximum = int.MaxValue;
            FromNumericUpDown.Minimum = 0;
            ToNumericUpDown.Maximum = int.MaxValue;
            ToNumericUpDown.Minimum = 0;
            currentLine = new LineBounds();
            pd = new ProcessingDialog();

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
                    if (pathAndName == string.Empty)
                    {
                        file.SetPathAndName(string.Empty);
                    }
                    else
                    {
                        file.SetPathAndName(pathAndName);
                    }
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

            // Set up GUI for "loading"
            ChangeGuiLockingState(GuiLockingState.Lock);
            ShowLoadingDialog(file.GetName());

            List<string> content;
            try
            {
                content = LoadFile();
            }
            catch (FileReadException ex)
            {
                if (ex.fileReadExceptionState == FileReadException.FileReadExceptionState.Empty)
                {
                    MessageBox.Show("The specified file was empty...", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("An error occured during file reading:\n" + ex.InnerException.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                content = new List<string>();
                content.Add("");
            }

            file.SetContent(content);
            DisplayFileContentsInTextBox(file.GetContent());
            PreCalcSelectionBounds();
            SetCurrentlyOpenedLabel(fileName);
        }
        private void StartButton_Click(object sender, EventArgs e)
        {
            if (!file.IsValid())
            {
                MessageBox.Show("File is not valid!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var fromAndTo = CalculateSelectedSection();
            int realFrom = fromAndTo.Item1;
            int realTo = fromAndTo.Item2;

            ////DBG: Just to not have to save to a file all the time. This way its easier to get the algorithm working.
            //RemoveColumSpanInContent(file.GetContent(), realFrom, realTo);
            //return;

            using (SaveFileDialog svd = new SaveFileDialog())
            {
                var pathAndName = file.GetPathAndName();
                if (pathAndName == string.Empty)
                {
                    svd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
                }
                else
                {
                    svd.InitialDirectory = FileInfo.ExtractFilePath(pathAndName);
                }
                svd.Filter = "Input file type " + "(*." + file.GetFileTypeString() + ")|*." + file.GetFileTypeString();
                svd.FilterIndex = 1;
                svd.RestoreDirectory = true;
                svd.CheckPathExists = true;

                bool errorOccured = false;
                DialogResult dr = svd.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    // TODO: Also seems to cripple here...
                    // Start new thread and show MessageBox "done" when done
                    ChangeGuiLockingState(GuiLockingState.Lock);
                    ShowProcessingDialog();
                    try
                    {
                        writingThread = new Thread(() =>
                        {
                            WriteFile(RemoveColumSpanInContent(file.GetContent(), realFrom, realTo), svd.FileName);
                        });
                        writingThread.Start();
                    }
                    catch (Exception)
                    {
                        errorOccured = true;
                    }

                    CloseProcessingDialog();
                    ChangeGuiLockingState(GuiLockingState.Release);
                    if (errorOccured)
                    {
                        ShowProcessingResult(ProcessingDialogResult.Failed);
                    }
                    else
                    {
                        ShowProcessingResult(ProcessingDialogResult.Success);
                    }
                    return;
                }
            }
        }
        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
        private void AboutButton_Click(object sender, EventArgs e)
        {
            var AboutDialog = new AboutDialog();
            AboutDialog.ShowDialog();
        }
        private void ToNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDownBoxChanged();
        }
        private void FromNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDownBoxChanged();
        }
        private void OnProcessingDialog_Cancel(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate { OnProcessingDialog_Cancel(this, EventArgs.Empty); });
            }
            else
            {
                // Abort thread which is used to write file
                try
                {
                    if (writingThread != null)
                    {
                        writingThread.Interrupt();
                        writingThread = null;
                    }
                }
                catch (ThreadAbortException)
                {
                    // Ignore
                }
                CloseProcessingDialog();
                ChangeGuiLockingState(GuiLockingState.Release);
                ShowProcessingResult(ProcessingDialogResult.Cancelled);
            }
        }
        #endregion
        #region Read/Write file & processing
        private List<string> LoadFile()
        {
            List<string> stringyContent = new List<string>();
            try
            {
                using (StreamReader sR = new StreamReader(new FileStream(file.GetPathAndName(), FileMode.Open)))
                {
                    while (!sR.EndOfStream)
                        stringyContent.Add(sR.ReadLine());
                }
            }
            catch (Exception ex)
            {
                throw new FileReadException(FileReadException.FileReadExceptionState.Other, ex);
            }
            if (stringyContent.Count <= 0)
            {
                throw new FileReadException(FileReadException.FileReadExceptionState.Empty);
            }

            // LoadingDialog gets closed in DisplayFileContentsInTextBox(...)
            return stringyContent;
        }
        private void WriteFile(List<string> content, string filePath)
        {
            // TODO: Inform youself: How to best handle problems of file writing/reading
            var errorOccured = false;
            if (string.IsNullOrEmpty(filePath) || content == null || content.Count <= 0)
            {
                // TODO: Error messages
                return;
            }
            using (StreamWriter sW = new StreamWriter(new FileStream(filePath, FileMode.OpenOrCreate), Encoding.UTF8))
            {
                //Console.WriteLine("filePath = " + filePath);

                // TODO: Content is missing the line 08 when it comes here. Check in "RemoveColumSpanInContent(...)"!
                // TODO: StreamReader is reading 1 more line than actually should be in the file. Why is there an extra line written?

                try
                {
                    for (int index = 0; index < content.Count; index++)
                    {
                        sW.WriteLine(content.ElementAt(index));
                    }
                }
                catch (Exception)
                {
                    // TODO: Catch errors, show "failed", etc.
                    throw;
                }
            }

            // TODO: Verify file content and check whether file writing was done successfully
            List<string> testContent = new List<string>();
            using (StreamReader sR = new StreamReader(new FileStream(filePath, FileMode.Open), Encoding.UTF8))
            {
                try
                {
                    while (!sR.EndOfStream)
                        testContent.Add(sR.ReadLine());
                }
                catch (Exception)
                {
                    // TODO: Catch errors, show "failed", etc.
                    throw;
                }
            }
            // If read file is empty
            if (testContent.Count <= 0)
            {
                errorOccured = true;
            }
            // Check whether every line in file corresponds to the same line in content
            for(int i = 0; i < testContent.Count; i++)
            {
                if (testContent[i] != content[i])
                {
                    errorOccured = true;
                    break;
                }
            }

            if (errorOccured)
                Console.WriteLine("errorOccured!");



        }
        private List<string> RemoveColumSpanInContent(List<string> content, int from, int to)
        {
            List<string> contentToEdit = content;
            List<string> editedContent = contentToEdit;

            // DBG: Only to show what was in there before
            {
                Console.WriteLine("=================================================");
                Console.Write("Original:\n");
                for (int item = 0; item < contentToEdit.Count; item++)
                {
                    Console.WriteLine(contentToEdit[item]);
                }
                Console.WriteLine("- - - - - - - - - - - - - - - - - - - - - - - - - ");
                Console.Write("Edited:\n");
            }

            // Removing columns in lines according to specified "from" and "to"
            for (int itemIndex = 0; itemIndex < contentToEdit.Count; itemIndex++)
            {
                // TODO: Maybe try to change this. Maybe the problem occurs here (file writing/reading, content etc.)
                var varLength = contentToEdit[itemIndex].Length;// - 1;
                if (varLength < from)
                {
                    // Line is too short for the removing of these columns. -> Do nothing
                    editedContent[itemIndex] = contentToEdit[itemIndex];
                }
                else if (varLength >= from && varLength <= to)
                {
                    // Line is too short for the removing from "from" to "to", only "from" is in bounds. 
                    // -> Delete from "from" to end of the line (varLength)
                    editedContent[itemIndex] = contentToEdit[itemIndex].Remove(from, varLength - from);
                }
                else
                {
                    // Line is long enough to have char's removed in between "from" and "to".
                    editedContent[itemIndex] = contentToEdit[itemIndex].Remove(from, to - from);
                }

                // DBG: Only to show the result of the operation
                Console.WriteLine(editedContent[itemIndex]);
            }

            return editedContent;
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
            // TODO: Make selection smoother. Whats wrong here? Why is it that cripply?
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
            int end;
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

            var res = preceedingLineBounds.Equals(trailingLineBounds);
            if (res)
                currentLine = preceedingLineBounds;
            return res;
        }
        /// <summary>Use this to calculate the actual "from" and "to" values independent from the line and length of the text from the RichTextBox</summary>
        /// <remarks>Dont use this to change the SpinnerBoxes' values, unless you want to make the code 1000x more complex.</remarks>
        /// <returns>(realFrom, realTo)</returns>
        private Tuple<int, int> CalculateSelectedSection()
        {
            int realFrom, realTo;
            int currentLineIndex = 0;

            // Get current line index
            for (int i = 0; i < lB.Count; i++)
            {
                if (lB[i].Equals(currentLine))
                {
                    currentLineIndex = i;
                    break;
                }
            }
            if (currentLineIndex == 0)
            {
                return Tuple.Create(from, to);
            }

            // Calculate realFrom and realTo
            int overallLength = from;
            for (int i = 0; i < currentLineIndex; i++)
            {
                overallLength -= (lB[i].content.Length + 1);
            }
            realFrom = overallLength;
            realTo = (to - from) + realFrom;
            return Tuple.Create(realFrom, realTo);
        }
        private void DisplayFileContentsInTextBox(List<string> readLines)
        {
            if (readLines.Count <= 0 || readLines == null)
            {
                MessageBox.Show("Loaded file is empty or another error occured.\nPlease try again or choose another file.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            StringBuilder sb = new StringBuilder(readLines.Count - 1);
            for (int i = 0; (i < readLines.Count); i++)
                sb.AppendLine(readLines[i]);
            if (sb.Length > 0)
                FileContentBox.Text = sb.ToString();
            else
                FileContentBox.Text = "";
            ChangeGuiLockingState(GuiLockingState.Release);
        }
        private void ShowProcessingDialog()
        {
            pd = new ProcessingDialog();
            pd.ProcessCancellingRequested += OnProcessingDialog_Cancel;
            pd.ShowDialog();
        }
        private void CloseProcessingDialog()
        {
            if (pd != null)
            {
                pd.ProcessCancellingRequested -= OnProcessingDialog_Cancel;
                pd.Close();
                pd = null;
            }
        }
        private void ShowLoadingDialog(string fileName)
        {
            // Building message
            StringBuilder sb = new StringBuilder();
            {
                sb.AppendLine("Loading file:");
                if (fileName.Length <= MaxCurrentlyOpenedDialogLabelLength)
                {
                    sb.AppendLine(fileName);
                }
                else
                {
                    string spacedotdotdot = " ...";
                    int removeFrom = MaxCurrentlyOpenedDialogLabelLength - spacedotdotdot.Length;
                    int removeCount = fileName.Length - removeFrom;
                    string castractedFileName = fileName.Remove(removeFrom, removeCount);
                    sb.AppendLine(castractedFileName + spacedotdotdot);
                }
                sb.AppendLine("Please wait...");
            }
            MessageBox.Show(sb.ToString(), "Loading file...", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void ChangeGuiLockingState(GuiLockingState gls)
        {
            // Determine whether GUI components should be enabled or disabled
            bool enabled = true; // Default: Enabled/Unlocked GUI
            switch (gls)
            {
                case (GuiLockingState.Release):
                    enabled = true;
                    break;
                case (GuiLockingState.Lock):
                    enabled = false;
                    break;
            }

            if (this.InvokeRequired)
            {
                //this.Invoke((MethodInvoker)delegate { ChangeGuiLockingState(gls); });
                this.Invoke(new Action(() => ChangeGuiLockingState(gls)));
            }
            else
            {
                // Disable controls
                foreach (Control cntrl in this.Controls)
                {
                    cntrl.Enabled = enabled;
                }
            }
        }
        private void ShowProcessingResult(ProcessingDialogResult processingDialogResult)
        {
            // Show msgbox when "done"
            switch (processingDialogResult)
            {
                case (ProcessingDialogResult.Success):
                    MessageBox.Show("Removal of specified columns is done.\nYou may close the program now\nor continue.",
                                    Program.ProgramHeader + "Processing done",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                    break;
                case (ProcessingDialogResult.Cancelled):
                    MessageBox.Show("Removal of specified columns was cancelled.\nYou may close the program now\nor continue.",
                        Program.ProgramHeader + "Processing cancelled",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    break;
                case (ProcessingDialogResult.Failed):
                    MessageBox.Show("Removal of specified columns failed.\nYou may close the program now\nor continue.",
                        Program.ProgramHeader + "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    break;
            }
        }
        private void ProcessCmdLineArgs()
        {
            int index = Program.CmdLineArgumentOffset;

            // Check whether arguments are more than one
            if (cmdLineArgs.Length <= index)
                return;

            // Process control variables
            List<Exception> exceptions = new List<Exception>();
            bool errorOccured = false;
            int exitCode = 0;

            // Lock GUI because its in cmdline mode
            ChangeGuiLockingState(GuiLockingState.Lock);

            // Load arguments into meaningful var's
            var source = cmdLineArgs[index++];
            var from = cmdLineArgs[index++];
            var to = cmdLineArgs[index++];
            var destination = cmdLineArgs[index++];

            // Convert int's
            if (!int.TryParse(from, out int iFrom))
            {
                exceptions.Add(new Exception("Couldn't parse \"from\"-integer"));
                errorOccured = true;
            }
            if (!int.TryParse(to, out int iTo))
            {
                exceptions.Add(new Exception("Couldn't parse \"to\"-integer"));
                errorOccured = true;
            }

            // Load file
            file.SetPathAndName(source);
            List<string> content = new List<string>();
            try
            {
                content = LoadFile();
            }
            catch (Exception e)
            {
                exceptions.Add(e);
                errorOccured = true;
            }

            // Remove columns
            content = RemoveColumSpanInContent(content, iFrom, iTo);

            // Write file
            try
            {
                WriteFile(content, destination);
            }
            catch (Exception e)
            {
                exceptions.Add(e);
                errorOccured = true;
            }

            // If necessary, show exceptions and errors
            if (errorOccured)
            {
                // Build message
                StringBuilder sb = new StringBuilder();
                {
                    sb.AppendLine("Errors occured during the process:");
                    sb.AppendLine("\n[Start List of errors]");
                    if (exceptions.Count == 0)
                    {
                        sb.AppendLine("\n...");
                    }
                    else
                    {
                        foreach (Exception ex in exceptions)
                        {
                            sb.AppendLine("\n" + ex.Message);
                        }
                    }
                    sb.AppendLine("\n[End List of error]\n");
                    sb.AppendLine("Please check your input- and output-files, whether you used this program as described, and if the input parameters were correct:");
                    sb.AppendLine("<Source> <From> <To> <Destination>");
                    sb.AppendLine("");
                }
                MessageBox.Show(sb.ToString(), "Errors occured during program execution!", MessageBoxButtons.OK, MessageBoxIcon.Error);

                exitCode++;
            }
            else
            {
                MessageBox.Show("Program executed without errors.", Program.ProgramHeader + "Processing done.", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // Close program
            Environment.Exit(exitCode);
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
        public enum ProcessingDialogResult { Success, Cancelled, Failed };
        public enum GuiLockingState { Lock, Release }
        #endregion

    }
}
