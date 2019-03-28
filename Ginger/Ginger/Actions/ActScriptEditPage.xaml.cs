#region License
/*
Copyright © 2014-2019 European Support Limited

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
using amdocs.ginger.GingerCoreNET;
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

        string SHFilesPath = System.IO.Path.Combine( WorkSpace.Instance.Solution.Folder, @"Documents\Scripts\");


        public ActScriptEditPage(GingerCore.Actions.ActScript Act)
        {
            InitializeComponent();
            this.f = Act;
            GingerCore.General.FillComboFromEnumObj(ScriptActComboBox, Act.ScriptCommand);
            GingerCore.General.FillComboFromEnumObj(ScriptInterpreterComboBox, Act.ScriptInterpreterType);
         
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ScriptInterpreterComboBox, ComboBox.SelectedValueProperty, Act, ActScript.Fields.ScriptInterpreterType);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ScriptActComboBox, ComboBox.SelectedValueProperty, Act, ActScript.Fields.ScriptCommand);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ScriptNameComboBox, ComboBox.SelectedValueProperty, Act, ActScript.Fields.ScriptName);
           
            ScriptNameComboBox.SelectionChanged += ScriptNameComboBox_SelectionChanged;
           
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
                    ScriptDescriptionPanel.Visibility = Visibility.Collapsed;
                    f.RemoveAllButOneInputParam("Free Command");
                    f.AddInputValueParam("Free Command");
                    ScriptNameComboBox.SelectedItem = null;
                    break;

                case ActScript.eScriptAct.Script:
                    ScriptStackPanel.Visibility = System.Windows.Visibility.Visible;
                    f.RemoveInputParam("Free Command");
                    break;
            }
        } 
          
        private void ScriptNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (ScriptNameComboBox.SelectedItem != null)
            {
                string ScriptFile = SHFilesPath + ScriptNameComboBox.SelectedItem;
                if (!Directory.Exists(SHFilesPath))
                    Directory.CreateDirectory(SHFilesPath);
                f.ReturnValues.Clear();
                f.InputValues.Clear();

                string[] script = File.ReadAllLines(ScriptFile);
                ScriptDescriptionContent.Content = "";
                parseScriptHeader(script);
            }

            if (f.InputValues.Count == 0)
            {
                f.AddInputValueParam("Value");
            }
        }      

        private void parseScriptHeader(string[] script)
        {
            foreach (string line in script)
            {
                if (line.StartsWith("'GINGER_Description") || line.StartsWith("//GINGER_Description") || line.StartsWith("#GINGER_Description") || line.StartsWith("REM GINGER_Description"))
                {
                    ScriptDescriptionContent.Content = line.Replace("'GINGER_Description", "").Replace("#GINGER_Description", "").Replace("//GINGER_Description", "").Replace("REM GINGER_Description", "");
                }
                if (line.StartsWith("'GINGER_$") || line.StartsWith("//GINGER_$") || line.StartsWith("#GINGER_$") || line.StartsWith("REM GINGER_$"))
                {
                    f.AddOrUpdateInputParamValue(line.Replace("'GINGER_$", "").Replace("#GINGER_$", "").Replace("//GINGER_$", "").Replace("REM GINGER_$", ""), "");
                }
            }
            if(String.IsNullOrEmpty(ScriptDescriptionContent.Content.ToString()))
            {
                ScriptDescriptionPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                ScriptDescriptionPanel.Visibility = Visibility.Visible;
            }
        }

        private void ScriptInterpreterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActScript.eScriptInterpreterType interpreterType;
            Enum.TryParse(ScriptInterpreterComboBox.SelectedValue.ToString(), out interpreterType);

            string[] fileEntries = null;

            if (!Directory.Exists(SHFilesPath))
            {
                Directory.CreateDirectory(SHFilesPath);
            }
            
            if (interpreterType == ActScript.eScriptInterpreterType.Other)
            {
                InterpreterPathPanel.Visibility = Visibility.Visible;
                fileEntries = Directory.EnumerateFiles(SHFilesPath, "*.*", SearchOption.AllDirectories)
               .Where(s => s.ToLower().EndsWith(".vbs") || s.ToLower().EndsWith(".js") || s.ToLower().EndsWith(".pl") || s.ToLower().EndsWith(".bat") || s.ToLower().EndsWith(".cmd")).ToArray();
            }
            else if (interpreterType == ActScript.eScriptInterpreterType.BAT)
            {
                InterpreterPathPanel.Visibility = Visibility.Collapsed;              
                fileEntries = GingerCore.General.ReturnFilesWithDesiredExtension(SHFilesPath, ".bat");
            }
            else if (interpreterType == ActScript.eScriptInterpreterType.VBS)
            {
                InterpreterPathPanel.Visibility = Visibility.Collapsed;
                fileEntries = GingerCore.General.ReturnFilesWithDesiredExtension(SHFilesPath, ".vbs");
            }
            else if (interpreterType == ActScript.eScriptInterpreterType.JS)
            {
                InterpreterPathPanel.Visibility = Visibility.Collapsed;
                fileEntries = GingerCore.General.ReturnFilesWithDesiredExtension(SHFilesPath, ".js");
            }

            if (fileEntries != null)
            {
                fileEntries = fileEntries.Select(q => q.Replace(SHFilesPath, "")).ToArray();
                ScriptNameComboBox.ItemsSource = fileEntries;

                if (f.ScriptName == null)
                {
                    ScriptNameComboBox.SelectedValue = fileEntries.FirstOrDefault();
                }
            }
        }

      
    }


}


