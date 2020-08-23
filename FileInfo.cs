using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Multiliner
{
    class FileInfo
    {
        #region Enums
        // File extensions
        const string tsv = "tsv";
        const string txt = "txt";
        public enum FileType { tsv, txt, other, none };
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
        #endregion
        #region Setter
        public void SetPathAndName(string pathAndName)
        {
           this.pathAndName = pathAndName;
        }
        public void SetName(string name)
        {
            this.name = name;
        }
        public void SetPath(string path)
        {
            this.path = path;
        }
        public void SetType(FileType extension)
        {
            this.extension = extension;
        }
        public void SetContentAt(string content, int index)
        {
            this.content[index] = content;
        }
        public void SetContent(List<string> content)
        {
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

            if (fileExtension == tsv)
            {
                return FileType.tsv;
            }
            else if (fileExtension == txt)
            {
                return FileType.txt;
            }

            return FileType.other;
        }
        public static string ExtractFileName(string filePathAndName)
        {
            string[] filePathAndNameSplitted = filePathAndName.Split(System.IO.Path.DirectorySeparatorChar);
            return filePathAndNameSplitted[filePathAndNameSplitted.Length - 1];
        }
        #endregion
    }
}
