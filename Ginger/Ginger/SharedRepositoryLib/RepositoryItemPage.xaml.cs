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

using Amdocs.Ginger.Common;
using Ginger.UserControlsLib.TextEditor.XML;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Repository
{
    /// <summary>
    /// Interaction logic for RepositoryItemPage.xaml
    /// </summary>
    public partial class RepositoryItemPage : Page
    {
        public RepositoryItemPage(string FileName)
        {
            InitializeComponent();
            if (FileName.EndsWith("xml", StringComparison.CurrentCultureIgnoreCase))
            {
                XMLTextEditor e = new XMLTextEditor();
                xmlViewer.Init(FileName, e, true);
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free, bool showSaveButton = false)
        {
            if (showSaveButton)
            {
                Button SaveButton = new Button();
                SaveButton.Content = "Save";
                SaveButton.Click += new RoutedEventHandler(Save);

                GenericWindow genWin = null;
                GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, this.Title, this, new ObservableList<Button> { SaveButton });
            }
            else
            {
                GenericWindow genWin = null;
                GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, this.Title, this);
            }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            xmlViewer.Save();
        }
    }
}
