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
using Amdocs.Ginger.CoreNET;
using Ginger.UserControls;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.Actions.ActionConversion
{
    /// <summary>
    /// Interaction logic for ConversionConfigurationWzardPage.xaml
    /// </summary>
    public partial class ConversionConfigurationWzardPage : Page, IWizardPage
    {
        ActionsConversionWizard mWizard;
        ObservableList<string> TargetAppList;

        /// <summary>
        /// Constructor for configuration page
        /// </summary>
        public ConversionConfigurationWzardPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Wizard events
        /// </summary>
        /// <param name="WizardEventArgs"></param>
        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (ActionsConversionWizard)WizardEventArgs.Wizard;
                    break;
                case EventType.Active:
                    Init(WizardEventArgs);
                    break;
            }
        }

        /// <summary>
        /// This methos will set the selected POMs to the wizard object 
        /// </summary>
        private void POMSelectedEventHandler(object sender, Guid guid)
        {
            if (mWizard.SelectedPOMs != null)
            {
                mWizard.SelectedPOMs.Add(guid);
            }
        }

        /// <summary>
        /// This method is used to init the configuration settings page
        /// </summary>
        /// <param name="WizardEventArgs"></param>
        private void Init(WizardEventArgs WizardEventArgs)
        {
            xPOMSelectionPage.OwnerWindow = (Window)mWizard.mWizardWindow;
            DataContext = mWizard;
            xRadSameActivity.IsChecked = !mWizard.NewActivityChecked;
            xNewActivityRadioBtn.IsChecked = mWizard.NewActivityChecked;
            SetTargetApplicationGridView();
             
            xChkPOM.Visibility = IsPOMSupportedFromSelectedTargetApplications() ? Visibility.Visible : Visibility.Hidden;
            xChkPOM.IsChecked = mWizard.ConvertToPOMAction;
        }

        /// <summary>
        /// This method is used to check if the single target application supports POM or not?
        /// </summary>
        /// <returns></returns>
        private bool IsPOMSupportedFromSelectedTargetApplications()
        {
            bool isSupported = false;
            foreach (string targetapp in TargetAppList)
            {                
                ePlatformType platform = (from x in WorkSpace.Instance.Solution.ApplicationPlatforms where x.AppName == targetapp select x.Platform).FirstOrDefault();
                if (platform != ePlatformType.NA && PlatformInfoBase.GetPlatformImpl(platform) != null)
                {
                    isSupported = PlatformInfoBase.GetPlatformImpl(platform).IsPlatformSupportPOM();
                    if (isSupported)
                    {
                        break;
                    } 
                }
            }
            return isSupported;
        }

        /// <summary>
        /// This method is used to set the columns TargetApplciation GridView
        /// </summary>
        private void SetTargetApplicationGridView()
        {
            //Set the Data Grid columns
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            
            if (mWizard.ConvertableTargetApplications != null && mWizard.ConvertableTargetApplications.Count <= 0)
            {
                TargetAppList = new ObservableList<string>();
                mWizard.ConvertableTargetApplications = GetTargetApplication(); 
            }

            view.GridColsView.Add(new GridColView() { Field = nameof(ConvertableTargetApplicationDetails.SourceTargetApplicationName), WidthWeight = 15, ReadOnly = true, Header = "Source - Taret Application" });
            view.GridColsView.Add(new GridColView()
            {
                Field = nameof(ConvertableTargetApplicationDetails.TargetTargetApplicationName),
                BindingMode = BindingMode.TwoWay,
                StyleType = GridColView.eGridColStyleType.ComboBox,
                CellValuesList = TargetAppList,
                WidthWeight = 15,
                Header = "Map to - Target Application"
            });
            xTargetApplication.SetAllColumnsDefaultView(view);
            xTargetApplication.InitViewItems();
            xTargetApplication.DataSourceList = mWizard.ConvertableTargetApplications;
            xTargetApplication.ShowTitle = Visibility.Collapsed;
            xTargetApplication.ActiveStatus = false;
        }
        
        /// <summary>
        /// This event is used to expand the Target Application grid which helps to map the target application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ControlsViewsExpander_Expanded(object sender, RoutedEventArgs e)
        {
            if (Convert.ToString(((System.Windows.FrameworkElement)sender).Name) == xTargetApplicationExpander.Name)
            {
                xControlsViewRow.Height = new GridLength(230);
                xControlsViewRow.MaxHeight = Double.PositiveInfinity;
            }
            else
            {
                xPOMControlsViewRow.Height = new GridLength(270);
                xPOMControlsViewRow.MaxHeight = Double.PositiveInfinity;
            }
        }

        /// <summary>
        /// This event is used to collapsed the Target Application grid which helps to map the target application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ControlsViewsExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            if (Convert.ToString(((System.Windows.FrameworkElement)sender).Name) == xTargetApplicationExpander.Name)
            {
                xControlsViewRow.Height = new GridLength(35);
                xControlsViewRow.MaxHeight = 35;
            }
            else
            {
                xPOMControlsViewRow.Height = new GridLength(35);
                xPOMControlsViewRow.MaxHeight = 35;
            }
        }
        
        /// <summary>
        /// This method is used to get the Target Application
        /// </summary>
        /// <returns></returns>
        private ObservableList<ConvertableTargetApplicationDetails> GetTargetApplication()
        {
            ObservableList<ConvertableTargetApplicationDetails> lstTA = new ObservableList<ConvertableTargetApplicationDetails>();
            // fetching list of selected convertible activities from the first grid
            if (mWizard.ConversionType == ActionsConversionWizard.eActionConversionType.SingleBusinessFlow)
            {
                foreach (var targetBase in mWizard.Context.BusinessFlow.TargetApplications)
                {
                    if (!TargetAppList.Contains(targetBase.ItemName))
                    {
                        lstTA.Add(new ConvertableTargetApplicationDetails() { SourceTargetApplicationName = targetBase.ItemName, TargetTargetApplicationName = targetBase.ItemName });
                        TargetAppList.Add(targetBase.ItemName);
                    }
                }
            }
            else
            {
                foreach (var targetBase in mWizard.ListOfBusinessFlow.Where(x => x.IsSelected).SelectMany(y => y.BusinessFlow.TargetApplications))
                {
                    if (!TargetAppList.Contains(targetBase.ItemName))
                    {
                        lstTA.Add(new ConvertableTargetApplicationDetails() { SourceTargetApplicationName = targetBase.ItemName, TargetTargetApplicationName = targetBase.ItemName });
                        TargetAppList.Add(targetBase.ItemName);
                    }
                }
            }

            return lstTA;
        }

        /// <summary>
        /// This event is used to set the height and display contents based on its checked state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            mWizard.NewActivityChecked = (bool)xNewActivityRadioBtn.IsChecked;
            if ((bool)xRadSameActivity.IsChecked)
            {
                xControlsViewRow.Height = new GridLength(0);                
            }
            else
            {
                xControlsViewRow.Height = new GridLength(230);
            }
        }
    }
}
