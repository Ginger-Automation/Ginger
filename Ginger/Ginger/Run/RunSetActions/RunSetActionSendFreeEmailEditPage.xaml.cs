#region License
/*
Copyright © 2014-2020 European Support Limited

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
        public RunSetActionSendFreeEmailEditPage(RunSetActionSendFreeEmail runSetActionSendFreeEmail)
        {
            InitializeComponent();
            if (runSetActionSendFreeEmail.Email == null)
            {
                runSetActionSendFreeEmail.Email = new Email();
            }

            RunsetActionDeliveryMethodConfigPageFrame.Content = new RunSetActionDeliveryMethodConfigPage(runSetActionSendFreeEmail.Email);
            Context context = new Context() { Environment = WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment };
            MailFromTextBox.Init(context, runSetActionSendFreeEmail, nameof(RunSetActionSendFreeEmail.MailFrom));
            MailToTextBox.Init(context, runSetActionSendFreeEmail, nameof(RunSetActionSendFreeEmail.MailTo));
            MailCCTextBox.Init(context, runSetActionSendFreeEmail, nameof(RunSetActionSendFreeEmail.MailCC));
            SubjectTextBox.Init(context, runSetActionSendFreeEmail, nameof(RunSetActionSendFreeEmail.Subject));
            BodyTextBox.Init(context, runSetActionSendFreeEmail, nameof(RunSetActionSendFreeEmail.Bodytext));
            BodyTextBox.AdjustHight(100);
            if (string.IsNullOrEmpty(runSetActionSendFreeEmail.MailTo))
            {
                runSetActionSendFreeEmail.MailFrom =  WorkSpace.Instance.UserProfile.UserEmail;
            }
        }
    }
}
 