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

        public void ProcessAllFiles(IFileProcessor fileProcessor)
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

            foreach (var filename in allFiles)
            {
                ProcessFile(filename, fileProcessor);
            }
        }

        public void ProcessFile(string filename, IFileProcessor fileProcessor)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentNullException("filename");
            }

            if (fileProcessor == null)
            {
                throw new ArgumentNullException("fileProcessor");
            }

            if (!File.Exists(filename))
            {
                throw new FileNotFoundException("File does not exist at location specified!", filename);
            }

            try
            {
                bool success = fileProcessor.ProcessFile(filename);
                if (success)
                {
                    bool successfulMove = MoveFileToProcessedLocation(filename);
                    if (successfulMove)
                    {
                        log.InfoFormat("Success");

                    }
                    else
                    {
                        log.ErrorFormat("Unable to process file {0}", filename);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Unable to process file {0} {1}", filename, ex);
            }
        }

        private bool MoveFileToProcessedLocation(string filename)
        {
            try
            {
                var originalfile = new FileInfo(filename);

                var newFileLocation = Path.Combine(this.unprocessedFilesPath, originalfile.Name);

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
