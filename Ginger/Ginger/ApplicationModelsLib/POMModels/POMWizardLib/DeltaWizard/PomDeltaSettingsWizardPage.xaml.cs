using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Platforms.PlatformsInfo;
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
                    if (mWizard.mPomLearnUtils.POM.TargetApplicationKey != null)
                    {
                        mAppPlatform = WorkSpace.UserProfile.Solution.GetTargetApplicationPlatform(mWizard.mPomLearnUtils.POM.TargetApplicationKey);
                    }

                    SetAutoMapElementTypes();
                    SetAutoMapElementTypesGridView();
                    SetAutoMapElementLocatorssSection();                    
                    SetAutoMapElementLocatorsGridView();
                    break;
            }
        }

        private void SetAutoMapElementTypes()
        {
            if (mWizard.mPomLearnUtils.AutoMapElementTypesList.Count == 0)
            {
                switch (mAppPlatform)
                {
                    case ePlatformType.Web:
                        foreach (PlatformInfoBase.ElementTypeData elementTypeOperation in new WebPlatform().GetPlatformElementTypesData().ToList())
                        {
                            mWizard.mPomLearnUtils.AutoMapElementTypesList.Add(new UIElementFilter(elementTypeOperation.ElementType, string.Empty, elementTypeOperation.IsCommonElementType));
                        }
                        break;
                }
            }
            xAutoMapElementTypesGrid.DataSourceList = mWizard.mPomLearnUtils.AutoMapElementTypesList;
        }

        private void SetAutoMapElementLocatorssSection()
        {
            if (mWizard.mPomLearnUtils.AutoMapElementLocatorsList.Count == 0)
            {
                switch (mAppPlatform)
                {
                    case ePlatformType.Web:
                        mWizard.mPomLearnUtils.AutoMapElementLocatorsList = new WebPlatform().GetLearningLocators();
                        break;
                }
            }
            xAutoMapElementLocatorsGrid.DataSourceList = mWizard.mPomLearnUtils.AutoMapElementLocatorsList;
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
            if (mWizard.mPomLearnUtils.AutoMapElementTypesList.Count > 0)
            {
                bool valueToSet = !mWizard.mPomLearnUtils.AutoMapElementTypesList[0].Selected;
                foreach (UIElementFilter elem in mWizard.mPomLearnUtils.AutoMapElementTypesList)
                    elem.Selected = valueToSet;
            }
        }       
    }
}
