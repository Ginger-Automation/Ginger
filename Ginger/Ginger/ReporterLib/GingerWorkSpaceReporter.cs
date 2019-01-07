using Amdocs.Ginger.Common;
using GingerCore;
using GingerCoreNET.ReporterLib;
using System;
using System.Windows;

namespace Ginger.ReporterLib
{
    public class GingerWorkSpaceReporter : WorkSpaceReporterBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public override Amdocs.Ginger.Common.eUserMsgSelection ToUser(string messageText, string caption, 
                        Amdocs.Ginger.Common.eUserMsgOption buttonsType, eUserMsgIcon messageImage, 
                        Amdocs.Ginger.Common.eUserMsgSelection defualtResault)
        {
            Amdocs.Ginger.Common.eUserMsgSelection result = defualtResault;  // if user just close the window we return the default defined result
            App.MainWindow.Dispatcher.Invoke(() =>
            {                
                    MessageBoxWindow messageBoxWindow = new MessageBoxWindow(messageText, caption, buttonsType, messageImage, defualtResault);                    
                    messageBoxWindow.ShowDialog();
                    result = messageBoxWindow.messageBoxResult; 
            });

            return result;
        }

        public override void ToStatus(eStatusMsgType messageType, string statusText)
        {
            // TODO: Add icon, other info? tooltip seperate
            App.MainWindow.ShowStatus(messageType, statusText);
        }

        public override void ToLog(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null, bool writeAlsoToConsoleIfNeeded = true, bool writeOnlyInDebugMode = false)
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
                Console.WriteLine(logLevel.ToString() + ": " + messageToLog, exceptionToLog);
            }
            catch (Exception ex)
            {
                // TODO: throw?
                //failed to write to log
            }
        }
    }
}
