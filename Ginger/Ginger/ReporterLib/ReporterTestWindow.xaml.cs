#region License
/*
Copyright © 2014-2023 European Support Limited

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
