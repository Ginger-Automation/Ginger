using System;
using System.IO;

namespace GingerCoreNET.GeneralLib
{
    public class Logger
    {
        StreamWriter log;

        public Logger(string fileName, bool overWrite = true)
        {
            try
            {
                if (overWrite == true)
                {
                    log = new StreamWriter(fileName);
                }
                else
                {
                    if (!File.Exists(fileName))
                    {
                        log = new StreamWriter(fileName);
                    }
                    else
                    {
                        log = File.AppendText(fileName);
                    }
                }

                // Gets or sets a value indicating whether the StreamWriter
                // will flush its buffer to the underlying stream after every 
                // call to StreamWriter.Write.
                log.AutoFlush = true;

                log.WriteLine("-------------------------------");
                log.Write("Log Entry : ");
                log.WriteLine("{0} {1}", DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString());
                log.WriteLine("-------------------------------");

            }
            catch (Exception ex)
            {
                //string errorMessage = "[" + this.GetType() + "] Error in Logger Constructor: " + Environment.NewLine + "Error= " + ex.Message;
                //App.Log(errorMessage);
            }
        }

        public void Log(string logMessage)
        {
            log.WriteLine(logMessage);
        }

        public void LogTime()
        {
            log.WriteLine("{0}", DateTime.Now.ToString("yyyyMMddHHmmss"));
        }

        public void LogTime(string logMessage)
        {
            log.WriteLine("{0} :: {1}", logMessage, DateTime.Now.ToString("yyyyMMddHHmmss"));
        }

        public void LogTimeStamp()
        {
            log.WriteLine("-------------------------------");
            log.WriteLine("{0} {1}", DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString());
            log.WriteLine("-------------------------------");
        }

        public void LogLine()
        {
            log.WriteLine("-------------------------------");
        }


    }
}
