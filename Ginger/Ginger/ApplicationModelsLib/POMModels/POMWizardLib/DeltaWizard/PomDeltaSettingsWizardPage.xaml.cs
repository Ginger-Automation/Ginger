#region License
/*
Copyright © 2014-2021 European Support Limited

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
using Amdocs.Ginger.Common.UIElement;
using Ginger.UserControls;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.Application_Models;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
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

                    SetAutoMapElementTypes();                    
                    SetAutoMapElementTypesGridView();
                    xLearnOnlyMappedElements.BindControl(mWizard.mPomDeltaUtils.PomLearnUtils, nameof(mWizard.mPomDeltaUtils.PomLearnUtils.LearnOnlyMappedElements));
                    SetElementLocatorsSettingsData();                    
                    SetElementLocatorsSettingsGridView();
                    ShowSpecficFrameLearnConfigPanel();

                    xAvoidPropertiesAllRadioButton.IsChecked = true;
                    xKeepLocatorsOrderCheckBox.IsChecked = true;
                    break;
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

        private void SetAutoMapElementTypes()
        {
            if (mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapBasicElementTypesList.Count == 0)
            {
                switch (mAppPlatform)
                {
                    case ePlatformType.Web:
                        SetPlatformAutoMapElements(new WebPlatform().GetPlatformElementTypesData().ToList());
                        break;
                    case ePlatformType.Java:
                        var elementList = new JavaPlatform().GetUIElementFilterList();
                        mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapBasicElementTypesList = elementList["Basic"];
                        mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapAdvanceElementTypesList = elementList["Advanced"];
                        break;
                }
            }
            xAutoMapBasicElementTypesGrid.DataSourceList = mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapBasicElementTypesList;
            xAutoMapAdvancedElementTypesGrid.DataSourceList = mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapAdvanceElementTypesList;
        }

        private void SetPlatformAutoMapElements(List<PlatformInfoBase.ElementTypeData> elemenTypeDataList)
        {
            foreach (PlatformInfoBase.ElementTypeData elementTypeOperation in elemenTypeDataList)
            {
                if (elementTypeOperation.IsCommonElementType)
                {
                    mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapBasicElementTypesList.Add(new UIElementFilter(elementTypeOperation.ElementType, string.Empty, elementTypeOperation.IsCommonElementType));
                }
                else
                {
                    mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapAdvanceElementTypesList.Add(new UIElementFilter(elementTypeOperation.ElementType, string.Empty, elementTypeOperation.IsCommonElementType));
                }
            }
        }

       

        private void SetElementLocatorsSettingsData()
        {
            if (mWizard.mPomDeltaUtils.PomLearnUtils.ElementLocatorsSettingsList.Count == 0)
            {
                switch (mAppPlatform)
                {
                    case ePlatformType.Web:
                        mWizard.mPomDeltaUtils.PomLearnUtils.ElementLocatorsSettingsList = new WebPlatform().GetLearningLocators();
                        break;
                    case ePlatformType.Java:
                        mWizard.mPomDeltaUtils.PomLearnUtils.ElementLocatorsSettingsList = new JavaPlatform().GetLearningLocators();
                        break;
                }
            }
            xElementLocatorsSettingsGrid.DataSourceList = mWizard.mPomDeltaUtils.PomLearnUtils.ElementLocatorsSettingsList;
        }

        private void SetAutoMapElementTypesGridView()
        {
            //tool bar
            xAutoMapBasicElementTypesGrid.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Check/Uncheck All Elements", new RoutedEventHandler(CheckUnCheckAllBasicElements));
            xAutoMapAdvancedElementTypesGrid.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Check/Uncheck All Elements", new RoutedEventHandler(CheckUnCheckAllAdvancedElements));

            //Set the Data Grid columns            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(UIElementFilter.Selected), Header = "To Map", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = nameof(UIElementFilter.ElementType), Header = "Element Type", WidthWeight = 100, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(UIElementFilter.ElementExtraInfo), Header = "Element Extra Info", WidthWeight = 100, ReadOnly = true });

            xAutoMapBasicElementTypesGrid.SetAllColumnsDefaultView(view);
            xAutoMapBasicElementTypesGrid.InitViewItems();

            xAutoMapAdvancedElementTypesGrid.SetAllColumnsDefaultView(view);
            xAutoMapAdvancedElementTypesGrid.InitViewItems();
        }

        private void SetElementLocatorsSettingsGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();

            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Active), WidthWeight = 8, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateBy), Header = "Locate By", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.Text, ReadOnly = true });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Help), WidthWeight = 25, ReadOnly = true });

            xElementLocatorsSettingsGrid.SetAllColumnsDefaultView(defView);
            xElementLocatorsSettingsGrid.InitViewItems();

            xElementLocatorsSettingsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
        }

        private void CheckUnCheckAllBasicElements(object sender, RoutedEventArgs e)
        {
            if (mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapBasicElementTypesList.Count > 0)
            {
                bool valueToSet = !mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapBasicElementTypesList[0].Selected;
                foreach (UIElementFilter elem in mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapBasicElementTypesList)
                {
                    elem.Selected = valueToSet;
                }
            }
        }

        private void CheckUnCheckAllAdvancedElements(object sender, RoutedEventArgs e)
        {
            if (mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapAdvanceElementTypesList.Count > 0)
            {
                bool valueToSet = !mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapAdvanceElementTypesList[0].Selected;
                foreach (UIElementFilter elem in mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapAdvanceElementTypesList)
                {
                    elem.Selected = valueToSet;
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
                var windowExplorerDriver = ((IWindowExplorer)(mWizard.mPomDeltaUtils.Agent.Driver));

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
