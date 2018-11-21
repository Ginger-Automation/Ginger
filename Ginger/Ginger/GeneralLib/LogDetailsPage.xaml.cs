using GingerCore;
using GingerCore.GeneralLib;
using GingerCore.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Amdocs.Ginger.Common;
using System.Text.RegularExpressions;

namespace Ginger.GeneralLib
{
    /// <summary>
    /// Interaction logic for LogDetailsPage.xaml
    /// </summary>
    public partial class LogDetailsPage : Page
    {
        string mLogText;
        TextBlockHelper mTextBlockHelper;
        GenericWindow mPageGenericWin;
        string mLogFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\\amdocs\\Ginger\\WorkingFolder\\Logs\\Ginger_Log.txt";

        public LogDetailsPage()
        {
            InitializeComponent();

            FillLogData();
            xScrollViewer.ScrollToBottom();
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
            foreach (string log in logs)
            {
                if (log == string.Empty)
                {
                    mTextBlockHelper.AddLineBreak();
                    continue;
                }
                else if (log.Contains("#### Application version"))
                {
                    mTextBlockHelper.AddHeader1(log);
                }
                else if(log.Contains("| INFO  |"))
                {
                    mTextBlockHelper.AddFormattedText(log, Brushes.Blue);
                }
                else if (log.Contains("| WARN  |"))
                {
                    mTextBlockHelper.AddFormattedText(log, Brushes.Orange);
                }
                else if(log.Contains("| ERROR |"))
                {
                    mTextBlockHelper.AddFormattedText(log, Brushes.Red, isBold:true);
                }
                else
                {
                    mTextBlockHelper.AddText(log);
                }

                mTextBlockHelper.AddLineBreak();
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
            Clipboard.SetText(mLogText);
        }

        private void ViewLogBtn_Click(object sender, RoutedEventArgs e)
        {
            if (System.IO.File.Exists(mLogFilePath))
            {
                Process.Start(mLogFilePath);
            }
            else
            {
                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Ginger log file was not found.");
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
                    Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Failed to send the Log Details mail." + System.Environment.NewLine + System.Environment.NewLine + "Details: " + mail.Event);
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Failed to send the Log Details mail." + System.Environment.NewLine + System.Environment.NewLine + "Details: " + ex.Message);
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
            html.Append(@"<tr><td><p>" + mLogText.Replace("\n", "<br>") + "<br></p></td></tr>").AppendLine();
            html.Append(@"</table></body></html>").AppendLine();

            return html.ToString();
        }
    }
}
