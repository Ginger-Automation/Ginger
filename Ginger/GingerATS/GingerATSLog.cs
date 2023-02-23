#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace GingerATS
{
    public enum eLogLineType
    {
        INFO, WARNING, ERROR
    }

    public class GingerATSLog
    {
        string mGingerATSLogPath;
        List<string> mGingerATSLogLines;

        public GingerATSLog(string logsFolderPath)
        {
            mGingerATSLogPath = logsFolderPath + @"\GingerATSLog.txt";
            mGingerATSLogLines = new List<string>();
        }

        public void AddLineToLog(eLogLineType logLineType, string logLineContent, Exception ex = null)
        {
            try
            {
                mGingerATSLogLines.Add(DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss") + " | " + logLineType.ToString());
                if (logLineContent != null && logLineContent != string.Empty)
                    mGingerATSLogLines.Add(logLineContent);
                if (ex != null)
                {
                    mGingerATSLogLines.Add("## Exception Details: ");
                    mGingerATSLogLines.Add("Message: " + ex.Message);
                    mGingerATSLogLines.Add("Source: " + ex.Source);
                    mGingerATSLogLines.Add("InnerException: " + ex.InnerException);
                    mGingerATSLogLines.Add("StackTrace: " + ex.StackTrace);
                }
            }
            catch (Exception)
            {
                //failed to write to log
            }
        }

        public void WriteToLogFile()
        {
            //limit the log file size to 10 MB
            try
            {
                if (File.Exists(mGingerATSLogPath))
                    if (ConvertBytesToMegabytes(new FileInfo(mGingerATSLogPath).Length) > 10)
                        File.Delete(mGingerATSLogPath);
            }
            catch (Exception) { }

            bool succeedWritingFile = false;
            int tryingCounter = 1;
            while (succeedWritingFile == false && tryingCounter <= 3)
            {
                try
                {
                    File.AppendAllLines(mGingerATSLogPath, mGingerATSLogLines);
                    succeedWritingFile = true;
                }
                catch (Exception)
                {
                    Thread.Sleep(1000);
                    tryingCounter++;
                }
            }
        }

        static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }
    }
}
