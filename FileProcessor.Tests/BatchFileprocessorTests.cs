using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Xunit;

namespace FileProcessor.Tests
{
    using System.Reflection;

    public class BatchFileprocessorTests : IDisposable
    {
        private string processedPath;
        private string unprocessedPath;
        private string errorPath;
        private string commonDir;

        #region setup
        public BatchFileprocessorTests()
        {
            var tempPath = Path.GetTempPath();
            var common = Guid.NewGuid().ToString("N");

            commonDir = Path.Combine(tempPath, common);

            processedPath = Path.Combine(tempPath, common, "processed");
            unprocessedPath = Path.Combine(tempPath, common, "unprocessed");
            errorPath = Path.Combine(tempPath, common, "error");

            Directory.CreateDirectory(processedPath);
            Directory.CreateDirectory(unprocessedPath);
            Directory.CreateDirectory(errorPath);
        }

        private void CreateSingleTempFile()
        {
            CreateTempFile();
        }

        private void CreateMultipleTempFiles(int numberOfFiles)
        {
            for (int i = 0; i < numberOfFiles; i++)
            {
                CreateTempFile();
            }
        }

        private void CreateTempFile()
        {
            var tempfile = Path.GetTempFileName();
            var testfile = new FileInfo(tempfile);

            testfile.MoveTo(Path.Combine(unprocessedPath, testfile.Name));
        }

        private DataSet CreateMockDataSet1TableNoData()
        {
            var data = new DataSet();
            data.Tables.Add(new DataTable());

            return data;
        }

        public void Dispose()
        {
            Directory.Delete(commonDir, true);
        }

        private string GetSingleUnprocessedFile()
        {
            return Directory.EnumerateFiles(unprocessedPath).FirstOrDefault();
        }

        private IEnumerable<string> GetAllUnprocessedFiles()
        {
            return Directory.EnumerateFiles(unprocessedPath).ToList();
        }
        #endregion

        #region Tests
        [Fact]
        public void ProcessFile_FileExistsProcessed_MovedFile()
        {
            // Arrange
            CreateSingleTempFile();
            string filename = GetSingleUnprocessedFile();

            var fp = A.Fake<IFileProcessor>();
            A.CallTo(() => fp.ProcessFile(filename)).Returns(CreateMockDataSet1TableNoData());
            var sut = new BatchFileProcessor(this.unprocessedPath, this.processedPath, this.errorPath);

            // Act
            sut.ProcessFile(filename, fp);

            // Assert
            A.CallTo(() => fp.ProcessFile(filename)).MustHaveHappened();
            A.CallTo(() => fp.ProcessFile(filename)).Returns(A<DataSet>.Ignored);
            Assert.Equal(1, Directory.EnumerateFiles(processedPath).Count());
        }

        [Fact]
        public void ProcessFile_FileExistsNotProcessed_MovedFileToErrorLocation()
        {
            // Arrange
            CreateSingleTempFile();
            string filename = GetSingleUnprocessedFile();

            var fp = A.Fake<IFileProcessor>();
            A.CallTo(() => fp.ProcessFile(filename)).Returns(new DataSet());
            var sut = new BatchFileProcessor(this.unprocessedPath, this.processedPath, this.errorPath);

            // Act
            sut.ProcessFile(filename, fp);

            // Assert
            A.CallTo(() => fp.ProcessFile(filename)).MustHaveHappened();
            Assert.Equal(1, Directory.EnumerateFiles(errorPath).Count());
        }

        [Fact]
        public void ProcessFile_FileNotExist_FileRemains()
        {
            // Arrange
            CreateSingleTempFile();
            string filename = GetSingleUnprocessedFile();

            var fp = A.Fake<IFileProcessor>();
            A.CallTo(() => fp.ProcessFile(filename)).Returns(A<DataSet>.Ignored);
            var sut = new BatchFileProcessor(this.unprocessedPath, this.processedPath, this.errorPath);

            // Act
            Assert.Throws<FileNotFoundException>(() => sut.ProcessFile(Guid.NewGuid().ToString("N"), fp));
            Assert.Equal(1, Directory.EnumerateFiles(unprocessedPath).Count());
        }

