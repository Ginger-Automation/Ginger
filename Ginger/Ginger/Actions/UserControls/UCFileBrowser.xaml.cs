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

using Amdocs.Ginger.Repository;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for UCValueExpression.xaml
    /// </summary>
    public partial class UCFileBrowser : UserControl
    {
        private object obj;
        private string AttrName;
        private bool MakePathsRelative = false;
        public List<string> FileExtensions = [];
        public UCFileBrowser()
        {
            InitializeComponent();
        }

        public void Init(object obj, string AttrName, bool mMakePathsRelative = false)
        {
            this.obj = obj;
            this.AttrName = AttrName;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(FilePathTextBox, TextBox.TextProperty, obj, AttrName);
            MakePathsRelative = mMakePathsRelative;
        }

        public void Init(ActInputValue AIV, bool mMakePathsRelative = false)
        {
            // If the VE is on stand alone form:
            MakePathsRelative = mMakePathsRelative;
            this.obj = AIV;
            this.AttrName = nameof(ActInputValue.Value);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(FilePathTextBox, TextBox.TextProperty, obj, AttrName);
        }

        private void BrowseFileButton_Click(object sender, RoutedEventArgs e)
        {
            string filter = string.Empty;
            foreach (string extension in FileExtensions)
            {
                filter = extension[1..].ToUpper() + " Files (*" + extension + ")|*" + extension;
            }
            filter += "|All Files (*.*)|*.*";
            if (General.SetupBrowseFile(new System.Windows.Forms.OpenFileDialog()
            {
                Filter = filter
            }, MakePathsRelative) is string fileName)
            {
                FilePathTextBox.Text = fileName;
            }
        }
    }
}