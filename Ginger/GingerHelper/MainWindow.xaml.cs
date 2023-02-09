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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GingerHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            IgnoreExtensionsTextBox.Text= ".docx,.doc,.mp4,.db";
            SourceTextBox.Text = @"\\ilrnaGinger01\Share\Ginger Support\Help\Library Under Work";
            DestinationTextBox.Text= @"\\ilrnaGinger01\Share\Ginger Support\Help\Library";
        }

        private void SourcBrowsbtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "Select Source Folder";
            dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            if (SourceTextBox.Text != string.Empty)
                dlg.SelectedPath = SourceTextBox.Text;
            else
                dlg.SelectedPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            dlg.ShowNewFolderButton = true;           
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SourceTextBox.Text = dlg.SelectedPath;
            }
        }

        private void DestinationBrowsbtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "Select Destination Folder";
            dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            if (DestinationTextBox.Text != string.Empty)
                dlg.SelectedPath = DestinationTextBox.Text;
            else
                dlg.SelectedPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            dlg.ShowNewFolderButton = true;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DestinationTextBox.Text = dlg.SelectedPath;
            }
        }

        private void Runbtn_Click(object sender, RoutedEventArgs e)
        {
            Runbtn.Content = "Working...";
            string error = string.Empty;
            List<string> extenationsToIgnore = IgnoreExtensionsTextBox.Text.Split(new char[] { ',' },StringSplitOptions.RemoveEmptyEntries).ToList<string>();

            if (GingerHelperHandler.CreateLibrary(SourceTextBox.Text, DestinationTextBox.Text, extenationsToIgnore, DeleteExitingTargetCheckbox.IsChecked, ref error))                            
                MessageBox.Show("Library creation ended successfully!");            
            else
                MessageBox.Show("Library creation ended with error." + System.Environment.NewLine + System.Environment.NewLine + "Details: " + error);

            Runbtn.Content = "Run";
        }
    }
}
