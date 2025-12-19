#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.CoreNET.ActionsLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActTextBoxEditPage.xaml
    /// </summary>
    public partial class ActConsoleCommandEditPage : Page
    {
        private ActConsoleCommand mActConsoleCommand;

        private Context mContext;

        string SHFilesPath = System.IO.Path.Combine(WorkSpace.Instance.Solution.Folder, @"Documents\sh\");


        public ActConsoleCommandEditPage(ActConsoleCommand actConsoleCommand)
        {
            InitializeComponent();
            this.mActConsoleCommand = actConsoleCommand;


            if (mActConsoleCommand.Context != null)
            {
                mContext = Context.GetAsContext(mActConsoleCommand.Context);
            }

            List<object> list = GetActionListPlatform();

            GingerCore.General.FillComboFromEnumObj(ConsoleActionComboBox, actConsoleCommand.ConsoleCommand, list);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ConsoleActionComboBox, ComboBox.SelectedValueProperty, actConsoleCommand, ActConsoleCommand.Fields.ConsoleCommand);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(CommandTextBox, TextBox.TextProperty, actConsoleCommand, ActConsoleCommand.Fields.Command);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ScriptNameComboBox, ComboBox.TextProperty, actConsoleCommand, ActConsoleCommand.Fields.ScriptName);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtWait, TextBox.TextProperty, actConsoleCommand, ActConsoleCommand.Fields.WaitTime);
            xDelimiterVE.BindControl(Context.GetAsContext(actConsoleCommand.Context), actConsoleCommand, nameof(ActConsoleCommand.Delimiter));
            txtExpected.Init(Context.GetAsContext(mActConsoleCommand.Context), mActConsoleCommand, ActConsoleCommand.Fields.ExpString);
            GingerCore.General.FillComboFromEnumObj(CommandTerminatorComboBox, actConsoleCommand.CommandEndKey);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(CommandTerminatorComboBox, ComboBox.SelectedValueProperty, actConsoleCommand, nameof(ActConsoleCommand.CommandEndKey));

            // Apply initial visibility according to current selected command
            if (ConsoleActionComboBox.SelectedItem != null)
            {
                UpdateVisibilityForCommand((ActConsoleCommand.eConsoleCommand)Enum.Parse(typeof(ActConsoleCommand.eConsoleCommand), ConsoleActionComboBox.SelectedValue?.ToString()));
            }
        }



        private List<object> GetActionListPlatform()
        {
            if (mActConsoleCommand.Platform == ePlatformType.NA)
            {
                if (mContext != null && mContext.BusinessFlow != null && mContext.BusinessFlow.CurrentActivity != null)
                {
                    string targetapp = mContext.BusinessFlow.CurrentActivity.TargetApplication;
                    mActConsoleCommand.Platform = WorkSpace.Instance.Solution.ApplicationPlatforms.FirstOrDefault(x => x.AppName == targetapp).Platform;
                }
                //TODO: Need to handle in generic way for all actions added to SR
                //else - if platform is NA and context is also null means we are in Shared repository
                //So the actions list will have only free command which is wrong
            }


            List<object> actionList = [ActConsoleCommand.eConsoleCommand.FreeCommand];

            if (mActConsoleCommand.Platform == ePlatformType.Unix)
            {
                actionList.Add(ActConsoleCommand.eConsoleCommand.ParametrizedCommand);
                actionList.Add(ActConsoleCommand.eConsoleCommand.Script);
                actionList.Add(ActConsoleCommand.eConsoleCommand.StartRecordingConsoleLogs);
                actionList.Add(ActConsoleCommand.eConsoleCommand.StopRecordingConsoleLogs);
                actionList.Add(ActConsoleCommand.eConsoleCommand.GetRecordedConsoleLogs);
            }
            else if (mActConsoleCommand.Platform == ePlatformType.DOS)
            {
                actionList.Add(ActConsoleCommand.eConsoleCommand.CopyFile);
                actionList.Add(ActConsoleCommand.eConsoleCommand.IsFileExist);
                actionList.Add(ActConsoleCommand.eConsoleCommand.StartRecordingConsoleLogs);
                actionList.Add(ActConsoleCommand.eConsoleCommand.StopRecordingConsoleLogs);
                actionList.Add(ActConsoleCommand.eConsoleCommand.GetRecordedConsoleLogs);
            }
            return actionList;
        }
        private void ConsoleActionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConsoleActionComboBox.SelectedItem == null)
            {
                return;
            }

            ScriptStackPanel.Visibility = Visibility.Collapsed;
            CommandPanel.Visibility = Visibility.Collapsed;

            ActConsoleCommand.eConsoleCommand comm = (ActConsoleCommand.eConsoleCommand)ConsoleActionComboBox.SelectedValue;

            switch (comm)
            {
                case ActConsoleCommand.eConsoleCommand.FreeCommand:
                    mActConsoleCommand.RemoveAllButOneInputParam("Free Command");
                    mActConsoleCommand.AddInputValueParam("Free Command");
                    mActConsoleCommand.ScriptName = string.Empty;
                    break;
                case ActConsoleCommand.eConsoleCommand.StartRecordingConsoleLogs:
                case ActConsoleCommand.eConsoleCommand.StopRecordingConsoleLogs:
                    // No params required for recording buffer start/stop
                    mActConsoleCommand.InputValues.Clear();
                    mActConsoleCommand.ScriptName = string.Empty;
                    break;
                case ActConsoleCommand.eConsoleCommand.Script:
                    ScriptStackPanel.Visibility = Visibility.Visible;
                    FillScriptNameCombo();
                    break;
                case ActConsoleCommand.eConsoleCommand.ParametrizedCommand:
                    CommandPanel.Visibility = Visibility.Visible;
                    SetupValueInputParam();
                    mActConsoleCommand.ScriptName = string.Empty;
                    break;
                default:
                    SetupValueInputParam();
                    mActConsoleCommand.ScriptName = string.Empty;
                    break;
            }

            if (ConsoleActionComboBox.SelectedItem != null)
            {
                UpdateVisibilityForCommand(comm);
            }
        }

        private void SetupValueInputParam()
        {
            mActConsoleCommand.RemoveAllButOneInputParam("Value");
            mActConsoleCommand.AddInputValueParam("Value");
        }


        private void UpdateVisibilityForCommand(ActConsoleCommand.eConsoleCommand command)
        {
            // Hide all optional controls when StartRecordingBuffer or StopRecordingBuffer selected
            bool showCommonControls = command != ActConsoleCommand.eConsoleCommand.StartRecordingConsoleLogs && command != ActConsoleCommand.eConsoleCommand.StopRecordingConsoleLogs;

            if (ExpectedStringLabel != null) { ExpectedStringLabel.Visibility = showCommonControls ? Visibility.Visible : Visibility.Collapsed; }
            if (txtExpected != null) { txtExpected.Visibility = showCommonControls ? Visibility.Visible : Visibility.Collapsed; }
            if (DelimiterLabel != null) { DelimiterLabel.Visibility = showCommonControls ? Visibility.Visible : Visibility.Collapsed; }
            if (xDelimiterVE != null) { xDelimiterVE.Visibility = showCommonControls ? Visibility.Visible : Visibility.Collapsed; }
            if (WaitTimeStackPanel != null) { WaitTimeStackPanel.Visibility = showCommonControls ? Visibility.Visible : Visibility.Collapsed; }
            if (CommandTerminatorStackPanel != null) { CommandTerminatorStackPanel.Visibility = showCommonControls ? Visibility.Visible : Visibility.Collapsed; }

            if (!showCommonControls)
            {
                mActConsoleCommand.ScriptName = string.Empty;
            }
        }

        private void CommandTerminatorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CommandTerminatorComboBox.SelectedValue is ActConsoleCommand.eCommandEndKey key)
            {
                mActConsoleCommand.CommandEndKey = key;
            }
        }
        private void FillScriptNameCombo()
        {
            ScriptNameComboBox.Items.Clear();
            if (!Directory.Exists(SHFilesPath))
            {
                Directory.CreateDirectory(SHFilesPath);
            }

            string[] fileEntries = Directory.GetFiles(SHFilesPath, "*.sh");
            foreach (string file in fileEntries)
            {
                string s = file.Replace(SHFilesPath, "");
                ScriptNameComboBox.Items.Add(s);
            }
            if (mActConsoleCommand.ScriptName == "")
            {
                mActConsoleCommand.ReturnValues.Clear();
                mActConsoleCommand.InputValues.Clear();
            }
        }

        private void ScriptNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string ScriptFile = SHFilesPath + ScriptNameComboBox.SelectedValue;

            if (ScriptNameComboBox.SelectedValue == null)
            {
                return;
            }

            if (mActConsoleCommand.ScriptName != ScriptNameComboBox.SelectedValue.ToString())
            {
                mActConsoleCommand.ReturnValues.Clear();
                mActConsoleCommand.InputValues.Clear();
                string[] script = File.ReadAllLines(ScriptFile);
                ScriptDescriptionLabel.Content = "";
                foreach (string line in script)
                {
                    if (line.StartsWith("#GINGER_Description"))
                    {
                        ScriptDescriptionLabel.Content = line.Replace("#GINGER_Description", "");
                    }
                    if (line.StartsWith("'GINGER_$") || line.StartsWith("//GINGER_$") || line.StartsWith("#GINGER_$"))
                    {
                        mActConsoleCommand.AddOrUpdateInputParamValue(line.Replace("'GINGER_$", "").Replace("#GINGER_$", "").Replace("//GINGER_$", ""), "");
                    }
                }
            }
        }
    }
}
