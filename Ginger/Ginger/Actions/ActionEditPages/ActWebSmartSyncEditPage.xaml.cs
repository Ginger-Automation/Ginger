#region License
/*
Copyright © 2014-2024 European Support Limited

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
using GingerCore.Actions;
using GingerCore.GeneralLib;
using Ginger;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Ginger.UserControls;
namespace Ginger.Actions
{
    public partial class ActWebSmartSyncEditPage : Page, IActEditPage
    {
        ActWebSmartSync mAct;
        public Visibility LocatorVisibility
        {
            get => GetLocatorVisibility();
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        public ActWebSmartSyncEditPage(GingerCore.Actions.ActWebSmartSync Act)
        {
            InitializeComponent();     
            GingerCore.General.FillComboFromEnumObj(ActionNameComboBox, Act.SyncOperations, sortValues: false);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ActionNameComboBox, ComboBox.SelectedValueProperty, Act, nameof(ActWebSmartSync.SyncOperations));
            xTxtMatchVE.Init(Context.GetAsContext(Act.Context), Act, nameof(ActWebSmartSync.TxtMatchInput));
            xAttributeValueVE.Init(Context.GetAsContext(Act.Context), Act, nameof(ActWebSmartSync.AttributeValue));
            xAttributeNameVE.Init(Context.GetAsContext(Act.Context), Act, nameof(ActWebSmartSync.AttributeName));
            xUrlMatchesVE.Init(Context.GetAsContext(Act.Context), Act, nameof(ActWebSmartSync.UrlMatches));


        }

     

        private Visibility GetLocatorVisibility()
        {
            if (ActionNameComboBox.SelectedValue is not ActWebSmartSync.eSyncOperation selectedSyncAction)
            {
                return Visibility.Visible;
            }

            if (selectedSyncAction == ActWebSmartSync.eSyncOperation.PageHasBeenLoaded || selectedSyncAction == ActWebSmartSync.eSyncOperation.AlertIsPresent || selectedSyncAction == ActWebSmartSync.eSyncOperation.UrlMatches)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }
     
        private void SetPanelVisibility(Panel panel, ActWebSmartSync.eSyncOperation action)
        {
            if (ActionNameComboBox.SelectedValue is not ActWebSmartSync.eSyncOperation selectedSyncAction)
            {
                panel.Visibility = Visibility.Collapsed;
                return;
            }

            panel.Visibility = selectedSyncAction == action ? Visibility.Visible : Visibility.Collapsed;
        }

        private void NotifyLocatorVisibilityChanged()
        {
            PropertyChanged?.Invoke(sender: this, new PropertyChangedEventArgs(propertyName: nameof(LocatorVisibility)));
        }

        private void ActionNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ActionNameComboBox.SelectedValue is not ActWebSmartSync.eSyncOperation)
            {
                return;
            }

            NotifyLocatorVisibilityChanged();
            SetPanelVisibility(TxtMatch_Pnl, ActWebSmartSync.eSyncOperation.TextMatches);
            SetPanelVisibility(UrlMatches_Pnl, ActWebSmartSync.eSyncOperation.UrlMatches);
            SetPanelVisibility(AttributeMatches_Pnl, ActWebSmartSync.eSyncOperation.AttributeMatches);
            setOperationDescription();
        }

        void setOperationDescription()
        {
            switch (ActionNameComboBox.SelectedValue)
            {
                case ActWebSmartSync.eSyncOperation.ElementIsVisible:
                    xOperationDescription.Text = "Verifies if an element is present on both the webpage's structure (DOM) and visible to the user.";
                    break;
                case ActWebSmartSync.eSyncOperation.ElementExists:
                    xOperationDescription.Text = "Checks if an element exists on the webpage, regardless of whether it is visible.";
                    break;
                case ActWebSmartSync.eSyncOperation.AlertIsPresent:
                    xOperationDescription.Text = "Waits until an alert message pops up on the webpage. ";
                    break;
                case ActWebSmartSync.eSyncOperation.ElementIsSelected:
                    xOperationDescription.Text = "Confirms if a form element like a checkbox or radio button is currently selected. ";
                    break;
                case ActWebSmartSync.eSyncOperation.PageHasBeenLoaded:
                    xOperationDescription.Text = "Ensures that the entire webpage has been loaded and is ready for interaction. ";
                    break;
                case ActWebSmartSync.eSyncOperation.ElementToBeClickable:
                    xOperationDescription.Text = "Waits until an element is both visible and enabled, indicating that it can be clicked. ";
                    break;
                case ActWebSmartSync.eSyncOperation.TextMatches:
                    xOperationDescription.Text = "Waits until the text of an element matches a specified pattern. Input Text is case-sensitive and does the contains search.";
                    break;
                case ActWebSmartSync.eSyncOperation.AttributeMatches:
                    xOperationDescription.Text = "Waits until a specific attribute of an element matches a specified pattern. ";
                    break;
                case ActWebSmartSync.eSyncOperation.EnabilityOfAllElementsLocatedBy:
                    xOperationDescription.Text = "Checks if all the elements found by a given locator are enabled and can be interacted. ";
                    break;
                case ActWebSmartSync.eSyncOperation.FrameToBeAvailableAndSwitchToIt:
                    xOperationDescription.Text = "Waits until a frame is available to switch to and then switches to it. ";
                    break;
                case ActWebSmartSync.eSyncOperation.InvisibilityOfAllElementsLocatedBy:
                    xOperationDescription.Text = "Waits until all the elements found by a given locator are invisible or not present. ";
                    break;
                case ActWebSmartSync.eSyncOperation.InvisibilityOfElementLocated:
                    xOperationDescription.Text = "Waits until a specific element is no longer visible or not present. ";
                    break;
                case ActWebSmartSync.eSyncOperation.PresenceOfAllElementsLocatedBy:
                    xOperationDescription.Text = "Ensures that all the elements found by a given locator are present in the webpage's structure (DOM). ";
                    break;
                case ActWebSmartSync.eSyncOperation.SelectedOfAllElementsLocatedBy:
                    xOperationDescription.Text = "Ensures that all the elements found by a given locator are selected. ";
                    break;
                case ActWebSmartSync.eSyncOperation.UrlMatches:
                    xOperationDescription.Text = "Waits until the URL of the current page matches a specified pattern. ";
                    break;
                case ActWebSmartSync.eSyncOperation.VisibilityOfAllElementsLocatedBy:
                    xOperationDescription.Text = "Ensures that all the elements found by a given locator are visible on the webpage. ";
                    break;
                default:
                    xOperationDescription.Text = "No operation selected.";
                    break;


            }

        }
        
    }

    public interface IActEditPage : INotifyPropertyChanged
    {
        public Visibility LocatorVisibility { get; }
    }
}
