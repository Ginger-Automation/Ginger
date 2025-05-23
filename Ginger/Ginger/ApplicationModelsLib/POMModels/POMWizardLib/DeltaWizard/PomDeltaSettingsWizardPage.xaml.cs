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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.UIElement;
using Ginger.UserControls;
using GingerCore;
using GingerCore.GeneralLib;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.Application_Models;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.ApplicationModelsLib.POMModels.POMWizardLib
{
    /// <summary>
    /// Interaction logic for PomDeltaSettingsPage.xaml
    /// </summary>
    public partial class PomDeltaSettingsWizardPage : Page, IWizardPage
    {
        private PomDeltaWizard mWizard;
        private ePlatformType mAppPlatform;
        public bool isEnableFriendlyLocatorGrid = false;
        public PomDeltaSettingsWizardPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (PomDeltaWizard)WizardEventArgs.Wizard;
                    if (mWizard.mPomDeltaUtils.POM.TargetApplicationKey != null)
                    {
                        mAppPlatform = WorkSpace.Instance.Solution.GetTargetApplicationPlatform(mWizard.mPomDeltaUtils.POM.TargetApplicationKey);
                    }
                    if (mAppPlatform == ePlatformType.Web)
                    {
                        isEnableFriendlyLocatorGrid = true;
                    }
                    BindingHandler.ObjFieldBinding(AcceptElementsFoundByMatchersCheckBox, CheckBox.IsCheckedProperty, mWizard.mPomDeltaUtils, nameof(PomDeltaUtils.AcceptElementFoundByMatcher));
                    SetAutoMapElementTypes();
                    SetAutoMapElementTypesGridView();
                    xLearnOnlyMappedElements.BindControl(mWizard.mPomDeltaUtils.PomLearnUtils, nameof(mWizard.mPomDeltaUtils.PomLearnUtils.LearnOnlyMappedElements));
                    xLearnShadowDOMElements.BindControl(mWizard.mPomDeltaUtils.PomLearnUtils, nameof(mWizard.mPomDeltaUtils.PomLearnUtils.POM.PomSetting.LearnShadowDomElements));
                    xLearnScreenshotsOfElements.BindControl(mWizard.mPomDeltaUtils.PomLearnUtils, nameof(mWizard.mPomDeltaUtils.PomLearnUtils.POM.PomSetting.LearnScreenshotsOfElements));
                    ShowLearnScreenshotsOfElements();
                    SetElementLocatorsSettingsData();
                    SetElementLocatorsSettingsGridView();
                    ShowSpecficFrameLearnConfigPanel();
                    ShowsCustomRelativePathTemplateConfig();

                    xAvoidPropertiesAllRadioButton.IsChecked = true;
                    xKeepLocatorsOrderCheckBox.IsChecked = true;
                    break;
                case EventType.LeavingForNextPage:
                    UpdateCustomTemplateList();
                    break;
            }
        }

        private void UpdateCustomTemplateList()
        {
            if (mAppPlatform.Equals(ePlatformType.Web) || mAppPlatform.Equals(ePlatformType.Mobile))
            {
                if (xCustomRelativeXpathTemplateFrame.xCustomRelativeXpathCofigChkBox.IsChecked == true)
                {
                    mWizard.mPomDeltaUtils.POM.PomSetting.RelativeXpathTemplateList = xCustomRelativeXpathTemplateFrame.RelativeXpathTemplateList;
                }
                else
                {
                    if (mWizard.mPomDeltaUtils.POM.PomSetting != null && mWizard.mPomDeltaUtils.POM.PomSetting.RelativeXpathTemplateList != null)
                    {
                        mWizard.mPomDeltaUtils.POM.PomSetting.RelativeXpathTemplateList.Clear();
                    }

                }
            }
        }
        private void ShowsCustomRelativePathTemplateConfig()
        {
            if (mAppPlatform.Equals(ePlatformType.Web) || mAppPlatform.Equals(ePlatformType.Mobile))
            {
                xCustomRelativeXpathTemplateFrame.Visibility = Visibility.Visible;
                if (mWizard.mPomDeltaUtils.POM.PomSetting.RelativeXpathTemplateList != null && mWizard.mPomDeltaUtils.POM.PomSetting.RelativeXpathTemplateList.Count > 0)
                {
                    xCustomRelativeXpathTemplateFrame.UpdateCustomRelPathGridList(mWizard.mPomDeltaUtils.POM.PomSetting.RelativeXpathTemplateList);
                }
            }
            else
            {
                xCustomRelativeXpathTemplateFrame.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowSpecficFrameLearnConfigPanel()
        {
            if (mAppPlatform.Equals(ePlatformType.Java))
            {
                xSpecificFrameConfigPanel.Visibility = Visibility.Visible;
            }
            else
            {
                xSpecificFrameConfigPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowLearnScreenshotsOfElements()
        {
            if (mAppPlatform.Equals(ePlatformType.Web))
            {
                xLearnScreenshotsOfElements.Visibility = Visibility.Visible;
            }
            else
            {
                xLearnScreenshotsOfElements.Visibility = Visibility.Collapsed;
            }
        }

        private void SetAutoMapElementTypes()
        {
            if (mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapBasicElementTypesList.Count == 0)
            {
                var elementList = PlatformInfoBase.GetPlatformImpl(mAppPlatform).GetUIElementFilterList();
                mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapBasicElementTypesList = elementList["Basic"];
                mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapAdvanceElementTypesList = elementList["Advanced"];
            }
            mWizard.mPomDeltaUtils.PomLearnUtils.SelectElementsToList(mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapBasicElementTypesList, mWizard.mPomDeltaUtils.PomLearnUtils.POM.PomSetting.FilteredElementType);
            mWizard.mPomDeltaUtils.PomLearnUtils.SelectElementsToList(mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapAdvanceElementTypesList, mWizard.mPomDeltaUtils.PomLearnUtils.POM.PomSetting.FilteredElementType);


            xAutoMapBasicElementTypesGrid.DataSourceList = mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapBasicElementTypesList;
            xAutoMapAdvancedElementTypesGrid.DataSourceList = mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapAdvanceElementTypesList;
        }




        private void SetElementLocatorsSettingsData()
        {
            if (mWizard.mPomDeltaUtils.PomLearnUtils.POM.PomSetting != null)
            {
                mWizard.mPomDeltaUtils.PomLearnUtils.POM.PomSetting.ElementLocatorsSettingsList = PlatformInfoBase.GetPlatformImpl(mAppPlatform).GetLearningLocators();
            }
            xElementLocatorsSettingsGrid.DataSourceList = mWizard.mPomDeltaUtils.PomLearnUtils.POM.PomSetting.ElementLocatorsSettingsList;
        }

        private void SetAutoMapElementTypesGridView()
        {
            //tool bar
            xAutoMapBasicElementTypesGrid.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Check/Uncheck All Elements", new RoutedEventHandler(CheckUnCheckAllBasicElements));
            xAutoMapAdvancedElementTypesGrid.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Check/Uncheck All Elements", new RoutedEventHandler(CheckUnCheckAllAdvancedElements));

            //Set the Data Grid columns            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(UIElementFilter.Selected), Header = "To Map", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.CheckBox },
                new GridColView() { Field = nameof(UIElementFilter.ElementType), Header = "Element Type", WidthWeight = 100, ReadOnly = true },
                new GridColView() { Field = nameof(UIElementFilter.ElementExtraInfo), Header = "Element Extra Info", WidthWeight = 100, ReadOnly = true },
            ]
            };

            xAutoMapBasicElementTypesGrid.SetAllColumnsDefaultView(view);
            xAutoMapBasicElementTypesGrid.InitViewItems();

            xAutoMapAdvancedElementTypesGrid.SetAllColumnsDefaultView(view);
            xAutoMapAdvancedElementTypesGrid.InitViewItems();
        }

        private void SetElementLocatorsSettingsGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(ElementLocator.Active), WidthWeight = 8, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox },
                new GridColView() { Field = nameof(ElementLocator.LocateBy), Header = "Locate By", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.Text, ReadOnly = true },
                new GridColView() { Field = nameof(ElementLocator.EnableFriendlyLocator), Header = "Friendly Locator", Visible = isEnableFriendlyLocatorGrid, WidthWeight = 25, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox },
                new GridColView() { Field = nameof(ElementLocator.Help), WidthWeight = 25, ReadOnly = true },
            ]
            };

            xElementLocatorsSettingsGrid.SetAllColumnsDefaultView(defView);
            xElementLocatorsSettingsGrid.InitViewItems();

            xElementLocatorsSettingsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
        }

        private void CheckUnCheckAllBasicElements(object sender, RoutedEventArgs e)
        {
            if (mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapBasicElementTypesList.Count > 0)
            {
                bool valueToSet = !mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapBasicElementTypesList.All(elem => elem.Selected);

                if (mWizard.mPomDeltaUtils.PomLearnUtils.POM.PomSetting.FilteredElementType == null)
                {
                    mWizard.mPomDeltaUtils.PomLearnUtils.POM.PomSetting.FilteredElementType = [];
                }

                foreach (UIElementFilter elem in mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapBasicElementTypesList)
                {
                    elem.Selected = valueToSet;
                    if (elem.Selected)
                    {
                        if (!mWizard.mPomDeltaUtils.PomLearnUtils.POM.PomSetting.FilteredElementType.Any(x => x.ElementType.Equals(elem.ElementType)))
                        {
                            mWizard.mPomDeltaUtils.PomLearnUtils.POM.PomSetting.FilteredElementType.Add(elem);
                        }
                    }
                    else
                    {
                        mWizard.mPomDeltaUtils.PomLearnUtils.POM.PomSetting.FilteredElementType.Remove(elem);
                    }
                }
            }
        }

        private void CheckUnCheckAllAdvancedElements(object sender, RoutedEventArgs e)
        {
            if (mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapAdvanceElementTypesList.Count > 0)
            {
                bool valueToSet = !mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapAdvanceElementTypesList.All(elem => elem.Selected);

                if (mWizard.mPomDeltaUtils.PomLearnUtils.POM.PomSetting.FilteredElementType == null)
                {
                    mWizard.mPomDeltaUtils.PomLearnUtils.POM.PomSetting.FilteredElementType = [];
                }

                foreach (UIElementFilter elem in mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapAdvanceElementTypesList)
                {
                    elem.Selected = valueToSet;
                    if (elem.Selected)
                    {
                        if (!mWizard.mPomDeltaUtils.PomLearnUtils.POM.PomSetting.FilteredElementType.Any(x => x.ElementType.Equals(elem.ElementType)))
                        {
                            mWizard.mPomDeltaUtils.PomLearnUtils.POM.PomSetting.FilteredElementType.Add(elem);
                        }
                    }
                    else
                    {
                        mWizard.mPomDeltaUtils.PomLearnUtils.POM.PomSetting.FilteredElementType.Remove(elem);
                    }
                }

            }
        }

        private void xAvoidPropertiesAllRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (mWizard != null)
            {
                mWizard.mPomDeltaUtils.PropertiesChangesToAvoid = DeltaControlProperty.ePropertiesChangesToAvoid.All;
            }
        }

        private void xAvoidPropertiesOnlyVisualPropRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            mWizard.mPomDeltaUtils.PropertiesChangesToAvoid = DeltaControlProperty.ePropertiesChangesToAvoid.OnlySizeAndLocationProperties;
        }

        private void xAvoidPropertiesNoneRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            mWizard.mPomDeltaUtils.PropertiesChangesToAvoid = DeltaControlProperty.ePropertiesChangesToAvoid.None;
        }

        private void XKeepLocatorsOrderCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (mWizard != null)
            {
                mWizard.mPomDeltaUtils.KeepOriginalLocatorsOrderAndActivation = xKeepLocatorsOrderCheckBox.IsChecked;
            }
        }

        private void xLearnSpecificFrameChkBox_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToBoolean(xLearnSpecificFrameChkBox.IsChecked))
            {
                xFrameListGrid.Visibility = Visibility.Visible;
                BindWindowFrameCombox();
            }
            else
            {
                xFrameListGrid.Visibility = Visibility.Collapsed;
                mWizard.mPomDeltaUtils.SpecificFramePath = null;
            }
        }

        private void BindWindowFrameCombox()
        {
            mWizard.mPomDeltaUtils.SpecificFramePath = null;
            if (mAppPlatform.Equals(ePlatformType.Java))
            {
                var windowExplorerDriver = ((IWindowExplorer)(((AgentOperations)mWizard.mPomDeltaUtils.Agent.AgentOperations).Driver));

                var list = windowExplorerDriver.GetWindowAllFrames();
                xFrameListCmbBox.ItemsSource = list;
                xFrameListCmbBox.DisplayMemberPath = nameof(AppWindow.Title);
            }
        }

        private void xFrameListCmbBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (AppWindow)xFrameListCmbBox.SelectedItem;
            if (selectedItem != null)
            {
                mWizard.mPomDeltaUtils.SpecificFramePath = selectedItem.Path;
            }
        }

        private void xFrameRefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            BindWindowFrameCombox();
        }
    }
}
