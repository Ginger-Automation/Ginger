﻿using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Amdocs.Ginger.CoreNET.log4netLib
{

    public class GingerLog
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static string mGingerLogFile = null;
        public static string GingerLogFile
        {
            get
            {
                if (mGingerLogFile == null)
                {                 
                    mGingerLogFile = Path.Combine(Common.GeneralLib.General.LocalUserApplicationDataFolderPath, "WorkingFolder", "Logs", "Ginger_Log.txt");
                }
                return mGingerLogFile;
            }
        }

        
        public static void InitLog4Net()
        {
          var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());            
            string xmltext = Getlog4netConfig();            
            xmltext = xmltext.Replace("${filename}", GingerLogFile);  // replace with full log path and filename
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmltext);
            XmlConfigurator.Configure(logRepository, doc.DocumentElement);            
        }

        static string Getlog4netConfig()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Amdocs.Ginger.CoreNET.log4netLib.log4net.config";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string config = reader.ReadToEnd();
                return config;
            }
        }

        public static void ToLog(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null)
        {            
            try
            {
                switch (logLevel)
                {
                    case eLogLevel.DEBUG:
                        log.Debug(messageToLog, exceptionToLog);
                        break;
                    case eLogLevel.ERROR:
                        log.Error(messageToLog, exceptionToLog);
                        break;
                    case eLogLevel.FATAL:
                        log.Fatal(messageToLog, exceptionToLog);
                        break;
                    case eLogLevel.INFO:
                        log.Info(messageToLog, exceptionToLog);
                        break;
                    case eLogLevel.WARN:
                        log.Warn(messageToLog, exceptionToLog);
                        break;
                    default:
                        log.Info(messageToLog, exceptionToLog);
                        break;
                }
            }
            catch (Exception ex)
            {
                //failed to write to log
                throw (ex);
            }
        }

        public static void PrintStartUpInfo()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("*******************************************************************************************************************").Append(Environment.NewLine);
            stringBuilder.Append("Ginger  : ").Append(Assembly.GetEntryAssembly().Location).Append(Environment.NewLine);
            stringBuilder.Append(ApplicationInfo.ApplicationName + " Started").Append(Environment.NewLine);
            stringBuilder.Append("Version : " + ApplicationInfo.ApplicationVersion).Append(Environment.NewLine);
            stringBuilder.Append("Log File: " + GingerLogFile).Append(Environment.NewLine);            
            stringBuilder.Append("*******************************************************************************************************************").Append(Environment.NewLine);
            stringBuilder.Append(Environment.NewLine);

            Reporter.ToLog(eLogLevel.INFO, stringBuilder.ToString());
        }
    }
}
