using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FileProcessor
{
    public interface IFileProcessor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filelocation"> fully qualified path name for the file to be processed</param>
        /// <returns>true if file processed correctly, false if file did not process </returns>
        DataSet ProcessFile(string filelocation);
    }
}
