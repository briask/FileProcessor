using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace FileProcessor
{
    public class BatchFileProcessor
    {
        private string unprocessedFilesPath;
        private string processedFilesPath;

        private List<string> successfullyProcessedFiles;
        private List<string> unsuccessfullyProcessedFiles;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private BatchFileProcessor()
        {
        }

        public BatchFileProcessor(string unprocessedFilesPath, string processedFilesPath)
        {
            if (string.IsNullOrWhiteSpace(unprocessedFilesPath))
            {
                throw new ArgumentNullException("unprocessedFilesPath");
            }

            if (string.IsNullOrWhiteSpace(processedFilesPath))
            {
                throw new ArgumentNullException("processedFilesPath");
            }

            this.unprocessedFilesPath = unprocessedFilesPath;
            this.processedFilesPath = processedFilesPath;
        }

        public IDictionary<string, DataSet> ProcessAllFiles(IFileProcessor fileProcessor)
        {
            if (fileProcessor == null)
            {
                throw new ArgumentNullException("fileProcessor");
            }

            if (!Directory.Exists(unprocessedFilesPath) && !Directory.Exists(processedFilesPath))
            {
                throw new BatchFileProcessorException("HELP!!!");
            }

            var allFiles = Directory.EnumerateFiles(unprocessedFilesPath);

            successfullyProcessedFiles = new List<string>();
            unsuccessfullyProcessedFiles = new List<string>();

            var toReturn = new Dictionary<string, DataSet>();

            int successfullyProcessed = 0;
            foreach (var filename in allFiles)
            {
                var shortFilename = new FileInfo(filename).Name;
                DataSet data = ProcessFile(filename, fileProcessor);
                if (data.Tables.Count > 0)
                {
                    successfullyProcessed++;
                    toReturn.Add(filename, data);
                    successfullyProcessedFiles.Add(filename);
                }
                else
                {
                    unsuccessfullyProcessedFiles.Add(filename);
                }
            }

            return toReturn;
        }

        public DataSet ProcessFile(string filename, IFileProcessor fileProcessor)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentNullException("filename");
            }

            if (fileProcessor == null)
            {
                throw new ArgumentNullException("fileProcessor");
            }

            var fullFileName = Path.Combine(this.unprocessedFilesPath, filename);

            if (!File.Exists(fullFileName))
            {
                throw new FileNotFoundException("File does not exist at location specified!", filename);
            }

            try
            {
                DataSet data = fileProcessor.ProcessFile(fullFileName);
                if (data.Tables.Count > 0)
                {
                    bool successfulMove = MoveFileToProcessedLocation(fullFileName);
                    if (successfulMove)
                    {
                        data.DataSetName = fullFileName;
                        log.InfoFormat("Success");
                        return data;
                    }
                    else
                    {
                        log.ErrorFormat("Unable to process file {0}", fullFileName);
                        return new DataSet();
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Unable to process file {0} {1}", filename, ex);
            }

            return new DataSet();
        }

        private bool MoveFileToProcessedLocation(string filename)
        {
            try
            {
                var originalfile = new FileInfo(filename);
                string originalFilename = originalfile.Name;

                var processedTime = DateTime.Now.ToString("yyyy-MM-ddThhmmss");
                var newFileName = string.Format("{0}.{1}", originalFilename, processedTime);

                var newFileLocation = Path.Combine(this.processedFilesPath, newFileName);
                if (File.Exists(newFileLocation))
                {
                    processedTime = DateTime.Now.ToString("yyyy-MM-ddThhmmss.fff");
                    newFileName = string.Format("{0}.{1}", originalFilename, processedTime);
                    newFileLocation = Path.Combine(this.processedFilesPath, newFileName);

                    if (File.Exists(newFileLocation))
                    {
                        newFileName = string.Format("{0}.{1}", originalFilename, Guid.NewGuid().ToString("N"));
                        newFileLocation = Path.Combine(this.processedFilesPath, newFileName);
                    }
                }

                originalfile.MoveTo(newFileLocation);

                return true;
            }
            catch (Exception ex)
            {
                log.Fatal("MoveFileToProcessedLocation error : ", ex);
            }

            return false;
        }
    }
}

