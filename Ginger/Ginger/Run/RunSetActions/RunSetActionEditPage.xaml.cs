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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Run.RunSetActions
{
    /// <summary>
    /// Interaction logic for RunSetActionEditPage.xaml
    /// </summary>
    public partial class RunSetActionEditPage : Page
    {
        RunSetActionBase mRunSetAction;
        public RunSetActionEditPage(RunSetActionBase RunSetAction)
        {
            InitializeComponent();
            mRunSetAction = RunSetAction;
            RunSetActionBaseOperations runSetActionBaseOperations = new RunSetActionBaseOperations(mRunSetAction);
            mRunSetAction.runSetActionBaseOperations = runSetActionBaseOperations;

            xShowIDUC.Init(mRunSetAction);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(NameTextBox, TextBox.TextProperty, RunSetAction, nameof(RunSetActionBase.Name));
            NameTextBox.AddValidationRule(new RunSetOperationNameValidationRule());
            RunAtComboBox.Init(mRunSetAction, nameof(RunSetActionBase.RunAt),mRunSetAction.GetRunOptions(), ComboBox.SelectedValueProperty);

            GingerCore.General.FillComboFromEnumObj(ConditionComboBox, RunSetAction.Condition);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ConditionComboBox, ComboBox.SelectedValueProperty, RunSetAction, nameof(RunSetActionBase.Condition));
            
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(StatusTextBox, TextBox.TextProperty, RunSetAction, nameof(RunSetActionBase.Status));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ErrorsTextBox, TextBox.TextProperty, RunSetAction, nameof(RunSetActionBase.Errors));

            //Page p = mRunSetAction.GetEditPage();
            Page p = GetEditPage(mRunSetAction.GetEditPage());

            ActionEditPageFrame.Content = p;

            if (mRunSetAction.SupportRunOnConfig)
            {
                RunActionBtn.Visibility = Visibility.Visible;
            }
        }

        public Page GetEditPage(string R)
        {
            //All runset operations are under namespace Ginger.Run.RunSetActions except ExportResultsToALMConfigPage
            //So for avoding exceptions
            string classname = null;
            if (R.ToString() == nameof(ExportResultsToALMConfigPage))
            {
                classname = "Ginger.Run." + R.ToString();
            }
            else
            {
                classname = "Ginger.Run.RunSetActions." + R.ToString();
            }
            Type t = Assembly.GetExecutingAssembly().GetType(classname);
            if (t == null)
            {
                throw new Exception("Runset edit page not found - " + classname);
            }
            Page p = (Page)Activator.CreateInstance(t, mRunSetAction);

            return p;
        }

        private void RunActionBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod == Reports.ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB || WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod == Reports.ExecutionLoggerConfiguration.DataRepositoryMethod.TextFile)
                && this.ActionEditPageFrame.Content.GetType() == typeof(RunSetActionHTMLReportSendEmailEditPage) && WorkSpace.Instance.RunsetExecutor.RunSetConfig.RunSetExecutionStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending)
            {
                Reporter.ToUser(eUserMsgKey.RunSetNotExecuted);
                return;
            }
            mRunSetAction.SolutionFolder = WorkSpace.Instance.Solution.Folder;
            mRunSetAction.runSetActionBaseOperations.ExecuteWithRunPageBFES();
        }
    }
}