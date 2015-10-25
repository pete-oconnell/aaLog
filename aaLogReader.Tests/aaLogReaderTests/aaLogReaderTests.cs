﻿using System.Linq;
using aaLogReader.Helpers;
using NUnit.Framework;
using System.IO;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;



namespace aaLogReader.Tests.aaLogReaderTests
{
    [TestFixture]
    public class aaLogReaderTests
    {
        private const string ROOT_FILE_PATH = @"aaLogReaderTests";
        private const string LOG_FILE_PATH = ROOT_FILE_PATH + @"\logFiles";
        private const string LOG_FILE_INSTANCE = LOG_FILE_PATH + @"\2014R2-VS-WSP1445626381.aaLog";

        [TestFixtureSetUp]
        public void Setup()
        {

            // Cleanup any old cache files
            foreach(string filename in Directory.GetFiles(LOG_FILE_PATH,"*cache*"))
            {
                File.Delete(filename);
            }
        }

        [Test]
        public void OpenCurrentLogFile()
        {
            aaLogReader alr = new aaLogReader();

            ReturnCodeStruct rcs = alr.OpenCurrentLogFile(LOG_FILE_PATH);

            Assert.That(rcs.Status);
            Assert.That(rcs.Message.Length == 0);
        }

        [Test]
        public void OpenCurrentLogFileWithOptions()
        {
            OptionsStruct options = new OptionsStruct();
            options.LogDirectory = LOG_FILE_PATH;

            aaLogReader alr = new aaLogReader(options);

            ReturnCodeStruct rcs = alr.OpenCurrentLogFile();

            Assert.That(rcs.Status);
            Assert.That(rcs.Message.Length == 0);
        }
        
        [Test]
        public void OpenLogFile()
        {
            aaLogReader alr = new aaLogReader();

            ReturnCodeStruct rcs = alr.OpenLogFile(LOG_FILE_INSTANCE);

            Assert.That(rcs.Status);
            Assert.That(rcs.Message.Length == 0);
        }

        [Test]
        public void ReadLogFileReadHeader()
        {
            aaLogReader alr = new aaLogReader();

            ReturnCodeStruct rcs = alr.OpenLogFile(LOG_FILE_INSTANCE);

            Assert.That(rcs.Status);
            Assert.That(rcs.Message.Length == 0);


            string refHeaderJSON = File.ReadAllText(ROOT_FILE_PATH + @"\refFiles\2014R2-VS-WSP1445626381.header.json");
            string thisHeaderJSON = JsonConvert.SerializeObject(alr.CurrentLogHeader);

            Assert.AreEqual(refHeaderJSON, thisHeaderJSON);
        }

        /// <summary>
        /// Verify log file can be opened and closed without error
        /// </summary>
        [Test]
        public void CloseCurrentLogFile()
        {
            aaLogReader alr = new aaLogReader();

            ReturnCodeStruct rcs = alr.OpenLogFile(LOG_FILE_INSTANCE);

            Assert.That(rcs.Status);
            Assert.That(rcs.Message.Length == 0);

            rcs = alr.CloseCurrentLogFile();

            Assert.That(rcs.Status);
            Assert.That(rcs.Message.Length == 0);
        }

        /// <summary>
        /// Verify log file can be opened and closed without error
        /// </summary>
        [Test]
        public void CurrentLogFilePath()
        {
            aaLogReader alr = new aaLogReader();           

            ReturnCodeStruct rcs = alr.OpenLogFile(LOG_FILE_INSTANCE);

            Assert.That(rcs.Status);
            Assert.That(rcs.Message.Length == 0);

            Assert.AreEqual(alr.CurrentLogFilePath, LOG_FILE_INSTANCE,"Current log file path did not match");
            
        }

        /// <summary>
        /// Verify the first record can be retrieved correctly
        /// </summary>
        [Test]
        public void GetFirstRecord()
        {
            aaLogReader alr = new aaLogReader();

            alr.OpenLogFile(LOG_FILE_INSTANCE);

            LogRecord lr = alr.GetFirstRecord();
            
            string json = File.ReadAllText(ROOT_FILE_PATH + @"\refFiles\2014R2-VS-WSP1445626381.GetFirstRecord.json");

            Assert.AreEqual(json, JsonConvert.SerializeObject(lr),"Record contents did not match");

        }

