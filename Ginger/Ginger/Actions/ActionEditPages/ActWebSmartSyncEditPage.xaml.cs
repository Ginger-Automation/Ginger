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


using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Ginger.Actions._Common.ActUIElementLib;
using GingerCore.Actions;
using GingerCore.GeneralLib;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
namespace Ginger.Actions
{
    public partial class ActWebSmartSyncEditPage : Page
    {
        ActWebSmartSync mAction;
        string mExistingPOMAndElementGuidString = null;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the ActWebSmartSyncEditPage class.
        /// </summary>
        /// <param name="Act">The ActWebSmartSync object.</param>
        public ActWebSmartSyncEditPage(GingerCore.Actions.ActWebSmartSync Act)
        {
            mAction = Act;
            InitializeComponent();
            setLocateBycombo();
            GingerCore.General.FillComboFromEnumObj(ActionNameComboBox, Act.SyncOperations, sortValues: false);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ActionNameComboBox, ComboBox.SelectedValueProperty, Act, nameof(ActWebSmartSync.SyncOperations));
            xTxtMatchVE.Init(Context.GetAsContext(Act.Context), Act, nameof(ActWebSmartSync.TxtMatchInput));
            xAttributeValueVE.Init(Context.GetAsContext(Act.Context), Act, nameof(ActWebSmartSync.AttributeValue));
            xAttributeNameVE.Init(Context.GetAsContext(Act.Context), Act, nameof(ActWebSmartSync.AttributeName));
            xUrlMatchesVE.Init(Context.GetAsContext(Act.Context), Act, nameof(ActWebSmartSync.UrlMatches));
            xAlllocatorCheckbox.BindControl(Act, nameof(ActWebSmartSync.UseAllLocators));
            xLocateByComboBox.SelectionChanged += ElementLocateByComboBox_SelectionChanged;
            SetLocateValueFrame();
        }
        private void ElementLocateByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mAction.LocateValue = string.Empty;
            mAction.LocateValueCalculated = string.Empty;
            mAction.ElementLocateValue = string.Empty;
            SetLocateValueFrame();
            setAlllocatorCheckboxVisbility();
        }
        /// <summary>
        /// Sets the content of the LocateValueEditFrame based on the selected LocateBy type.
        /// </summary>
        private void SetLocateValueFrame()
        {
            LocateValueEditFrame.ClearAndSetContent(null);
            if (xLocateByComboBox.SelectedItem == null)
            {
                return;
            }
            eLocateBy SelectedLocType = (eLocateBy)((ComboEnumItem)xLocateByComboBox.SelectedItem).Value;
            Page p = GetLocateValueEditPage(SelectedLocType);
            LocateValueEditFrame.ClearAndSetContent(p);
        }

        /// <summary>
        /// Gets the appropriate LocateValueEditPage based on the selected LocateBy type.
        /// </summary>
        /// <param name="SelectedLocType">The selected LocateBy type.</param>
        /// <returns>The LocateValueEditPage.</returns>
        private Page GetLocateValueEditPage(eLocateBy SelectedLocType)
        {
            switch (SelectedLocType)
            {
                case eLocateBy.POMElement:
                    LocateByPOMElementPage locateByPOMElementPage = new LocateByPOMElementPage(Context.GetAsContext(mAction.Context), objectElementType: null, elementTypeFieldName: "", mAction, nameof(ActWebSmartSync.ElementLocateValue));
                    locateByPOMElementPage.ElementChangedPageEvent -= POMElementChanged;
                    locateByPOMElementPage.ElementChangedPageEvent += POMElementChanged;
                    return locateByPOMElementPage;
                default:
                    return new LocateValueEditPage(Context.GetAsContext(mAction.Context), mAction, ActWebSmartSync.Fields.ElementLocateValue);
            }
        }

        /// <summary>
        /// Event handler for the POMElementChanged event.
        /// </summary>
        private void POMElementChanged()
        {
            if (mExistingPOMAndElementGuidString != mAction.LocateValue)
            {
                mAction.AddOrUpdateInputParamValue(ActWebSmartSync.Fields.ValueToSelect, string.Empty);
            }
        }


        /// <summary>
        /// Sets the items for the LocateByComboBox.
        /// </summary>
        private void setLocateBycombo()
        {

            xLocateByComboBox.BindControl(mAction, nameof(ActWebSmartSync.ElementLocateBy), ActWebSmartSync.SupportedLocatorsTypeList, isSorted: false);
        }


        /// <summary>
        /// Gets the visibility of the locator based on the selected sync action.
        /// </summary>
        /// <returns>The visibility of the locator.</returns>
        private Visibility GetLocatorVisibility()
        {
            if (ActionNameComboBox.SelectedValue is not ActWebSmartSync.eSyncOperation selectedSyncAction)
            {
                return Visibility.Visible;
            }

            if (selectedSyncAction is ActWebSmartSync.eSyncOperation.PageHasBeenLoaded or ActWebSmartSync.eSyncOperation.AlertIsPresent or ActWebSmartSync.eSyncOperation.UrlMatches)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }
        /// <summary>
        /// Sets the visibility of the locator based on the selected sync action.
        /// </summary>
        private void locatorVisibility()
        {
            xLocaterPnl.Visibility = GetLocatorVisibility();
        }

        /// <summary>
        /// Sets the visibility of the panel based on the selected sync action.
        /// </summary>
        /// <param name="panel">The panel to set the visibility for.</param>
        /// <param name="action">The sync action to compare against.</param>
        private void SetPanelVisibility(Panel panel, ActWebSmartSync.eSyncOperation action)
        {
            if (ActionNameComboBox.SelectedValue is not ActWebSmartSync.eSyncOperation selectedSyncAction)
            {
                panel.Visibility = Visibility.Collapsed;
                return;
            }

            panel.Visibility = selectedSyncAction == action ? Visibility.Visible : Visibility.Collapsed;
        }

        private void setAlllocatorCheckboxVisbility()
        {
            if (xLocateByComboBox.SelectedValue is eLocateBy.POMElement)
            {
                //xAlllocatorCheckbox.Visibility = Visibility.Visible;
            }
            else
            {
                xAlllocatorCheckbox.Visibility = Visibility.Collapsed;
            }
        }


        /// <summary>
        /// Event handler for the ActionNameComboBox selection changed event.
        /// </summary>
        private void ActionNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ActionNameComboBox.SelectedValue is not ActWebSmartSync.eSyncOperation)
            {
                return;
            }

            locatorVisibility();
            SetPanelVisibility(TxtMatch_Pnl, ActWebSmartSync.eSyncOperation.TextMatches);
            SetPanelVisibility(UrlMatches_Pnl, ActWebSmartSync.eSyncOperation.UrlMatches);
            SetPanelVisibility(AttributeMatches_Pnl, ActWebSmartSync.eSyncOperation.AttributeMatches);
            setOperationDescription();
            setAlllocatorCheckboxVisbility();
        }

        /// <summary>
        /// Sets the operation description based on the selected sync action.
        /// </summary>
        private void setOperationDescription()
        {
            xOperationDescription.Text = ActionNameComboBox.SelectedValue switch
            {
                ActWebSmartSync.eSyncOperation.ElementIsVisible => "Verifies if an element is present on both the webpage's structure (DOM) and visible to the user.",
                ActWebSmartSync.eSyncOperation.ElementExists => "Checks if an element exists on the webpage, regardless of whether it is visible.",
                ActWebSmartSync.eSyncOperation.AlertIsPresent => "Waits until an alert message pops up on the webpage. ",
                ActWebSmartSync.eSyncOperation.ElementIsSelected => "Confirms if a form element like a checkbox or radio button is currently selected. ",
                ActWebSmartSync.eSyncOperation.PageHasBeenLoaded => "Ensures that the entire webpage has been loaded and is ready for interaction. ",
                ActWebSmartSync.eSyncOperation.ElementToBeClickable => "Waits until an element is both visible and enabled, indicating that it can be clicked. ",
                ActWebSmartSync.eSyncOperation.TextMatches => "Waits until the text of an element matches a specified pattern. Input Text is case-sensitive and does the contains search.",
                ActWebSmartSync.eSyncOperation.AttributeMatches => "Waits until a specific attribute of an element matches a specified pattern. ",
                ActWebSmartSync.eSyncOperation.EnabilityOfAllElementsLocatedBy => "Checks if all the elements found by a given locator are enabled and can be interacted. ",
                ActWebSmartSync.eSyncOperation.FrameToBeAvailableAndSwitchToIt => "Waits until a frame is available to switch to and then switches to it. ",
                ActWebSmartSync.eSyncOperation.InvisibilityOfAllElementsLocatedBy => "Waits until all the elements found by a given locator are invisible or not present. ",
                ActWebSmartSync.eSyncOperation.InvisibilityOfElementLocated => "Waits until a specific element is no longer visible or not present. ",
                ActWebSmartSync.eSyncOperation.PresenceOfAllElementsLocatedBy => "Ensures that all the elements found by a given locator are present in the webpage's structure (DOM). ",
                ActWebSmartSync.eSyncOperation.SelectedOfAllElementsLocatedBy => "Ensures that all the elements found by a given locator are selected. ",
                ActWebSmartSync.eSyncOperation.UrlMatches => "Waits until the URL of the current page matches a specified pattern. ",
                ActWebSmartSync.eSyncOperation.VisibilityOfAllElementsLocatedBy => "Ensures that all the elements found by a given locator are visible on the webpage. ",
                _ => "No operation selected.",
            };
        }

    }


}
