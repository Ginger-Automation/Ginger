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

using GingerCore.Actions;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActTextBoxEditPage.xaml
    /// </summary>
    public partial class ActConsoleCommandEditPage : Page
    {
        private ActConsoleCommand mActConsoleCommand;

        string SHFilesPath = System.IO.Path.Combine(App.UserProfile.Solution.Folder, @"Documents\sh\");        

        public ActConsoleCommandEditPage(ActConsoleCommand actConsoleCommand)
        {
            InitializeComponent();
            this.mActConsoleCommand = actConsoleCommand;
            List<object> list = GetActionListPlatform();            
            App.FillComboFromEnumVal(ConsoleActionComboBox, actConsoleCommand.ConsoleCommand, list);
            App.ObjFieldBinding(ConsoleActionComboBox, ComboBox.TextProperty, actConsoleCommand, ActConsoleCommand.Fields.ConsoleCommand);
            App.ObjFieldBinding(CommandTextBox, TextBox.TextProperty, actConsoleCommand, ActConsoleCommand.Fields.Command);
            App.ObjFieldBinding(ScriptNameComboBox, ComboBox.TextProperty, actConsoleCommand, ActConsoleCommand.Fields.ScriptName);
            App.ObjFieldBinding(txtWait, TextBox.TextProperty, actConsoleCommand, ActConsoleCommand.Fields.WaitTime);
            xDelimiterVE.BindControl(actConsoleCommand, nameof(ActConsoleCommand.Delimiter));
            txtExpected.Init(mActConsoleCommand, ActConsoleCommand.Fields.ExpString);           
        }

        private List<object> GetActionListPlatform()
        {
            List<object> actionList = new List<object>();
            string targetapp = App.BusinessFlow.CurrentActivity.TargetApplication;
            ePlatformType platform = (from x in App.UserProfile.Solution.ApplicationPlatforms where x.AppName == targetapp select x.Platform).FirstOrDefault();
            actionList.Add(ActConsoleCommand.eConsoleCommand.FreeCommand);

            if (platform == ePlatformType.Unix)
            {                
                actionList.Add(ActConsoleCommand.eConsoleCommand.ParametrizedCommand);
                actionList.Add(ActConsoleCommand.eConsoleCommand.Script);
            }
            else if(platform == ePlatformType.DOS)
            {
                actionList.Add(ActConsoleCommand.eConsoleCommand.CopyFile);
                actionList.Add(ActConsoleCommand.eConsoleCommand.IsFileExist);
            }
            return actionList;
        }
        private void ConsoleActionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ScriptStackPanel.Visibility = System.Windows.Visibility.Collapsed;
            CommandPanel.Visibility = System.Windows.Visibility.Collapsed;           
            //Ugly code but working, find way to make it simple use the enum val from combo            
            ActConsoleCommand.eConsoleCommand comm =  (ActConsoleCommand.eConsoleCommand)Enum.Parse(typeof(ActConsoleCommand.eConsoleCommand), ConsoleActionComboBox.SelectedItem.ToString());
            
            switch (comm)
            {                
                case ActConsoleCommand.eConsoleCommand.FreeCommand:                    
                    mActConsoleCommand.RemoveAllButOneInputParam("Free Command");
                    mActConsoleCommand.AddInputValueParam("Free Command");
                    mActConsoleCommand.ScriptName = "";
                    break;
                case ActConsoleCommand.eConsoleCommand.Script:
                    ScriptStackPanel.Visibility = System.Windows.Visibility.Visible;                    
                    FillScriptNameCombo();
                    break;
                case ActConsoleCommand.eConsoleCommand.ParametrizedCommand:
                    CommandPanel.Visibility = System.Windows.Visibility.Visible;
                    mActConsoleCommand.RemoveAllButOneInputParam("Value");
                    mActConsoleCommand.AddInputValueParam("Value");
                    mActConsoleCommand.ScriptName = "";
                    break;
                default:
                    mActConsoleCommand.RemoveAllButOneInputParam("Value");
                    mActConsoleCommand.AddInputValueParam("Value");
                    mActConsoleCommand.ScriptName = "";
                    break;
            }
        }

        private void FillScriptNameCombo()
        {
            ScriptNameComboBox.Items.Clear();
            if (!Directory.Exists(SHFilesPath))
                Directory.CreateDirectory(SHFilesPath);
            string[] fileEntries = Directory.GetFiles(SHFilesPath, "*.sh");
            foreach (string file in fileEntries)
            {
                string s = file.Replace(SHFilesPath, "");
                ScriptNameComboBox.Items.Add(s);                
            }
            if(mActConsoleCommand.ScriptName =="")
            {
                mActConsoleCommand.ReturnValues.Clear();
                mActConsoleCommand.InputValues.Clear();
            }
        }

        private void ScriptNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string ScriptFile = SHFilesPath + ScriptNameComboBox.SelectedValue;

            if (ScriptNameComboBox.SelectedValue == null) return;            
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
