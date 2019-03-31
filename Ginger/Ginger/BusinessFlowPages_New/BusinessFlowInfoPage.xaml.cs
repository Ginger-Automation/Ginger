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
using GingerCore;
using GingerCore.Platforms;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.BusinessFlowsLib
{
    /// <summary>
    /// Interaction logic for BusinessFlowInfoPage.xaml
    /// </summary>
    public partial class BusinessFlowInfoPage : Page
    {
        BusinessFlow mBusinessFlow;
        public BusinessFlowInfoPage(BusinessFlow businessFlow)
        {
            InitializeComponent();

            mBusinessFlow = businessFlow;

            NameTextBox.BindControl(mBusinessFlow, nameof(BusinessFlow.Name));
            DescriptionTextBox.BindControl(mBusinessFlow, nameof(BusinessFlow.Description));

            TargetApplicationsListBox.ItemsSource = mBusinessFlow.TargetApplications; 
            TargetApplicationsListBox.DisplayMemberPath = nameof(TargetApplication.AppName);
        }
       
        public GenericWindow ShowAsWindow(System.Windows.Window owner = null)
        {
            Button saveButton = new Button();
            saveButton.Content = "Save";            
            saveButton.Click += new RoutedEventHandler(saveButton_Click);


            ObservableList<Button> Buttons = new ObservableList<Button>();
            Buttons.Add(saveButton);


            GenericWindow genWin = null;
            GenericWindow.LoadGenericWindow(ref genWin, owner, eWindowShowStyle.Free, this.Title, this, Buttons);
            return genWin;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mBusinessFlow);
        }

        private void EditTargetApplicationsButton_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}