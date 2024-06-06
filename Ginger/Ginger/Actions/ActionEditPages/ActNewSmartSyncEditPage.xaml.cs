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


using GingerCore.Actions;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.Actions
{
    public partial class ActNewSmartSyncEditPage : Page, IActEditPage
    {
        public Visibility LocatorVisibility
        {
            get => GetLocatorVisibility();
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        public ActNewSmartSyncEditPage(GingerCore.Actions.ActNewSmartSync Act)
        {
            InitializeComponent();

            GingerCore.General.FillComboFromEnumObj(ActionNameComboBox, Act.SyncOperations, sortValues: false);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ActionNameComboBox, ComboBox.SelectedValueProperty, Act, nameof(ActNewSmartSync.SyncOperations));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(TxtMatchTextbox, TextBox.TextProperty, Act, nameof(ActNewSmartSync.TxtMatchInput));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(AttributeValueTextbox, TextBox.TextProperty, Act, nameof(ActNewSmartSync.AttributeValue));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(AttributeNameTextbox, TextBox.TextProperty, Act, nameof(ActNewSmartSync.AttributeName));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(UrlMatchesTextbox, TextBox.TextProperty, Act, nameof(ActNewSmartSync.UrlMatches));
        }

       

        private Visibility GetLocatorVisibility()
        {
            if (ActionNameComboBox.SelectedValue is not ActNewSmartSync.eSyncOperation selectedSyncAction)
            {
                return Visibility.Visible;
            }

            if (selectedSyncAction == ActNewSmartSync.eSyncOperation.PageHasBeenLoaded || selectedSyncAction == ActNewSmartSync.eSyncOperation.AlertIsPresent || selectedSyncAction == ActNewSmartSync.eSyncOperation.UrlMatches)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }
       // eSyncOperation operation = EnumHelper.GetEnumValueFromDescription<eSyncOperation>(description);

        private void SetPanelVisibility(Panel panel, ActNewSmartSync.eSyncOperation action)
        {
            if (ActionNameComboBox.SelectedValue is not ActNewSmartSync.eSyncOperation selectedSyncAction)
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
            if (ActionNameComboBox.SelectedValue is not ActNewSmartSync.eSyncOperation)
            {
                return;
            }

            NotifyLocatorVisibilityChanged();
            SetPanelVisibility(TxtMatch_Pnl, ActNewSmartSync.eSyncOperation.TextMatches);
            SetPanelVisibility(UrlMatches_Pnl, ActNewSmartSync.eSyncOperation.UrlMatches);
            SetPanelVisibility(AttributeMatches_Pnl, ActNewSmartSync.eSyncOperation.AttributeMatches);
            setOperationDescription();
        }

        void setOperationDescription()
        {
            switch (ActionNameComboBox.SelectedValue)
            {
                case ActNewSmartSync.eSyncOperation.ElementIsVisible:
                    xOperationDescription.Text = "Verifies if an element is present on both the webpage's structure (DOM) and visible to the user.";
                    break;
                case ActNewSmartSync.eSyncOperation.ElementExists:
                    xOperationDescription.Text = "Checks if an element exists on the webpage, regardless of whether it is visible.";
                    break;
                case ActNewSmartSync.eSyncOperation.AlertIsPresent:
                    xOperationDescription.Text = "Waits until an alert message pops up on the webpage. ";
                    break;
                case ActNewSmartSync.eSyncOperation.ElementIsSelected:
                    xOperationDescription.Text = "Confirms if a form element like a checkbox or radio button is currently selected. ";
                    break;
                case ActNewSmartSync.eSyncOperation.PageHasBeenLoaded:
                    xOperationDescription.Text = "Ensures that the entire webpage has been loaded and is ready for interaction. ";
                    break;
                case ActNewSmartSync.eSyncOperation.ElementToBeClickable:
                    xOperationDescription.Text = "Waits until an element is both visible and enabled, indicating that it can be clicked. ";
                    break;
                case ActNewSmartSync.eSyncOperation.TextMatches:
                    xOperationDescription.Text = "Waits until the text of an element matches a specified pattern. ";
                    break;
                case ActNewSmartSync.eSyncOperation.AttributeMatches:
                    xOperationDescription.Text = "Waits until a specific attribute of an element matches a specified pattern. ";
                    break;
                case ActNewSmartSync.eSyncOperation.EnabilityOfAllElementsLocatedBy:
                    xOperationDescription.Text = "Checks if all the elements found by a given method are enabled and can be interacted. ";
                    break;
                case ActNewSmartSync.eSyncOperation.FrameToBeAvailableAndSwitchToIt:
                    xOperationDescription.Text = "Waits until a frame is available to switch to and then switches to it. ";
                    break;
                case ActNewSmartSync.eSyncOperation.InvisibilityOfAllElementsLocatedBy:
                    xOperationDescription.Text = "Waits until all the elements found by a given method are invisible or not present. ";
                    break;
                case ActNewSmartSync.eSyncOperation.InvisibilityOfElementLocated:
                    xOperationDescription.Text = "Waits until a specific element is no longer visible or not present. ";
                    break;
                case ActNewSmartSync.eSyncOperation.PresenceOfAllElementsLocatedBy:
                    xOperationDescription.Text = "Ensures that all the elements found by a given method are present in the webpage's structure (DOM). ";
                    break;
                case ActNewSmartSync.eSyncOperation.SelectedOfAllElementsLocatedBy:
                    xOperationDescription.Text = "Ensures that all the elements found by a given method are visible on the webpage. ";
                    break;
                case ActNewSmartSync.eSyncOperation.UrlMatches:
                    xOperationDescription.Text = "Waits until the URL of the current page matches a specified pattern. ";
                    break;
                case ActNewSmartSync.eSyncOperation.VisibilityOfAllElementsLocatedBy:
                    xOperationDescription.Text = "Ensures that all the elements found by a given method are selected. ";
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
