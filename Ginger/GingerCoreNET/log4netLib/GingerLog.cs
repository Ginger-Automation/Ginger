using Amdocs.Ginger.Common;
using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
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
                 // Replace with log file name calc !!!!!!!!!!!!!!!!!!!!!!!!   
                    mGingerLogFile = Path.Combine(Common.GeneralLib.General.LocalUserApplicationDataFolderPath, "WorkingFolder", "Logs", "Ginger_Log.txt");
                }
                return mGingerLogFile;
            }
        }

        // TODO: verify it is done once !!!!!!!!!!!!!!!!!!
        public static void InitLog4Net()
        {
          var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlDocument doc = new XmlDocument();
            //  string xmltext = System.IO.File.ReadAllText(new FileInfo("log4net.config").FullName);

            // Adding 'DoNotVerify' - else for Linux it will return empty string
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify);

            if (!Directory.Exists(appdata))  // on Linux it sometimes not exist like on Azure build
            {
                Directory.CreateDirectory(appdata);
            }

            string xmltext = Getlog4netConfig();
            xmltext = xmltext.Replace("${AppData}", appdata);  // replace with full log name
            doc.LoadXml(xmltext);
            XmlConfigurator.Configure(logRepository, doc.DocumentElement);

            Console.WriteLine(">>>>>>>>>>>>>>>>>> Ginger Log File located at: " + appdata); // !!!!!!!!!!!!!!!!!!!! + ????
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

    }
}
