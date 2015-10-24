using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileProcessor
{
    class BatchFileProcessorException : Exception
    {
        public BatchFileProcessorException() : base()
        {
        }

        public BatchFileProcessorException(string message) 
            : base(message)
        {
        }

        public BatchFileProcessorException(string format, params object[] args) 
            : base(string.Format(format, args))
        {
        }

        public BatchFileProcessorException(string message, Exception innerException)   : base(message, innerException)
        {
        }

        public BatchFileProcessorException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException)
        {
        }
    }
}
