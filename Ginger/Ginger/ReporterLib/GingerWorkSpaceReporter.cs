using Amdocs.Ginger.Common;
using GingerCoreNET.ReporterLib;
using System;
using System.Windows;

namespace Ginger.ReporterLib
{
    public class GingerWorkSpaceReporter : IWorkSpaceReporter
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void ConsoleWriteLine(string message)
        {
            Console.WriteLine(message + System.Environment.NewLine);
        }

        public Amdocs.Ginger.Common.MessageBoxResult MessageBoxShow(string messageText, string caption, 
                        Amdocs.Ginger.Common.MessageBoxButton buttonsType, GingerCoreNET.ReporterLib.MessageBoxImage messageImage, 
                        Amdocs.Ginger.Common.MessageBoxResult defualtResault)
        {
            Amdocs.Ginger.Common.MessageBoxResult result = defualtResault;  // if user just close the window we return the default defined result
            App.MainWindow.Dispatcher.Invoke(() =>
            {                
                    MessageBoxWindow messageBoxWindow = new MessageBoxWindow(messageText, caption, buttonsType, messageImage, defualtResault);                    
                    messageBoxWindow.ShowDialog();
                    result = messageBoxWindow.messageBoxResult; 
            });

            return result;
        }


        // TODO: one for info one for errror !!!!!!!!!!!!!!!!!!!!!!
        public void ShowMessageToUser(string messageText)
        {                        
            MessageBoxWindow messageBoxWindow = new MessageBoxWindow(messageText, "Ginger",  Amdocs.Ginger.Common.MessageBoxButton.OK, GingerCoreNET.ReporterLib.MessageBoxImage.Information ,  Amdocs.Ginger.Common.MessageBoxResult.OK);
            messageBoxWindow.ShowDialog();            
        }


        public void ToLog(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null, bool writeAlsoToConsoleIfNeeded = true, bool writeOnlyInDebugMode = false)
        {
            try
            {
                // FIXME !!!!!!!!!!!!!!!!!!!!!!!

                //if (writeOnlyInDebugMode)
                //    if (CurrentAppLogLevel != eAppReporterLoggingLevel.Debug)
                //        return;
                switch (logLevel)
                {
                    case eLogLevel.DEBUG:
                        log.Debug(messageToLog, exceptionToLog);
                        break;
                    case eLogLevel.ERROR:
                        log.Error(messageToLog, exceptionToLog);

                        // FIXME !!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        // OnErrorReportedEvent();
                        App.MainWindow.mErrorsNum++;
                        App.MainWindow.UpdateErrorNotification();
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

                //if (writeAlsoToConsoleIfNeeded && AddAllReportingToConsole)
                // ToConsole(logLevel.ToString() + ": " + messageToLog, exceptionToLog);
                Console.WriteLine(logLevel.ToString() + ": " + messageToLog, exceptionToLog);
            }
            catch (Exception ex)
            {
                //failed to write to log
            }
        }
    }
}
