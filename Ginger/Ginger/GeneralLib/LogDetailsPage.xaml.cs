#region License
/*
Copyright © 2014-2019 European Support Limited

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

using Amdocs.Ginger.Common;
using GingerCore.GeneralLib;
using GingerCore.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ginger.GeneralLib
{
    /// <summary>
    /// Interaction logic for LogDetailsPage.xaml
    /// </summary>
    public partial class LogDetailsPage : Page
    {
        public enum eLogShowLevel
        {
            ALL, DEBUG, INFO, WARN, ERROR, FATAL
        }

        eLogShowLevel mLogLevel { get; set; }
        string mLogText;
        TextBlockHelper mTextBlockHelper;
        GenericWindow mPageGenericWin;
        string mLogFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\\amdocs\\Ginger\\WorkingFolder\\Logs\\Ginger_Log.txt";

        /// <summary>
        /// Log Details Page
        /// </summary>
        /// <param name="logLevelToShow">Selecte Log level to show</param>
        public LogDetailsPage(eLogShowLevel logLevelToShow = eLogShowLevel.ALL)
        {
            InitializeComponent();

            mLogLevel = logLevelToShow;
            GingerCore.General.FillComboFromEnumType(xLogTypeCombo, typeof(eLogShowLevel));
           
            xLogTypeCombo.SelectedValue = mLogLevel;
            xLogTypeCombo.SelectionChanged += XLogTypeCombo_SelectionChanged;

            FillLogData();
            xScrollViewer.ScrollToBottom();
        }

        private void XLogTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mLogLevel = (eLogShowLevel)xLogTypeCombo.SelectedValue;
            FillLogData();
        }

        private void FillLogData()
        {
            //get the log file text            
            using (StreamReader sr = new StreamReader(mLogFilePath))
            {
                mLogText = sr.ReadToEnd();
            }

            //cut all log not relevent to last application launch
            int indexOfStart = mLogText.LastIndexOf("######################## Application version");

            if(indexOfStart==-1)
            {
                indexOfStart = 0;
            }

            mLogText = mLogText.Substring(indexOfStart);

            //split the log per log info
            string[] logs = mLogText.Split(new[] { Environment.NewLine },StringSplitOptions.None);

            xLogDetailsTextBlock.Text = string.Empty;
            mTextBlockHelper = new TextBlockHelper(xLogDetailsTextBlock);
            bool allowLogDetailsWrite = true;
            foreach (string log in logs)
            {
                if (log == string.Empty)
                {
                    if (allowLogDetailsWrite)
                    {
                        mTextBlockHelper.AddLineBreak();
                    }
                    continue;
                }
                else if (log.Contains("#### Application version"))
                {
                    mTextBlockHelper.AddFormattedText(log, Brushes.Black, true);
                }
                else if(IsLogHeader(log))
                {
                    if (mLogLevel == eLogShowLevel.ALL || log.Contains("| " + mLogLevel.ToString()))
                    {
                        mTextBlockHelper.AddFormattedText(log, GetProperLogTypeBrush(log), isBold: true);
                        mTextBlockHelper.AddLineBreak();
                        allowLogDetailsWrite = true;
                    }
                    else
                    {
                        allowLogDetailsWrite = false;
                    }
                }
                else
                {
                    if (allowLogDetailsWrite)
                    {
                        mTextBlockHelper.AddText(log);
                        mTextBlockHelper.AddLineBreak();
                    }
                }                
            }
        }

        private bool IsLogHeader(string log)
        {
            if (log.Contains("| " + eLogLevel.DEBUG.ToString()) || log.Contains("| " + eLogLevel.ERROR.ToString()) ||
                log.Contains("| " + eLogLevel.FATAL.ToString()) || log.Contains("| " + eLogLevel.INFO.ToString()) ||
                log.Contains("| " + eLogLevel.WARN.ToString()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private Brush GetProperLogTypeBrush(string log)
        {
            if (log.Contains("| " + eLogLevel.INFO.ToString()))
            {
                return Brushes.Blue;
            }
            else if (log.Contains("| " + eLogLevel.DEBUG.ToString()))
            {
                return Brushes.Purple;
            }
            else if (log.Contains("| " + eLogLevel.WARN.ToString()))
            {
                return Brushes.Orange;
            }
            else if (log.Contains("| " + eLogLevel.ERROR.ToString()))
            {
                return Brushes.Red;
            }
            else if (log.Contains("| " + eLogLevel.FATAL.ToString()))
            {
                return Brushes.DarkRed;
            }
            else
            {
                return Brushes.Black;
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button CopyToClipboradBtn = new Button();
            CopyToClipboradBtn.Content = "Copy to Clipboard";
            CopyToClipboradBtn.Click += new RoutedEventHandler(CopyToClipboradBtn_Click);

            Button ViewLogBtn = new Button();
            ViewLogBtn.Content = "Open Full Log File";
            ViewLogBtn.Click += new RoutedEventHandler(ViewLogBtn_Click);

            Button SendMailBtn = new Button();
            SendMailBtn.Content = "Send Details in Mail";
            SendMailBtn.Click += new RoutedEventHandler(SendMailBtn_Click);

            Amdocs.Ginger.Common.ObservableList<Button> winButtons = new Amdocs.Ginger.Common.ObservableList<Button>();
            winButtons.Add(CopyToClipboradBtn);
            winButtons.Add(ViewLogBtn);
            winButtons.Add(SendMailBtn);

            this.Width = 800;
            this.Height = 600;
            GingerCore.General.LoadGenericWindow(ref mPageGenericWin, App.MainWindow, windowStyle, "Log Details", this, winButtons);
        }

        private void CopyToClipboradBtn_Click(object sender, RoutedEventArgs e)
        {            
            Clipboard.SetText(mTextBlockHelper.GetText());
        }

        private void ViewLogBtn_Click(object sender, RoutedEventArgs e)
        {
            if (System.IO.File.Exists(mLogFilePath))
            {
                Process.Start(mLogFilePath);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Ginger log file was not found.");
            }
        }

        private void SendMailBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Email mail = new Email();
                mail.EmailMethod = Email.eEmailMethod.OUTLOOK;

                mail.Subject = "Ginger Log Details";
                mail.Body = BuildMailContentAsHTML();
                mail.MailTo = "GingerCoreTeam@int.amdocs.com";

                //add Full log
                if (System.IO.File.Exists(mLogFilePath))
                {
                    mail.Attachments.Add(mLogFilePath);
                }               

                mail.DisplayAsOutlookMail();

                if (mail.Event != null && mail.Event.IndexOf("Failed") >= 0)
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Failed to send the Log Details mail." + System.Environment.NewLine + System.Environment.NewLine + "Details: " + mail.Event);
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Failed to send the Log Details mail." + System.Environment.NewLine + System.Environment.NewLine + "Details: " + ex.Message);
            }
        }

        private string BuildMailContentAsHTML()
        {
            StringBuilder html = new StringBuilder();
            html.Append(@"<html xmlns=""http://www.w3.org/1999/xhtml"">").AppendLine();
            html.Append(@"<body>").AppendLine();
            html.Append(@"<table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""font-family:arial;"">").AppendLine();
            html.Append(@"<tr><td><p>" + "Hi," + "<br><br></p></td></tr>").AppendLine();
            html.Append(@"<tr><td><p>" + "Please find my Log details below:" + "<br><br></p></td></tr>").AppendLine();
            html.Append(@"<tr><td><p>" + "<b>Version:</b> " + App.AppVersion + "<br><br></p></td></tr>").AppendLine();
            html.Append(@"<tr><td><p>" + "<b>Steps to Reproduce:</b> " + "<br><br></p></td></tr>").AppendLine();
            html.Append(@"<tr><td><p>" + "1. " + "<br></p></td></tr>").AppendLine();
            html.Append(@"<tr><td><p>" + "2. " + "<br></p></td></tr>").AppendLine();
            html.Append(@"<tr><td><p>" + "3. " + "<br><br><br></p></td></tr>").AppendLine();          
            html.Append(@"<tr><td><p>" + "<b>Log Details:</b> " + "<br></p></td></tr>").AppendLine();
            html.Append(@"<tr><td><p>" + mTextBlockHelper.GetText().Replace("\n", "<br>") + "<br></p></td></tr>").AppendLine();
            html.Append(@"</table></body></html>").AppendLine();

            return html.ToString();
        }


    }
}
