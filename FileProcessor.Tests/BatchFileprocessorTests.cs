﻿using FakeItEasy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace FileProcessor.Tests
{
    public class BatchFileprocessorTests : IDisposable
    {
        private string processedPath;
        private string unprocessedPath;
        private string commonDir;

        public BatchFileprocessorTests()
        {
            var tempPath = Path.GetTempPath();
            var common = Guid.NewGuid().ToString("N");

            commonDir = Path.Combine(tempPath, common);

            processedPath = Path.Combine(tempPath, common, "processed");
            unprocessedPath = Path.Combine(tempPath, common, "unprocessed");

            Directory.CreateDirectory(processedPath);
            Directory.CreateDirectory(unprocessedPath);

            var sut = new BatchFileProcessor(unprocessedPath, processedPath);
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

        [Fact]
        public void ProcessFile_FileExistsProcessed_MovedFile()
        {
            // Arrange
            CreateSingleTempFile();
            string filename = GetSingleUnprocessedFile();

            var fp = A.Fake<IFileProcessor>();
            A.CallTo(() => fp.ProcessFile(filename)).Returns(true);
            var sut = new BatchFileProcessor(this.unprocessedPath, this.processedPath);

            // Act
            sut.ProcessFile(filename, fp);

            // Assert
            A.CallTo(() => fp.ProcessFile(filename)).MustHaveHappened();
            A.CallTo(() => fp.ProcessFile(filename)).Returns(true);
            Assert.Equal(1, Directory.EnumerateFiles(processedPath).Count());
        }

        [Fact]
        public void ProcessFile_FileExistsNotProcessed_NotMovedFile()
        {
            // Arrange
            CreateSingleTempFile();
            string filename = GetSingleUnprocessedFile();

            var fp = A.Fake<IFileProcessor>();
            A.CallTo(() => fp.ProcessFile(filename)).Returns(false);
            var sut = new BatchFileProcessor(this.unprocessedPath, this.processedPath);

            // Act
            sut.ProcessFile(filename, fp);

            // Assert
            A.CallTo(() => fp.ProcessFile(filename)).MustHaveHappened();
            A.CallTo(() => fp.ProcessFile(filename)).Returns(false);
            Assert.Equal(1, Directory.EnumerateFiles(unprocessedPath).Count());
        }

        [Fact]
        public void ProcessFile_FileNotExist_FileRemains()
        {
            // Arrange
            CreateSingleTempFile();
            string filename = GetSingleUnprocessedFile();

            var fp = A.Fake<IFileProcessor>();
            A.CallTo(() => fp.ProcessFile(filename)).Returns(true);
            var sut = new BatchFileProcessor(this.unprocessedPath, this.processedPath);

            // Act
            Assert.Throws<FileNotFoundException>(() => sut.ProcessFile(Guid.NewGuid().ToString("N"), fp));
            Assert.Equal(1, Directory.EnumerateFiles(unprocessedPath).Count());
        }

        [Fact]
        public void ProcessAllFiles_NoFiles_NothingHappened()
        {
            // Arrange
            var fp = A.Fake<IFileProcessor>();
            A.CallTo(() => fp.ProcessFile(string.Empty)).Returns(true);
            var sut = new BatchFileProcessor(this.unprocessedPath, this.processedPath);

            // Act
            var filesProcessed = sut.ProcessAllFiles(fp);
            
            Assert.Equal(0, filesProcessed);
            A.CallTo(() => fp.ProcessFile(string.Empty)).MustNotHaveHappened();
        }

        //[Fact]
        //public void ProcessAllFiles_5Files_5FilesProcessed()
        //{
        //    // Arrange
        //    CreateMultipleTempFiles(5);
        //    var fileList = GetAllUnprocessedFiles();

        //    var fp = A.Fake<IFileProcessor>();
        //    foreach (var filename in fileList)
        //    {
        //        A.CallTo(() => fp.ProcessFile(filename)).Returns(true);
        //    }
            
        //    var sut = new BatchFileProcessor(this.unprocessedPath, this.processedPath);

        //    // Act
        //    var filesProcessed = sut.ProcessAllFiles(fp);

        //    Assert.Equal(5, filesProcessed);
        //    foreach (var filename in fileList)
        //    {
        //        A.CallTo(() => fp.ProcessFile(filename)).MustHaveHappened();
        //    }
        //}
    }
}