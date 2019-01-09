using Amdocs.Ginger.Common;
using System;

namespace Ginger.ReporterLib
{
    public class GingerWorkSpaceReporter : WorkSpaceReporterBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public override Amdocs.Ginger.Common.eUserMsgSelection ToUser(string messageText, string caption, eUserMsgOption buttonsType, eUserMsgIcon messageImage, eUserMsgSelection defualtResault)
        {
            eUserMsgSelection result = defualtResault;  // if user just close the window we return the default defined result

            if (!App.RunningFromConfigFile)
            {
                App.MainWindow.Dispatcher.Invoke(() =>
                {
                    UserMessageBox messageBoxWindow = new UserMessageBox(messageText, caption, buttonsType, messageImage, defualtResault);
                    messageBoxWindow.ShowDialog();
                    result = messageBoxWindow.messageBoxResult;
                });
            }
            else
            {
                //not showing pop up message because running from config file and not wanting to get stuck
                ToLog(eLogLevel.WARN, string.Format("Not showing the User Message: '{0}' because application loaded in execution mode. Returning defualt selection value: '{1}'", messageText, defualtResault.ToString()));
            }

            return result;
        }

        public override void ToStatus(eStatusMsgType messageType, string statusText)
        {
            // TODO: Add icon, other info? tooltip seperate
            App.MainWindow.ShowStatus(messageType, statusText);
        }

        public override void ToLog(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null)
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
                //Console.WriteLine(logLevel.ToString() + ": " + messageToLog, exceptionToLog);
            }
            catch (Exception ex)
            {
                //failed to write to log
                throw (ex);
            }
        }
    }
}
