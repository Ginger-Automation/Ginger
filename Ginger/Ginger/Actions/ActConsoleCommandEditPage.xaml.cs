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
using System.IO;
using System.Windows.Controls;
using GingerCore.Actions;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActTextBoxEditPage.xaml
    /// </summary>
    public partial class ActConsoleCommandEditPage : Page
    {
        private GingerCore.Actions.ActConsoleCommand f;

        string SHFilesPath = System.IO.Path.Combine(App.UserProfile.Solution.Folder, @"Documents\sh\");        

        public ActConsoleCommandEditPage(GingerCore.Actions.ActConsoleCommand Act)
        {
            InitializeComponent();
            this.f = Act;
            List<object> list = GetActionListPlatform();            
            App.FillComboFromEnumVal(ConsoleActionComboBox, Act.ConsoleCommand, list);
            App.ObjFieldBinding(ConsoleActionComboBox, ComboBox.TextProperty, Act, ActConsoleCommand.Fields.ConsoleCommand);
            App.ObjFieldBinding(CommandTextBox, TextBox.TextProperty, Act, ActConsoleCommand.Fields.Command);
            App.ObjFieldBinding(ScriptNameComboBox, ComboBox.TextProperty, Act, ActConsoleCommand.Fields.ScriptName);
            App.ObjFieldBinding(txtWait, TextBox.TextProperty, Act, ActConsoleCommand.Fields.WaitTime);
            txtExpected.Init(f, ActConsoleCommand.Fields.ExpString);           
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
                    f.RemoveAllButOneInputParam("Free Command");
                    f.AddInputValueParam("Free Command");
                    f.ScriptName = "";
                    break;
                case ActConsoleCommand.eConsoleCommand.Script:
                    ScriptStackPanel.Visibility = System.Windows.Visibility.Visible;                    
                    FillScriptNameCombo();
                    break;
                case ActConsoleCommand.eConsoleCommand.ParametrizedCommand:
                    CommandPanel.Visibility = System.Windows.Visibility.Visible;
                    f.RemoveAllButOneInputParam("Value");
                    f.AddInputValueParam("Value");
                    f.ScriptName = "";
                    break;
                default:
                    f.RemoveAllButOneInputParam("Value");
                    f.AddInputValueParam("Value");
                    f.ScriptName = "";
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
            if(f.ScriptName =="")
            {
                f.ReturnValues.Clear();
                f.InputValues.Clear();
            }
        }

        private void ScriptNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string ScriptFile = SHFilesPath + ScriptNameComboBox.SelectedValue;

            if (ScriptNameComboBox.SelectedValue == null) return;            
            if (f.ScriptName != ScriptNameComboBox.SelectedValue.ToString())
            {
                f.ReturnValues.Clear();
                f.InputValues.Clear();
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
                        f.AddOrUpdateInputParamValue(line.Replace("'GINGER_$", "").Replace("#GINGER_$", "").Replace("//GINGER_$", ""), "");
                    }
                }
            }
        }
    }
}
