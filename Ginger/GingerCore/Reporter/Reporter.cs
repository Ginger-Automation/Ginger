#region License
/*
Copyright © 2014-2018 European Support Limited

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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using static GingerCore.GingerHelperEventArgs;

//[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace GingerCore
{
    public enum eAppLogLevel
    {
        Normal, Debug
    }

    public enum eLogLevel
    {
        DEBUG,INFO,WARN,ERROR,FATAL
    }

    public enum eMessageType
    {
        INFO,WARN,ERROR,QUESTION
    }

    public class UserMessage
    {
        public UserMessage(eMessageType MessageType, string Caption, string Message, MessageBoxButton ButtonsType, MessageBoxResult DefualtResualt)
        {
            this.MessageType = MessageType;
            this.Caption = Caption;
            this.Message = Message;            
            this.ButtonsType = ButtonsType;
            this.DefualtResualt = DefualtResualt;
        }

        public eMessageType MessageType { get; set; }
        public string Caption {get;set;}
        public string Message {get;set;}        
        public MessageBoxButton ButtonsType { get; set; }
        public MessageBoxResult DefualtResualt { get; set; }
    }

    public enum eGingerHelperMsgType
    {
        INFO, PROCESS
    }

    public class GingerHelperMsg
    {
        public GingerHelperMsg(eGingerHelperMsgType MessageType, string MsgHeader, string MsgContent, bool ShowBtn = false, string BtnContent = "")
        {
            this.MessageType = MessageType;
            this.MsgHeader = MsgHeader;
            this.MsgContent = MsgContent;
            this.ShowBtn = ShowBtn;
            this.BtnContent = BtnContent;
        }

        

        public eGingerHelperMsgType MessageType { get; set; }
        public string MsgHeader { get; set; }
        public string MsgContent { get; set; }
        public bool ShowBtn { get; set; }
        public string BtnContent { get; set; }
    }

    public class ReporterMessageEventArgs : EventArgs
    {
        string mMessage;

        public ReporterMessageEventArgs(string message)
        {
            mMessage = message;
        }

        public string MEssage { get { return mMessage; } }
    }

    public class Reporter
    {
        public static Dispatcher MainWindowDispatcher { get; set; }
        public static eAppLogLevel CurrentAppLogLevel;

        public static event ReporterEventHandler ReporterMessage;

        public class ReporterEventArgs : EventArgs
        {
            string mMessage;

            public ReporterEventArgs(string message)
            {
                mMessage = message;
            }

            public string Message { get { return mMessage; } }

        }


        public delegate void ReporterEventHandler(ReporterEventArgs e);

        public void OnMessage(string message)
        {
            ReporterEventHandler handler = ReporterMessage;
            if (handler != null)
            {
                handler(new ReporterEventArgs(message));
            }
        }

        public static void OnGingerHelperEvent(eGingerHelperEventActions EventAction, RoutedEventHandler btnHandler = null, GingerHelperMsg helperMsg=null)
        {
            GingerHelperEventHandler handler = HandlerGingerHelperEvent;
            if (handler != null)
            {
                handler(new GingerHelperEventArgs(EventAction, btnHandler, helperMsg));
            }
        }

        public delegate void GingerHelperEventHandler( GingerHelperEventArgs e);
        public static event GingerHelperEventHandler HandlerGingerHelperEvent;
        
        #region ReportToLog
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
                                        (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        
        public static void ToLog(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null, bool writeAlsoToConsoleIfNeeded=true, bool writeOnlyInDebugMode = false)
        {
            try
            {
                if (writeOnlyInDebugMode)
                    if (CurrentAppLogLevel != eAppLogLevel.Debug)                    
                        return;
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

                //if (writeAlsoToConsoleIfNeeded && AddAllReportingToConsole)
                    ToConsole(logLevel.ToString() + ": " + messageToLog, exceptionToLog);
            }
            catch (Exception)
            {
                //failed to write to log
            }
        }

        public static void ToLogAndConsole(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null)
        {
            ToLog(logLevel, messageToLog, exceptionToLog, false);
            ToConsole(messageToLog);
        }
        #endregion Report to Log

        #region Report to User
        public static Dictionary<eUserMsgKeys, UserMessage> UserMessagesPool { get; set; }
        
        public static MessageBoxResult ToUser(eUserMsgKeys messageKey, params object[] messageArgs)
        {
            UserMessage messageToShow = null;
            string messageText = string.Empty;
            MessageBoxImage messageImage = MessageBoxImage.None;

            try
            {
                //get the message from pool
                if ((UserMessagesPool != null) && UserMessagesPool.Keys.Contains(messageKey))
                    messageToShow = UserMessagesPool[messageKey];
                if (messageToShow == null)
                {
                    // We do want to pop the error message so below is just in case...
                    string mess = "";
                    foreach (object o in messageArgs)
                    {
                        mess += o.ToString() + " ";
                    }
                    MessageBox.Show(messageKey.ToString() + " - " + mess);

                    ToLog(eLogLevel.WARN, "The user message with key: '" + messageKey + "' was not found! and won't show to the user!");
                    return MessageBoxResult.None;
                }

                //set the message type
                switch (messageToShow.MessageType)
                {
                    case eMessageType.ERROR:
                        messageImage = MessageBoxImage.Error;
                        break;
                    case eMessageType.INFO:
                        messageImage = MessageBoxImage.Information;
                        break;
                    case eMessageType.QUESTION:
                        messageImage = MessageBoxImage.Question;
                        break;
                    case eMessageType.WARN:
                        messageImage = MessageBoxImage.Warning;
                        break;
                    default:
                        messageImage = MessageBoxImage.Information;
                        break;
                }

                //enter message args if exist
                if (messageArgs.Length > 0)
                    messageText = string.Format(messageToShow.Message, messageArgs);
                else
                    messageText = messageToShow.Message;

                //show the messege and return user selection
                //adding owner window to the message so it will appear on top of any other window including splash screen
                //return MessageBox.Show(messageText, messageToShow.Caption, messageToShow.ButtonsType, messageImage, messageToShow.DefualtResualt);

                if (CurrentAppLogLevel == eAppLogLevel.Debug)
                    ToLog(eLogLevel.INFO, "Showing User Message (Pop-Up): '" + messageText + "'");
                else if (AddAllReportingToConsole)
                    ToConsole("Showing User Message (Pop-Up): '" + messageText + "'");

                MessageBoxResult userSelection = MessageBoxResult.None; //????

                //Show msgbox from Main Window STA
                MainWindowDispatcher.Invoke(() => {
                    Window msgOwnerWin = new Window
                    {
                        Width = 0.001,
                        Height = 0.001,
                        Topmost = true,
                        ShowInTaskbar = false,
                        ShowActivated = true,
                        WindowStyle = WindowStyle.None,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };
                    msgOwnerWin.Show();
                    msgOwnerWin.Hide();
                    userSelection = MessageBox.Show(msgOwnerWin, messageText, messageToShow.Caption, messageToShow.ButtonsType,
                                                                                    messageImage, messageToShow.DefualtResualt);
                    msgOwnerWin.Close();

                    return userSelection;
                });


                if (CurrentAppLogLevel == eAppLogLevel.Debug)
                    ToLog(eLogLevel.INFO, "User Selection for Pop-Up Message: '" + userSelection.ToString() + "'");
                else if (AddAllReportingToConsole)
                    ToConsole("User Selection for Pop-Up Message: '" + userSelection.ToString() + "'");

                return userSelection;

            }
            catch (Exception ex)
            {
                ToLog(eLogLevel.ERROR, "Failed to show the user message with the key: " + messageKey, ex);
                MessageBox.Show("Failed to show the user message with the key: " + messageKey);
                return MessageBoxResult.None;
            }
        }
        #endregion Report to User

        #region Report to Ginger Helper
        public static Dictionary<eGingerHelperMsgKey, GingerHelperMsg> GingerHelperMsgsPool { get; set; }
        
        public static void ToGingerHelper(eGingerHelperMsgKey messageKey, RoutedEventHandler btnHandler = null, params object[] messageArgs)
        {            
            GingerHelperMsg messageToShow = null;
            string messageContent = string.Empty;
            string orgMessageContent = string.Empty;
            try
            {
                //get the message from pool
                if ((GingerHelperMsgsPool != null) && GingerHelperMsgsPool.Keys.Contains(messageKey))
                    messageToShow = GingerHelperMsgsPool[messageKey];
                if (messageToShow == null)
                {
                    // We do want to pop the error message so below is just in case...
                    string mess = "";
                    foreach (object o in messageArgs)
                    {
                        mess += o.ToString() + " ";
                    }
                    MessageBox.Show(messageKey.ToString() + " - " + mess);

                    ToLog(eLogLevel.WARN, "The Ginger Helper message with key: '" + messageKey + "' was not found! and won't show to the user!");
                }
                orgMessageContent = messageToShow.MsgContent;
                //enter message args if exist
                if (messageArgs.Length > 0)
                    messageToShow.MsgContent = string.Format(messageToShow.MsgContent, messageArgs);
               

                if (CurrentAppLogLevel == eAppLogLevel.Debug)
                    ToLog(eLogLevel.INFO, "Showing User Message (GingerHelper): " + messageContent);
                else if (AddAllReportingToConsole)
                    ToConsole("Showing User Message (GingerHelper): " + messageContent);

            
                OnGingerHelperEvent(eGingerHelperEventActions.Show, btnHandler, messageToShow);
                messageToShow.MsgContent = orgMessageContent;
            }
            catch (Exception ex)
            {                
                ToLog(eLogLevel.ERROR, "Failed to show the Ginger Helper message with the key: " + messageKey, ex);
            }
         
        }
        
        public static void CloseGingerHelper()
        {
            OnGingerHelperEvent(eGingerHelperEventActions.Close);
            if (CurrentAppLogLevel == eAppLogLevel.Debug)
                ToLog(eLogLevel.INFO, "User Message (GingerHelper) Closed.");
            else if (AddAllReportingToConsole)
                ToConsole("User Message (GingerHelper) Closed.");
        }
    #endregion Report to Ginger Helper

            #region Report to Console
        public static bool AddAllReportingToConsole = false;
        public static void ToConsole(string messageToConsole, Exception exceptionToConsole = null)
        {
            try
            {
                string msg = messageToConsole;
                if (exceptionToConsole != null)
                {
                    string excFullInfo = "Error:" + exceptionToConsole.Message + Environment.NewLine;
                    excFullInfo += "Source:" + exceptionToConsole.Source + Environment.NewLine;
                    excFullInfo += "Stack Trace: " + exceptionToConsole.StackTrace;
                    msg += System.Environment.NewLine + "Exception Details:" + System.Environment.NewLine + excFullInfo;
                }

                Console.WriteLine(msg + System.Environment.NewLine);
            }
            catch (Exception ex)
            {
                ToLog(eLogLevel.ERROR, "Failed to report to Console", ex);
            }
        }


        #endregion Report to Console


        static StringBuilder dt()
        {
            StringBuilder sb = new StringBuilder();

            //TODO: use locale
            sb.Append(DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss - "));
            return sb;
        }

        public static void ToTrace(string action, string info)
        {
            StringBuilder sb = dt();
            sb.Append(action);
            sb.Append(info);
            Trace.WriteLine(sb.ToString());
        }

        public static Stopwatch ToTraceStart(string action, string info)
        {
            Trace.WriteLine("{");
            Trace.Indent();            
            StringBuilder sb = dt();
            sb.Append(action);
            sb.Append(", ");
            sb.Append(info);
            sb.Append("*** START ***");
            Trace.WriteLine(sb.ToString());         
            Stopwatch st = new Stopwatch();
            st.Start();
            return st;
        }

        public static void ToTraceEnd(string action, string info, Stopwatch st)
        {            
            st.Stop();
            
            StringBuilder sb = dt();
            sb.Append(action);
            sb.Append(", ");
            sb.Append(info);
            sb.Append(", Elasped=");
            sb.Append(st.ElapsedMilliseconds);
            sb.Append(" *** END ***");
            Trace.WriteLine(sb.ToString());                        
            Trace.Unindent();
            Trace.WriteLine("}");
        }
    }
}