        [Test]
        public void GetLastRecord()
        {
            aaLogReader alr = new aaLogReader();

            alr.OpenLogFile(LOG_FILE_INSTANCE);
            
            LogRecord lr = alr.GetLastRecord();

            string json = File.ReadAllText(ROOT_FILE_PATH + @"\refFiles\2014R2-VS-WSP1445626381.GetLastRecord.json");
            //File.WriteAllText(ROOT_FILE_PATH + @"\refFiles\2014R2-VS-WSP1445626381.GetLastRecord.json",JsonConvert.SerializeObject(lr));

            Assert.AreEqual(json, JsonConvert.SerializeObject(lr), "Record contents did not match");

        }

        [Test]
        public void GetLogFilePathsForMessageFileTime()
        {

            OptionsStruct options = new OptionsStruct();
            options.LogDirectory = LOG_FILE_PATH;

            aaLogReader alr = new aaLogReader();
            
            List<LogHeader> logHeaders = alr.LogHeaderIndex;

            // Loop through all entries in the header index and verify we correctly identify the first, last, and middle message

            foreach(LogHeader lh in logHeaders)
            {
                Assert.That(alr.GetLogFilePathsForMessageFileTime(lh.EndFileTime).Exists(x=>x==lh.LogFilePath),"End FileTime log path not correctly identified");
                Assert.That(alr.GetLogFilePathsForMessageFileTime(lh.StartFileTime).Exists(x => x == lh.LogFilePath), "Start FileTime log path not correctly identified");
                Assert.That(alr.GetLogFilePathsForMessageFileTime((lh.StartFileTime + lh.EndFileTime)/2).Exists(x => x == lh.LogFilePath), "Middle FileTime log path not correctly identified");                
            }

        }

        [Test]
        public void GetLogFilePathsForMessageTimestamp()
        {

            OptionsStruct options = new OptionsStruct();
            options.LogDirectory = LOG_FILE_PATH;

            aaLogReader alr = new aaLogReader();

            List<LogHeader> logHeaders = alr.LogHeaderIndex;
            
            // Loop through all entries in the header index and verify we correctly identify the first, last, and middle message

            foreach (LogHeader lh in logHeaders)
            {
                Assert.That(alr.GetLogFilePathsForMessageTimestamp(lh.StartDateTime).Exists(x => x == lh.LogFilePath), "End Message Timestamp log path not correctly identified");
                Assert.That(alr.GetLogFilePathsForMessageTimestamp(lh.EndDateTime).Exists(x => x == lh.LogFilePath), "Start Mesage Timestamp log path not correctly identified");
                Assert.That(alr.GetLogFilePathsForMessageTimestamp(lh.StartDateTime.AddSeconds(lh.EndDateTime.Subtract(lh.StartDateTime).Seconds/2)).Exists(x => x == lh.LogFilePath), "Middle Message Timestamp log path not correctly identified");
            }
        }

        [Test]
        public void GetLogFilePathsForMessageNumber()
        {

            OptionsStruct options = new OptionsStruct();
            options.LogDirectory = LOG_FILE_PATH;

            aaLogReader alr = new aaLogReader();

            List<LogHeader> logHeaders = alr.LogHeaderIndex;

            // Loop through all entries in the header index and verify we correctly identify the first, last, and middle message

            foreach (LogHeader lh in logHeaders)
            {
                Assert.That(alr.GetLogFilePathsForMessageNumber(lh.StartMsgNumber).Exists(x => x == lh.LogFilePath), "End Message Number log path not correctly identified");
                Assert.That(alr.GetLogFilePathsForMessageNumber(lh.EndMsgNumber).Exists(x => x == lh.LogFilePath), "Start Mesage Number log path not correctly identified");
                Assert.That(alr.GetLogFilePathsForMessageNumber((lh.StartMsgNumber + lh.EndMsgNumber) / 2).Exists(x => x == lh.LogFilePath), "Middle Message Number log path not correctly identified");
            }

        }


    }
}