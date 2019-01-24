﻿using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Ginger.ReporterLib
{
    /// <summary>
    /// Interaction logic for ReporterTestWindow.xaml
    /// </summary>
    public partial class ReporterTestWindow : Window
    {
        public ReporterTestWindow()
        {
            InitializeComponent();

            FillListBox();
        }

        private void FillListBox()
        {
            List<MessageInfo> messages = new List<MessageInfo>();
            Array arr = Enum.GetValues(typeof(eUserMsgKey));
            foreach (eUserMsgKey o in arr)
            {
                UserMsg mess;
                MessageInfo messageInfo = new MessageInfo();
                messageInfo.MessageKey = o;
                bool b = Reporter.UserMsgsPool.TryGetValue(o, out mess);               
                if (!b)
                {
                    messageInfo.status = "Message not found in pool";
                }
                else
                {

                    messageInfo.caption = mess.Caption;
                    messageInfo.message = mess.Message;                    
                    messageInfo.status = "OK";
                }
                
                
                
                messages.Add(messageInfo);
            }
            xDataGrid.ItemsSource = messages;
        }

        private void XSimpleInfoMessageButton_Click(object sender, RoutedEventArgs e)
        {
            Amdocs.Ginger.Common.eUserMsgSelection messageBoxResult = Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Simple Info Message");
            MessageBox.Show("You selected: " + messageBoxResult);
        }

        private void XYesNoButton_Click(object sender, RoutedEventArgs e)
        {
            Amdocs.Ginger.Common.eUserMsgSelection messageBoxResult = Reporter.ToUser(eUserMsgKey.AskIfSureWantToClose, "param1", "param2");
            MessageBox.Show("You selected: " + messageBoxResult);
        }

       

        private void XDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MessageInfo messageInfo = (MessageInfo)xDataGrid.SelectedValue;
            eUserMsgKey mess = messageInfo.MessageKey;
            Amdocs.Ginger.Common.eUserMsgSelection messageBoxResult = Reporter.ToUser(mess, "%1", "%2", "%3");
            MessageBox.Show("You selected: " + messageBoxResult);
        }
    }
}
