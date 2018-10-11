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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GingerCore.Actions;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActTextBoxEditPage.xaml
    /// </summary>
    public partial class ActScriptEditPage : Page
    {
        public ActionEditPage actp;
        private GingerCore.Actions.ActScript f;

        string SHFilesPath = System.IO.Path.Combine(App.UserProfile.Solution.Folder, @"Documents\Scripts\");

        
        public ActScriptEditPage(GingerCore.Actions.ActScript Act)
        {
            InitializeComponent();
            this.f = Act;
            App.FillComboFromEnumVal(ScriptActComboBox, Act.ScriptCommand);
            App.FillComboFromEnumVal(ScriptInterpreterComboBox, Act.ScriptInterpreterType);
            App.ObjFieldBinding(ScriptActComboBox, ComboBox.TextProperty, Act, ActScript.Fields.ScriptCommand);
            App.ObjFieldBinding(ScriptInterpreterComboBox, ComboBox.TextProperty, Act, ActScript.Fields.ScriptInterpreterType);
            App.ObjFieldBinding(ScriptNameComboBox, ComboBox.TextProperty, Act, ActScript.Fields.ScriptName);

            ScriptInterPreter.FileExtensions.Add(".exe");
            ScriptInterPreter.Init(Act, ActScript.Fields.ScriptInterpreter,true);
            f.ScriptPath = SHFilesPath;
        }

        private void ScriptActComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Ugly code but working, find way to make it simple use the enum val from combo            
            ActScript.eScriptAct comm = (ActScript.eScriptAct)Enum.Parse(typeof(ActScript.eScriptAct), ScriptActComboBox.SelectedItem.ToString());
            switch (comm)
            {
                case ActScript.eScriptAct.FreeCommand:
                    ScriptStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    f.RemoveAllButOneInputParam("Free Command");
                    f.AddInputValueParam("Free Command");
                    break;

                case ActScript.eScriptAct.Script:                  
                    ScriptStackPanel.Visibility = System.Windows.Visibility.Visible;
                    FillScriptNameCombo();
                    break;
            }
        }
    
        private void FillScriptNameCombo()
        {
            ScriptNameComboBox.Items.Clear();
            if (!Directory.Exists(SHFilesPath))
                Directory.CreateDirectory(SHFilesPath);
            string[] fileEntries = Directory.EnumerateFiles(SHFilesPath, "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".vbs") || s.EndsWith(".js") || s.EndsWith(".pl") || s.EndsWith(".bat") || s.EndsWith(".cmd")).ToArray();
            foreach (string file in fileEntries)
            {
                string s = file.Replace(SHFilesPath, "");
                ScriptNameComboBox.Items.Add(s);                
            }
        }

        private void ScriptNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string ScriptFile = SHFilesPath + ScriptNameComboBox.SelectedValue;
            if (!Directory.Exists(SHFilesPath))
                Directory.CreateDirectory(SHFilesPath);
            if (ScriptNameComboBox.SelectedValue == null) return;
            if (f.ScriptName != ScriptNameComboBox.SelectedValue.ToString())
            {
                f.ReturnValues.Clear();
                f.InputValues.Clear();
            
            string[] script = File.ReadAllLines(ScriptFile);
            ScriptDescriptionLabel.Content = "";           
            parseScriptHeader(script);
            }
        }

        private void parseScriptHeader(string[] script)
        {
            foreach (string line in script)
            {
                if (line.StartsWith("'GINGER_Description") || line.StartsWith("//GINGER_Description") || line.StartsWith("#GINGER_Description") || line.StartsWith("REM GINGER_Description"))
                {
                    ScriptDescriptionLabel.Content = line.Replace("'GINGER_Description", "").Replace("#GINGER_Description", "").Replace("//GINGER_Description", "").Replace("REM GINGER_Description", "");
                }
                if (line.StartsWith("'GINGER_$") || line.StartsWith("//GINGER_$") || line.StartsWith("#GINGER_$") || line.StartsWith("REM GINGER_$"))
                {
                    f.AddOrUpdateInputParamValue(line.Replace("'GINGER_$", "").Replace("#GINGER_$", "").Replace("//GINGER_$", "").Replace("REM GINGER_$", ""), "");
                }
            }
        }
        private void ScriptInterpreterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ScriptInterpreterComboBox.SelectedValue.ToString() == "Other")
            {
                InterpreterPanel.Visibility = Visibility.Visible;
            }
            else
            {
                InterpreterPanel.Visibility = Visibility.Collapsed;
            }
        }
    }
}