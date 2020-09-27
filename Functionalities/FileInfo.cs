using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace RAPIS_FIMC
{
    class FileInfo
    {
        #region Enums
        // File extension strings
        public enum FileType { tsv, txt, csv, /*xlsx, docx,*/ htm, html, log, other, none };
        public static readonly Dictionary<FileType, string> supportedFileTypes = new Dictionary<FileType, string>
        {
            {FileType.txt, "txt" },
            {FileType.tsv, "tsv" },
            {FileType.csv, "csv" },
            //{FileType.xlsx, "xlsx" },
            //{FileType.docx, "docx" },
            {FileType.htm, "htm" },
            {FileType.html, "html" },
            {FileType.log, "log" }
        };

        #endregion
        #region Members
        private string pathAndName;
        private string path;
        private string name;
        private FileType extension;
        private List<string> content;
        #endregion
        #region Constructor
        public FileInfo()
        {
            pathAndName = string.Empty;
            path = string.Empty;
            name = string.Empty;
            extension = FileType.none;
            content = new List<string>();
        }
        #endregion
        #region Getter
        public string GetPathAndName()
        {
            return pathAndName;
        }
        public string GetName()
        {
            return name;
        }
        public string GetPath()
        {
            return path;
        }
        public FileType GetFileType()
        {
            return extension;
        }
        public string GetFileTypeString()
        {
            foreach (KeyValuePair<FileType, string> ft in supportedFileTypes)
            {
                if (ft.Key == extension)
                {
                    return ft.Value;
                }
            }

            return null;
        }
        public string GetContentAt(int index)
        {
            return content.ElementAt(index);
        }
        public List<string> GetContent()
        {
            return content;
        }
        public int GetContentCount()
        {
            return content.Count;
        }
        public bool IsValid()
        {
            return
                (
                    (content != null) &&
                    (extension != FileType.none) &&
                    (extension != FileType.other) &&
                    (content.Count > 0) &&
                    !string.IsNullOrEmpty(pathAndName) &&
                    !string.IsNullOrEmpty(path) &&
                    !string.IsNullOrEmpty(name)
                );
        }
        #endregion
        #region Setter
        public void SetPathAndName(string pathAndName)
        {
            if (string.IsNullOrEmpty(pathAndName))
            {
                this.pathAndName = path = name = pathAndName;
                extension = FileType.none;
            }
            else
            {
                this.pathAndName = pathAndName;
                this.path = ExtractFilePath(pathAndName);
                this.name = ExtractFileName(pathAndName);
                this.extension = ExtractFileType(this.name);
            }
        }
        public void SetContentAt(string content, int index)
        {
            this.content[index] = content;
        }
        public void SetContent(List<string> content)
        {
            if (content == null) throw new NullReferenceException("content of file == null");
            this.content.Clear();
            this.content = content;
        }
        #endregion
        #region Static Methods
        public static string ExtractFilePath(string filePathAndName)
        {
            string[] filePathAndNameSplitted = filePathAndName.Split(System.IO.Path.DirectorySeparatorChar);
            //filePathAndNameSplitted[filePathAndNameSplitted.Length - 1];

            StringBuilder sb = new StringBuilder(filePathAndNameSplitted.Length - 2);
            for (int i = 0; i <= filePathAndNameSplitted.Length - 2; i++)
            {
                sb.Append(filePathAndNameSplitted[i]);
                sb.Append(System.IO.Path.DirectorySeparatorChar);
            }
            return sb.ToString();
        }
        public static FileType ExtractFileType(string fileNameWithExtension)
        {
            string[] fileNameAndExtensionSplitted = fileNameWithExtension.Split('.');
            string fileExtension = fileNameAndExtensionSplitted[fileNameAndExtensionSplitted.Length - 1];

            foreach (KeyValuePair<FileType, string> ft in supportedFileTypes)
            {
                if (ft.Value == fileExtension)
                    return ft.Key;
            }

            return FileType.other;
        }
        public static string ExtractFileExtension(string fileNameWithExtension)
        {
            string[] fileNameAndExtensionSplitted = fileNameWithExtension.Split('.');
            return fileNameAndExtensionSplitted[fileNameAndExtensionSplitted.Length - 1];
        }
        public static string ExtractFileName(string filePathAndName)
        {
            string[] filePathAndNameSplitted = filePathAndName.Split(System.IO.Path.DirectorySeparatorChar);
            return filePathAndNameSplitted[filePathAndNameSplitted.Length - 1];
        }
        public static bool IsValidFileType(string fileExtension)
        {
            foreach (KeyValuePair<FileType, string> de in supportedFileTypes)
            {
                if (de.Value == fileExtension)
                    return true;
            }
            return false;
        }

        #endregion
    }

    /// <summary>An exception that is thrown when something went wrong during file writing.</summary>
    class FileWriteException : Exception
    {
        #region Members
        /// <summary>All exceptions that may have been thrown.</summary>
        List<Exception> innerExceptions;
        #endregion
        #region Methods
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(innerExceptions.Count);
            foreach(Exception ex in innerExceptions)
            {
                sb.Append(ex.Message);
                sb.AppendLine("");
            }

            return sb.ToString();
        }
        public void AddInnerException(Exception ex)
        {
            innerExceptions.Add(ex);
        }
        #endregion
        public FileWriteException()
        {
            innerExceptions = new List<Exception>();
        }
        public FileWriteException(string message)
        {
            innerExceptions = new List<Exception>();
            innerExceptions.Add(new Exception(message));
        }
    }
    /// <summary>
    /// An exception that is thrown when something went wrong during file loading. Contains states that describe the exceptions status further.
    /// </summary>
    class FileReadException : Exception
    {
        #region Helper Structures
        public enum FileReadExceptionState { Reading, Empty, Other };
        #endregion
        #region Members
        public FileReadExceptionState fileReadExceptionState;
        public Exception innerException;
        #endregion
        public FileReadException(FileReadExceptionState fileReadExceptionState, Exception innerException)
        {
            this.fileReadExceptionState = fileReadExceptionState;
            this.innerException = innerException;
        }
        public FileReadException(FileReadExceptionState fileReadExceptionState)
        {
            new FileReadException(fileReadExceptionState, null);
        }

    }

}
