using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.Application_Models;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
                        mAppPlatform = WorkSpace.UserProfile.Solution.GetTargetApplicationPlatform(mWizard.mPomDeltaUtils.POM.TargetApplicationKey);
                    }

                    SetAutoMapElementTypes();
                    SetAutoMapElementTypesGridView();
                    SetAutoMapElementLocatorssSection();                    
                    SetAutoMapElementLocatorsGridView();

                    xAvoidPropertiesAllRadioButton.IsChecked = true;
                    xKeepLocatorsOrderCheckBox.IsChecked = true;
                    break;
            }
        }

        private void SetAutoMapElementTypes()
        {
            if (mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapElementTypesList.Count == 0)
            {
                switch (mAppPlatform)
                {
                    case ePlatformType.Web:
                        foreach (PlatformInfoBase.ElementTypeData elementTypeOperation in new WebPlatform().GetPlatformElementTypesData().ToList())
                        {
                            mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapElementTypesList.Add(new UIElementFilter(elementTypeOperation.ElementType, string.Empty, elementTypeOperation.IsCommonElementType));
                        }
                        break;
                }
            }
            xAutoMapElementTypesGrid.DataSourceList = mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapElementTypesList;
        }

        private void SetAutoMapElementLocatorssSection()
        {
            if (mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapElementLocatorsList.Count == 0)
            {
                switch (mAppPlatform)
                {
                    case ePlatformType.Web:
                        mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapElementLocatorsList = new WebPlatform().GetLearningLocators();
                        break;
                }
            }
            xAutoMapElementLocatorsGrid.DataSourceList = mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapElementLocatorsList;
        }

        private void SetAutoMapElementTypesGridView()
        {
            //tool bar
            xAutoMapElementTypesGrid.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Check/Uncheck All Elements", new RoutedEventHandler(CheckUnCheckAllElements));

            //Set the Data Grid columns            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(UIElementFilter.Selected), Header = "To Map", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = nameof(UIElementFilter.ElementType), Header = "Element Type", WidthWeight = 100, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(UIElementFilter.ElementExtraInfo), Header = "Element Extra Info", WidthWeight = 100, ReadOnly = true });

            xAutoMapElementTypesGrid.SetAllColumnsDefaultView(view);
            xAutoMapElementTypesGrid.InitViewItems();
        }

        private void SetAutoMapElementLocatorsGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();

            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Active), WidthWeight = 8, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateBy), Header = "Locate By", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.Text, ReadOnly = true });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Help), WidthWeight = 25, ReadOnly = true });

            xAutoMapElementLocatorsGrid.SetAllColumnsDefaultView(defView);
            xAutoMapElementLocatorsGrid.InitViewItems();

            xAutoMapElementLocatorsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
        }

        private void CheckUnCheckAllElements(object sender, RoutedEventArgs e)
        {
            if (mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapElementTypesList.Count > 0)
            {
                bool valueToSet = !mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapElementTypesList[0].Selected;
                foreach (UIElementFilter elem in mWizard.mPomDeltaUtils.PomLearnUtils.AutoMapElementTypesList)
                    elem.Selected = valueToSet;
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
    }
}
