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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using GingerCore.Actions.Java;
using GingerCore.GeneralLib;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions.Java
{
    public partial class ActJavaEXEEditPage : Page
    {
        private ActJavaEXE mAct;

        string JarFilesPath = System.IO.Path.Combine( WorkSpace.Instance.Solution.Folder, @"Documents\Java\");

        public ActJavaEXEEditPage(ActJavaEXE act)
        {
            InitializeComponent();

            mAct = act;
            mAct.ScriptPath = JarFilesPath;

            FillScriptNameCombo();

            DoBinding();

            SetInitialLookAfterBind();

            RemoveOldInputParams();
        }

        private void DoBinding()
        {
            JavaPathTextBox.Init(Context.GetAsContext(mAct.Context), mAct, ActJavaEXE.Fields.JavaWSEXEPath);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ScriptNameComboBox, ComboBox.SelectedValueProperty, mAct, ActJavaEXE.Fields.ScriptName);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ScriptDescriptionLabel, Label.ContentProperty, mAct, ActJavaEXE.Fields.ScriptDecription);

            ScriptNameComboBox.SelectionChanged += ScriptNameComboBox_SelectionChanged;//here so won't be triggered after binding but only on user change
        }

        private void SetInitialLookAfterBind()
        {
            JavaPathHomeRdb.Content = "Use JAVA HOME Environment Variable (" + CommonLib.GetJavaHome() + ")";
            if (string.IsNullOrEmpty(mAct.JavaWSEXEPath))
                JavaPathHomeRdb.IsChecked = true;
            else
                JavaPathOtherRdb.IsChecked = true;

            if (string.IsNullOrEmpty(mAct.ScriptDecription))
                ScriptDescriptionLabel.Visibility = System.Windows.Visibility.Collapsed;
            else
                ScriptDescriptionLabel.Visibility = System.Windows.Visibility.Visible;
        }

        private void RemoveOldInputParams()
        {
            if (mAct.InputValues.Where(x => x.Param == "Value" && x.Value == string.Empty).FirstOrDefault() != null)
                mAct.RemoveInputParam("Value");
        }

        private void FillScriptNameCombo()
        {
            ScriptNameComboBox.Items.Clear();
            if (!Directory.Exists(JarFilesPath))
                Directory.CreateDirectory(JarFilesPath);
            string[] fileEntries = Directory.EnumerateFiles(JarFilesPath, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".jar")).ToArray();

            //TODO: add later support for class run if needed
            // .Where(s => s.EndsWith(".jar") || s.EndsWith(".class")).ToArray();

            foreach (string file in fileEntries)
            {
                string s = Path.GetFileName(file);
                ScriptNameComboBox.Items.Add(s);
            }
        }

        private void ScriptNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ScriptDescriptionLabel.Visibility = System.Windows.Visibility.Collapsed;

            string JarFile = System.IO.Path.Combine(JarFilesPath, ScriptNameComboBox.SelectedValue.ToString());
            if (!Directory.Exists(JarFilesPath))
                Directory.CreateDirectory(JarFilesPath);

            if (ScriptNameComboBox.SelectedValue == null)
                return;
            else
            {
                string[] a = mAct.GetParamsWithGingerHelp();               
                ParseParams(a);
            }
        }

        private void ParseParams(string[] a)
        {
            mAct.InputValues.Clear();
            foreach (string line in a)
            {
                if (line.StartsWith("GINGER_Description"))
                {
                    ScriptDescriptionLabel.Visibility = System.Windows.Visibility.Visible;
                    ScriptDescriptionLabel.Content = "Description: " + line.Replace("GINGER_Description", "");
                }

                if (line.StartsWith("GINGER_$"))
                {
                    mAct.AddOrUpdateInputParamValue(line.Replace("GINGER_$", ""), "");
                }
            }
        }

        private void JavaPathOtherRdb_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            if (JavaPathOtherRdb.IsChecked == true)
            {
                JavaPathTextBox.IsEnabled = true;
                BrowseJavaPath.IsEnabled = true;
            }
            else
            {
                if (JavaPathOtherRdb.IsVisible)
                    mAct.JavaWSEXEPath = string.Empty;
                JavaPathTextBox.IsEnabled = false;
                BrowseJavaPath.IsEnabled = false;
            }
        }

        private string OpenFolderDialog(string desc, Environment.SpecialFolder rootFolder, string currentFolder = "")
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = desc;
            dlg.RootFolder = rootFolder;
            if (currentFolder != "")
                dlg.SelectedPath = currentFolder;
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                return dlg.SelectedPath;
            }
            return null;
        }

        private void BrowseJavaPath_Click(object sender, RoutedEventArgs e)
        {
            string selectedFolder = OpenFolderDialog("Select Java Version Bin Folder", Environment.SpecialFolder.ProgramFilesX86, mAct.JavaWSEXEPath);
            if (string.IsNullOrEmpty(selectedFolder) == false)
                mAct.JavaWSEXEPath = selectedFolder;
        }
    }
}
