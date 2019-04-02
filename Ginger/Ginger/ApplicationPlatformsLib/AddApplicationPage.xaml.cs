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
using Ginger;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.ApplicationPlatformsLib
{
    /// <summary>
    /// Interaction logic for AddApplicationPage.xaml
    /// </summary>
    public partial class AddApplicationPage : Page
    {
        ApplicationPlatform mApplicationPlatform;

        GenericWindow genWin = null;
        public AddApplicationPage()
        {
            InitializeComponent();

            mApplicationPlatform = new ApplicationPlatform();
            NameTextBox.BindControl(mApplicationPlatform, nameof(ApplicationPlatform.AppName));
            PlatformComboBox.BindControl(mApplicationPlatform, nameof(ApplicationPlatform.Platform));
        }

        internal void ShowAsWindow(System.Windows.Window owner)
        {
            Button AddBtn = new Button();
            AddBtn.Content = "Add";
            AddBtn.Click += new RoutedEventHandler(AddButton_Click);
            ObservableList<Button> Buttons = new ObservableList<Button>();
            Buttons.Add(AddBtn);
            GenericWindow.LoadGenericWindow(ref genWin, owner, eWindowShowStyle.Free, this.Title, this, Buttons);
        }

        void AddButton_Click(object sender, RoutedEventArgs e)
        {
            WorkSpace.Instance.SolutionRepository.AddRepositoryItem(mApplicationPlatform);
            genWin.Close();
        }
    }
}