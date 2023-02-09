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

using System.Windows;
using System.Windows.Controls;
using GingerCore;
using GingerCore.GeneralLib;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;

namespace Ginger.Run.RunSetActions
{
    /// <summary>
    /// Interaction logic for RunSetActionHTMLReportSendEmailEditPage.xaml
    /// </summary>
    public partial class RunSetActionSendFreeEmailEditPage : Page
    {
        RunSetActionSendFreeEmail mRunSetActionSendFreeEmail;
        public RunSetActionSendFreeEmailEditPage(RunSetActionSendFreeEmail runSetActionSendFreeEmail)
        {
            mRunSetActionSendFreeEmail = runSetActionSendFreeEmail;
            InitializeComponent();
            if (mRunSetActionSendFreeEmail.Email == null)
            {
                mRunSetActionSendFreeEmail.Email = new Email();
            }

            RunsetActionDeliveryMethodConfigPageFrame.Content = new RunSetActionDeliveryMethodConfigPage(mRunSetActionSendFreeEmail.Email);
            Context context = new Context() { Environment = WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment };
            MailFromTextBox.Init(context, mRunSetActionSendFreeEmail, nameof(RunSetActionSendFreeEmail.MailFrom));
            xMailFromDisplayNameTextBox.Init(context, mRunSetActionSendFreeEmail, nameof(RunSetActionSendFreeEmail.MailFromDisplayName));
            MailToTextBox.Init(context, mRunSetActionSendFreeEmail, nameof(RunSetActionSendFreeEmail.MailTo));
            MailCCTextBox.Init(context, mRunSetActionSendFreeEmail, nameof(RunSetActionSendFreeEmail.MailCC));
            SubjectTextBox.Init(context, mRunSetActionSendFreeEmail, nameof(RunSetActionSendFreeEmail.Subject));
            BodyTextBox.Init(context, mRunSetActionSendFreeEmail, nameof(RunSetActionSendFreeEmail.Bodytext));
            BodyTextBox.AdjustHight(100);
            
            if (string.IsNullOrEmpty(mRunSetActionSendFreeEmail.MailTo))
            {
                mRunSetActionSendFreeEmail.MailFrom =  WorkSpace.Instance.UserProfile.UserEmail;
            }

            if (mRunSetActionSendFreeEmail.Email.EmailMethod == Email.eEmailMethod.SMTP)
            {
                if (string.IsNullOrEmpty(mRunSetActionSendFreeEmail.MailFromDisplayName))
                {
                    mRunSetActionSendFreeEmail.MailFromDisplayName = "_Amdocs Ginger Automation";
                }
            }
            ShowDisplayNameOption();
        }

        private void ShowDisplayNameOption()
        {
            if (mRunSetActionSendFreeEmail.Email.EmailMethod == Email.eEmailMethod.SMTP)
            {
                xLabelMailFromDisplayName.Visibility = Visibility.Visible;
                xMailFromDisplayNameTextBox.Visibility = Visibility.Visible;
            }
            else
            {
                xLabelMailFromDisplayName.Visibility = Visibility.Collapsed;
                xMailFromDisplayNameTextBox.Visibility = Visibility.Collapsed;
            }
        }

        private void Label_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowDisplayNameOption();
        }
    }
}
 