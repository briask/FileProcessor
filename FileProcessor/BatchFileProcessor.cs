using System;
using System.Collections.Generic;
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

        public int ProcessAllFiles(IFileProcessor fileProcessor)
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

            int successfullyProcessed = 0;
            foreach (var filename in allFiles)
            {
                bool success = ProcessFile(filename, fileProcessor);
                if (success)
                {
                    successfullyProcessed++;
                    successfullyProcessedFiles.Add(filename);
                }
                else
                {
                    unsuccessfullyProcessedFiles.Add(filename);
                }
            }

            return successfullyProcessed;
        }

        public bool ProcessFile(string filename, IFileProcessor fileProcessor)
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
                bool success = fileProcessor.ProcessFile(fullFileName);
                if (success)
                {
                    bool successfulMove = MoveFileToProcessedLocation(fullFileName);
                    if (successfulMove)
                    {
                        log.InfoFormat("Success");
                        return false;
                    }
                    else
                    {
                        log.ErrorFormat("Unable to process file {0}", fullFileName);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Unable to process file {0} {1}", filename, ex);
            }

            return false;
        }

        private bool MoveFileToProcessedLocation(string filename)
        {
            try
            {
                var originalfile = new FileInfo(filename);

                var newFileLocation = Path.Combine(this.processedFilesPath, originalfile.Name);

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