        [Fact]
        public void ProcessAllFiles_NoFiles_NothingHappened()
        {
            // Arrange
            var fp = A.Fake<IFileProcessor>();
            A.CallTo(() => fp.ProcessFile(string.Empty)).Returns(A<DataSet>.Ignored);
            var sut = new BatchFileProcessor(this.unprocessedPath, this.processedPath, this.errorPath);

            // Act
            var filesProcessed = sut.ProcessAllFiles(fp);
            
            Assert.Equal(0, filesProcessed.Count);
            A.CallTo(() => fp.ProcessFile(string.Empty)).MustNotHaveHappened();
        }

        [Fact]
        public void ProcessAllFiles_5Files_5FilesProcessed()
        {
            // Arrange
            CreateMultipleTempFiles(5);
            var fileList = GetAllUnprocessedFiles();

            var fp = A.Fake<IFileProcessor>();
            foreach (var filename in fileList)
            {
                A.CallTo(() => fp.ProcessFile(filename)).Returns(CreateMockDataSet1TableNoData());
            }

            var sut = new BatchFileProcessor(this.unprocessedPath, this.processedPath, this.errorPath);

            // Act
            var filesProcessed = sut.ProcessAllFiles(fp);

            Assert.Equal(5, filesProcessed.Count);
            foreach (var filename in fileList)
            {
                A.CallTo(() => fp.ProcessFile(filename)).MustHaveHappened();
            }

            Assert.Equal(5, Directory.EnumerateFiles(processedPath).Count());
        }

        [Fact]
        public void ProcessAllFiles_5Files_4FilesProcessed1Failed_4Moved_1MovedToErrorLocation()
        {
            // Arrange
            CreateMultipleTempFiles(5);
            var fileList = GetAllUnprocessedFiles();

            var fp = A.Fake<IFileProcessor>();
            foreach (var filename in fileList.Take(4))
            {
                A.CallTo(() => fp.ProcessFile(filename)).Returns(CreateMockDataSet1TableNoData());
            }

            var sut = new BatchFileProcessor(this.unprocessedPath, this.processedPath, this.errorPath);

            // Act
            var filesProcessed = sut.ProcessAllFiles(fp);

            foreach (var filename in fileList.Take(4))
            {
                A.CallTo(() => fp.ProcessFile(filename)).MustHaveHappened();
            }

            Assert.Equal(4, Directory.EnumerateFiles(processedPath).Count());
            Assert.Equal(1, Directory.EnumerateFiles(errorPath).Count());
        }

        [Fact]
        public void ProcessAllFiles_5Files_5FilesProcessedNameIsFileName()
        {
            // Arrange
            CreateMultipleTempFiles(5);
            var fileList = GetAllUnprocessedFiles();

            var fp = A.Fake<IFileProcessor>();
            foreach (var filename in fileList)
            {
                A.CallTo(() => fp.ProcessFile(filename)).Returns(CreateMockDataSet1TableNoData());
            }

            var sut = new BatchFileProcessor(this.unprocessedPath, this.processedPath, this.errorPath);

            // Act
            var filesProcessed = sut.ProcessAllFiles(fp);

            Assert.Equal(5, filesProcessed.Count);
            foreach (var filename in fileList)
            {
                A.CallTo(() => fp.ProcessFile(filename)).MustHaveHappened();
            }

            foreach (var filename in fileList)
            {
                Assert.Contains(filename, filesProcessed.Keys);
            }

            Assert.Equal(5, Directory.EnumerateFiles(processedPath).Count());
        }

        [Fact]
        public void ProcessAllFiles_ErrorFile_MovesFileToErrorLocation()
        {
            CreateSingleTempFile();
            var fp = A.Fake<IFileProcessor>();
            var filename = GetSingleUnprocessedFile();
            A.CallTo(() => fp.ProcessFile(filename)).Returns(null);

            var sut = new BatchFileProcessor(this.unprocessedPath, this.processedPath, this.errorPath);
            var processAllFiles = sut.ProcessAllFiles(fp);

            A.CallTo(() => fp.ProcessFile(filename)).MustHaveHappened();

            Assert.Equal(1, Directory.EnumerateFiles(errorPath).Count());
        }

        #endregion
    }
}
