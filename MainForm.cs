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
        // x Writing file in extra thread
        //      x Please wait dialog
        // + Check whether writing the file was successful or not
        // + SaveFileDialog()
        // x Load in seperate thread
        //      - Dialog
        // + Save in seperate thread
        // + About/Help
        //      + Write, that only one-line-selection is possible
        // - Add version number everywhere, where it makes sense
        // - Set icon(s)
        // ------------------------------------------------------------------
        // + Comments and description of methods (seperated by regions)
        //      + Constants
        //      + Variables
        //      + Constructors
        //      + UI Actions
        //      + Read/Write file & processing
        //      + Helper
        //      + Helping structures
        //      + FileInfo
        #endregion
        #region Constants
        // Messages
        const string SelectFileMsg = "Please select a file ...";
        const string SelectValidFileMsg = "Please select a valid file type ...";
        const string NoFileSelectedMsg = "No file selected ...";
        const string UnsupportedFileTypeMsg = "Unsupported file type!";

        // Other
        const int MaxCurrentlyOpenedLabelLength = 72;           // Char limit of opened file displayed in GUI mainform -> Above this will be truncatedly displayed
        const int MaxCurrentlyOpenedDialogLabelLength = 100;    // Used for the "please wait" dialog. Same as above
        #endregion
        #region Events / Delegates
        public delegate void OnProcessingDialog_Cancel_Delegate();
        #endregion
        #region Variables
        private readonly string[] cmdLineArgs;

        private FileInfo file;          // Info and attributs of currently opened file
        private int from = 0;           // From which column should be deleted
        private int to = 0;             // Up to which column should be deleted
        private List<LineBounds> lB;    // Line bounds for currently opened file
        private LineBounds currentLine; // Current line that is being tested

        private ProcessingDialog pd;    // Is being shown when processing of the file starts

        private Thread writingThread;   // Thread for writing the file [currently not in use]
        #endregion
        #region Constructors
        /// <summary>This creates an instance of the MainForm class.</summary>
        /// <remarks>There should always only be one active at a time.</remarks>
        /// <param name="cmdLineArgs">Arguments given by the command line.</param>
        public MainForm(string[] cmdLineArgs)
        {
            // Initialize form, program, compontents
            InitializeComponent();
            Initialization();

            // If command line arguments exist, execute program according to them
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
        /// <summary>Is called by the system when the MainForm is loaded.</summary>
        /// <remarks>Better not call this on your own.</remarks>
        private void MainForm_Load(object sender, EventArgs e)
        {
        }
        /// <summary>
        /// <para/>Is called when the selection of the FileContentBox has changed.
        /// <para/>Will determine the new "from" and "to" values.
        /// </summary>
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
        /// <summary>
        /// <para/>Is called when the BrowseButton was clicked.
        /// <para/>Determines standard path or path used before. Then sets the pathname of FileInfo to the selected path.
        /// </summary>
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
        /// <summary>
        /// <para/>Is called when LoadButton was clicked.
        /// <para/>Loads the content of the selected file (by browse) into the program and displays it to the FileContentBox.
        /// <para/>If no file is selected by browse yet, this function will also call BrowseButton_Click(...).
        /// </summary>
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
            //ShowLoadingDialog(file.GetName());

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
        /// <summary>
        /// <para/>Is called when StartButton was clicked. 
        /// <para/>Starts the removing of selected columns and proceeds to write the result to a file.
        /// </summary>
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
                FileWriteException error = null;
                DialogResult dr = svd.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    // Start new thread and show MessageBox "done" when done
                    ChangeGuiLockingState(GuiLockingState.Lock);
                    ShowProcessingDialog();
                    try
                    {
                        // FYI: Because of the exceptions, this cant be done in another thread...
                        //writingThread = new Thread(() =>
                        //{
                        //    WriteFile(RemoveColumSpanInContent(file.GetContent(), realFrom, realTo), svd.FileName);
                        //});
                        //writingThread.Start();
                        WriteFile(RemoveColumSpanInContent(file.GetContent(), realFrom, realTo), svd.FileName);
                    }
                    catch (FileWriteException ex)
                    {
                        error = ex;
                        errorOccured = true;
                    }
                    catch (Exception)
                    {
                        errorOccured = true;
                    }

                    CloseProcessingDialog();
                    ChangeGuiLockingState(GuiLockingState.Release);
                    if (errorOccured)
                    {
                        if (error != null)
                        {
                            WriteErrorLog(error, svd.FileName);
                        }
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
        /// <summary>
        /// <para/>Is called when CloseButton was clicked.
        /// <para/>Closes the program immediately.
        /// </summary>
        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
        /// <summary>
        /// <para/>Is called when the AboutButton was clicked.
        /// <para/>Opens the AboutDialog, a window where info about the program is displayed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutButton_Click(object sender, EventArgs e)
        {
            var AboutDialog = new AboutDialog();
            AboutDialog.ShowDialog();
        }
        /// <summary>
        /// <para/>Is called when the NumericUpDown for the "to" value changes its value.
        /// <para/>Just calls NumericUpDownBoxChanged(...).
        /// </summary>
        private void ToNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDownBoxChanged();
        }
        /// <summary>
        /// <para/>Is called when the NumericUpDown for the "from" value changes its value.
        /// <para/>Just calls NumericUpDownBoxChanged(...).
        /// </summary>
        private void FromNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDownBoxChanged();
        }
        /// <summary>
        /// <para/>Is called when the cancel button from the processing dialog was clicked.
        /// <para/>Aborts the removing of the columns and saving of the result into a file and thus stops the whole process.
        /// </summary>
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
        /// <summary>Loads a file specified in the FileInfo's path.</summary>
        /// <returns>All lines of the file as List<string></returns>
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
        /// <summary>Writes all lines in a List<string> into a file specified by filePath and closes the file.</summary>
        /// <param name="content">List of strings that are to be written into the file. The order is the same as the content of the list itself.</param>
        /// <param name="filePath">Path and name and extension of the file that should be written.</param>
        /// <exception cref="FileWriteException">Can throw the dedicated exception that indicates something went wrong with this file writing process.</exception>
        private void WriteFile(List<string> content, string filePath)
        {
            // TODO: Inform youself: How to best handle problems of file writing/reading
            var errorOccured = false;
            FileWriteException fileWriteException = new FileWriteException();

            if (string.IsNullOrEmpty(filePath) || content == null || content.Count <= 0)
            {
                throw new FileWriteException("File is empty.");
            }

            // Write file
            try
            {
                using (StreamWriter sW = new StreamWriter(new FileStream(filePath, FileMode.Create), Encoding.UTF8))
                {
                    for (int index = 0; index < content.Count; index++)
                        sW.WriteLine(content.ElementAt(index));
                }
            }
            catch (Exception ex)
            {
                fileWriteException.AddInnerException(ex);
                errorOccured = true;
            }

            // Verify file content and check whether file writing was done successfully
            List<string> testContent = new List<string>();
            try
            {
                using (StreamReader sR = new StreamReader(new FileStream(filePath, FileMode.Open), Encoding.UTF8))
                {
                    while (!sR.EndOfStream)
                        testContent.Add(sR.ReadLine());
                }
            }
            catch (Exception ex)
            {
                fileWriteException.AddInnerException(ex);
                errorOccured = true;
            }
            // If read file is empty
            if (testContent.Count <= 0)
            {
                fileWriteException.AddInnerException(new Exception("Written file was empty."));
                errorOccured = true;
            }
            // Check whether every line in file corresponds to the same line in content           
            if (testContent.Count != content.Count)
            {
                fileWriteException.AddInnerException(new Exception("Written content is not the same as the generated content."));
                errorOccured = true;
            }
            else
            {
                for (int i = 0; i < testContent.Count; i++)
                {
                    if (testContent[i] != content[i])
                    {
                        fileWriteException.AddInnerException(new Exception("Written content is not the same as the generated content."));
                        errorOccured = true;
                        break;
                    }
                }
            }

            if (errorOccured)
            {
                throw fileWriteException;
            }
        }
        /// <summary>Writes all lines of a string into a file specified by filePath and closes the file.</summary>
        /// <param name="content">String content that is to be written into the file.</param>
        /// <param name="filePath">Path and name and extension of the file that should be written.</param>
        /// <exception cref="FileWriteException">Can throw the dedicated exception that indicates something went wrong with this file writing process.</exception>
        /// <remarks>Splits the lines of the string at the default system line break and ignores white lines.</remarks>
        private void WriteFile(string content, string filePath)
        {
            // Convert string to lines of content
            string[] splitContent = content.Split(Environment.NewLine.ToCharArray());
            List<string> listedContent = new List<string>();
            for (int i = 0; i < splitContent.Length; i++)
            {
                if (string.IsNullOrEmpty(splitContent[i]) || string.IsNullOrWhiteSpace(splitContent[i]))
                    continue;

                listedContent.Add(splitContent[i]);
            }

            WriteFile(listedContent, filePath);
        }
        /// <summary>
        /// <para/>Writes an error log file when something went wrong while writing a file with WriteFile(...).
        /// <para/>The error log will be saved in the format "[fileName]_error_yyyy-MM-dd_HH-mm-ss.log" in the filePath directory.
        /// </summary>
        /// <param name="filePath">Where the file should be saved.</param>
        /// <remarks>Writes down all inner exceptions of the FileWriteException. When this process also fails, no exception will be thrown but just ignored.</remarks>
        private void WriteErrorLog(FileWriteException exception, string filePath)
        {
            // Change from written file to [...]_error_yyyy-MM-dd_HH-mm-ss.log
            string fileDateTimeFormat = "yyyy-MM-dd_HH-mm-ss";
            string onlyFilePath = System.IO.Path.GetDirectoryName(filePath); //FileInfo.ExtractFilePath(filePath);
            string onlyFileName = System.IO.Path.GetFileNameWithoutExtension(filePath); //FileInfo.ExtractFileName(filePath);
            string newFilePath = (onlyFilePath + System.IO.Path.DirectorySeparatorChar + onlyFileName + "_error_" + DateTime.Now.ToString(fileDateTimeFormat) + ".log");

            // Create error log content
            StringBuilder sb = new StringBuilder();
            {
                sb.AppendLine(Program.ProgramHeader + " | Error log | " + DateTime.Now.ToString(Program.ICul));
                sb.AppendLine("The file \"" + filePath + "\" couldn't be written properly.");
                sb.AppendLine("Below is a list of errors that occured:\n");
                sb.AppendLine("----------[Start error list]-------------------------");
                sb.Append(exception.ToString());
                sb.AppendLine("----------[End error list]---------------------------");
                sb.AppendLine("\nFor more information, visit: \"https://sunburst275.jimdofree.com/about-1/contact\" and/or contact Sunburst275 directly.");
            }

            try
            {
                WriteFile(sb.ToString(), newFilePath);
            }
            catch (Exception)
            {
                // Ignore
            }
        }
        /// <summary>
        /// Removes a column span in all the strings of a list of strings. 
        /// </summary>
        /// <param name="content">The list of strings where a column span is to be removed.</param>
        /// <param name="from">From which column of the string the characters should be removed.</param>
        /// <param name="to">Up to which column of the string the characters should be removed.</param>
        /// <returns>A list of the same strings but with removed characters inside of the column span.</returns>
        /// <remarks>
        /// <para/>When a string is too short for the "to" value, the method will remove everything from the "from" value to the end of the string.
        /// <para/>When a string is too short for even the "from" value, nothing will be removed.
        /// <para/>When a string is long enough so that the "to" and the "from" value are inside the lenght of the string, the column span will be removed as specified.
        /// </remarks>
        private List<string> RemoveColumSpanInContent(List<string> content, int from, int to)
        {
            List<string> contentToEdit = content;
            List<string> editedContent = contentToEdit;

            // Removing columns in lines according to specified "from" and "to"
            for (int itemIndex = 0; itemIndex < contentToEdit.Count; itemIndex++)
            {
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
            }

            return editedContent;
        }
        #endregion
        #region Helper
        /// <summary>
        /// <para\>Sets the CurrentlyOpenedLabel to the msg. 
        /// <para\>If the msg is too long, it will be truncated and the continuation of the msg will be indicated by "...".
        /// </summary>
        /// <remarks>Is intended to set the msg to the currentyl openend file path. The max limit is determined by "MaxCurrentlyOpenedLabelLength".</remarks>
        /// <paramref name="msg"/>The msg that should be displayed as the label.
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
        /// <summary>Is intended to be called when the NumericUpDownBoxes changed their value. Calculates the actual values of the columns and checks whether the selection is valid.</summary>
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
            // TODO: Max line length check, too (Really?)

            // Selection calculation and setting
            FileContentBox.SelectionStart = fromVal;
            FileContentBox.SelectionLength = toVal - fromVal;

            // Set global from
            from = fromVal;
            to = toVal;
        }
        /// <summary>Calculates the SelectionBounds of each line directly.</summary>
        /// <remarks>Should be directly called when the program loaded a text file.</remarks>
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
        /// <summary>Checks if the selection is valid by checking each line of the loaded file.</summary>
        /// <param name="from">Column from which on content should be removed.</param>
        /// <param name="to">Column up to which content should be removed.</param>
        /// <returns>True when the selection is valid, otherwise false.</returns>
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
        /// <summary>Calculates the actual "from" and "to" values independent from the line and length of the text from the FileContentBox (RichTextBox).</summary>
        /// <remarks>Dont use this to change the SpinnerBoxes' (NumericUpDown's) values, unless you want to make the code 1000x more complex.</remarks>
        /// <returns>The actual values of the "from" and "to" values. Meaning: (realFrom, realTo)</returns>
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
        /// <summary>Displays the from file read content in the FileContentBox and unlocks the main GUI of the MainForm.</summary>
        /// <param name="readLines">List of read lines of the file.</param>
        /// <remarks>Shows error message when the file was empty and sets the text of FileContentBox to "".</remarks>
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
        /// <summary>Shows the ProcessingDialog</summary>
        /// <remarks>Is intended to be called when the processing starts and no other ProcessingDialog is opened yet.</remarks>
        private void ShowProcessingDialog()
        {
            pd = new ProcessingDialog();
            pd.ProcessCancellingRequested += OnProcessingDialog_Cancel;
            pd.ShowDialog();
        }
        /// <summary>Closes down the ProcessingDialog that has been opened by ShowProcessingDialog() when its opened, otherwise nothing happens.</summary>
        private void CloseProcessingDialog()
        {
            if (pd != null)
            {
                pd.ProcessCancellingRequested -= OnProcessingDialog_Cancel;
                pd.Close();
                pd = null;
            }
        }
        /// <summary>Shows a MessageBox that says the file is now being loaded with the filePath/fileName displayed in it.</summary>
        /// <param name="fileName">The file that will be opened.</param>
        /// <remarks>Is intended to be called when a file is being loaded. [Is currently unused.]</remarks>
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
        /// <summary>Locks or unlocks the GUI according to the entered parameter.</summary>
        /// <remarks>Used e.g. when the processing of the file starts (and ends).</remarks>
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
        /// <summary>Shows a MessageBox with a message according to the result of the processing of the file.</summary>
        private void ShowProcessingResult(ProcessingDialogResult processingDialogResult)
        {
            // Show msgbox when "done"
            switch (processingDialogResult)
            {
                case (ProcessingDialogResult.Success):
                    MessageBox.Show("Removal of specified columns is done.\nYou may close the program now\nor continue.",
                                    Program.ProgramHeaderWithDash + "Processing done",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                    break;
                case (ProcessingDialogResult.Cancelled):
                    MessageBox.Show("Removal of specified columns was cancelled.\nYou may close the program now\nor continue.",
                        Program.ProgramHeaderWithDash + "Processing cancelled",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    break;
                case (ProcessingDialogResult.Failed):
                    MessageBox.Show("Removal of specified columns failed.\nAn error log should be in the directory you wanted to save your file in. You may close the program now or continue.",
                        Program.ProgramHeaderWithDash + "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    break;
            }
        }
        /// <summary>Processes the command line arguments. </summary>
        /// <remarks>If the command arguments are not as they should be, this method just doesn't do anything.</remarks>
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
                MessageBox.Show("Program executed without errors.", Program.ProgramHeaderWithDash + "Processing done.", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // Close program
            Environment.Exit(exitCode);
        }
        #endregion
        #region Helping structures
        /// <summary>Saves the start, end, content and (in a collection) index of a line/string of a file.</summary>
        /// <remarks>Start and end are usually relative to the "whole string" of the file content.</remarks>
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
