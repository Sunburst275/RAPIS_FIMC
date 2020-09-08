using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAPIS_FIMC
{
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
