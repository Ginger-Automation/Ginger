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
using GingerCore;
using GingerCore.Activities;
using GingerCore.GeneralLib;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.BusinessFlowPages
{
    /// <summary>
    /// Interaction logic for ActivitiesGroupSelectionPage.xaml
    /// </summary>
    public partial class ActivitiesGroupSelectionPage : Page
    {
        BusinessFlow mBusinessFlow;
        GenericWindow mPageGenericWin = null;
        ActivitiesGroup mSelectedAG = null;

        public ActivitiesGroupSelectionPage(BusinessFlow businessFlow )
        {
            InitializeComponent();

            mBusinessFlow = businessFlow;

            xAddGroupBtn.ButtonText = GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup, "New");

            xGroupComboBox.ItemsSource = mBusinessFlow.ActivitiesGroups;
            xGroupComboBox.DisplayMemberPath = nameof(ActivitiesGroup.Name);
            if (businessFlow.ActivitiesGroups.Count > 0)
            {
                xGroupComboBox.SelectedItem = businessFlow.ActivitiesGroups[0];
            }
        }

        private void XAddGroupBtn_Click(object sender, RoutedEventArgs e)
        {
            string groupName = string.Empty;
            if (InputBoxWindow.GetInputWithValidation("New Group", "New Group Name:", ref groupName))
            {
                if (!string.IsNullOrEmpty(groupName))
                {
                    if (mBusinessFlow.ActivitiesGroups.Where(x => x.Name.Trim() == groupName.Trim()).FirstOrDefault() == null)
                    {
                        ActivitiesGroup activitiesGroup = new ActivitiesGroup() { Name = groupName.Trim() };
                        mBusinessFlow.AddActivitiesGroup(activitiesGroup);
                        xGroupComboBox.SelectedItem = activitiesGroup;
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Group with same name already exist, please set unique name.");
                    }
                }
            }
        }

        public ActivitiesGroup ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool startupLocationWithOffset = false)
        {            
            string title = "Select " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup);

            ObservableList<Button> winButtons = new ObservableList<Button>();          
            Button selectBtn = new Button();
            selectBtn.Content = "Select";
            selectBtn.Click += SelectBtn_Click;
            winButtons.Add(selectBtn);

            this.Height = 200;
            this.Width = 500;

            GingerCore.General.LoadGenericWindow(ref mPageGenericWin, App.MainWindow, windowStyle, title, this, winButtons);
            return mSelectedAG;
        }

        private void SelectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (xGroupComboBox.SelectedItem != null)
            {
                mSelectedAG = (ActivitiesGroup)xGroupComboBox.SelectedItem;
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " was not selected.");
            }
            mPageGenericWin.Close();
        }
    }
}
