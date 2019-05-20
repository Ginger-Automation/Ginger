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

using Amdocs.Ginger.Repository;
using GingerCore.Actions.RobotFramework;
using System.Windows;
using System.Windows.Controls;
using GingerCore.GeneralLib;
using System.IO;

namespace Ginger.Actions.RobotFramework
{
    /// <summary>
    /// Interaction logic for ActRobotEditPage.xaml
    /// </summary>
    public partial class ActRobotEditPage : Page
    {
        ActRobot mAct;
        public ActRobotEditPage(ActRobot act)
        {
            InitializeComponent();
            mAct = act;           
            Bind();
            if (RobotFileTextBox.Text != string.Empty)
            {
                DisplayFileContents(RobotFileTextBox.Text);
            }
        }

        public void Bind()
        {
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(RobotLibsTextBox, TextBox.TextProperty, mAct.RobotLibraries, nameof(ActInputValue.Value));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(RobotFileTextBox, TextBox.TextProperty, mAct.RobotFileName, nameof(ActInputValue.Value));
        }
        
        private void DisplayFileContents(string fileName)
        {
            if (File.Exists(fileName))
            {
                // Read the file contents
                System.IO.StreamReader sr = new
                            System.IO.StreamReader(fileName);
                RobotFileContentsTextBox.Text = sr.ReadToEnd();
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog.Filter = "Robot Files (*.robot)|*.robot|All Files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;

            // allow the user to select only one file
            openFileDialog.Multiselect = false;

            System.Windows.Forms.DialogResult result = openFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                RobotFileTextBox.Text = openFileDialog.FileName;
                DisplayFileContents(openFileDialog.FileName);
            } // end of if

        } // end of BrowseButton_Click
    }
}