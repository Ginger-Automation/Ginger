using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore;
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

namespace Ginger.ApplicationModelsLib.POMModels.POMWizardLib.DeltaWizard
{
    /// <summary>
    /// Interaction logic for PomDeltaSettingsPage.xaml
    /// </summary>
    public partial class PomDeltaSettingsPage : Page
    {
        private PomDeltaWizard mWizard;
        private ePlatformType mAppPlatform;

        public PomDeltaSettingsPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (PomDeltaWizard)WizardEventArgs.Wizard;



                    
                    ClearAutoMapElementTypesSection();

                    SetAutoMapElementTypesGridView();
                    SetAutoMapElementLocatorsGridView();
                    break;
            }
        }



        private void RemoveValidations()
        {
            xAgentControlUC.RemoveValidations(ucAgentControl.SelectedAgentProperty);
        }

        private void SetAutoMapElementTypes()
        {
            if (mWizard.AutoMapElementTypesList.Count == 0)
            {
                switch (mAppPlatform)
                {
                    case ePlatformType.Web:
                        foreach (PlatformInfoBase.ElementTypeData elementTypeOperation in new WebPlatform().GetPlatformElementTypesData().ToList())
                        {
                            mWizard.AutoMapElementTypesList.Add(new UIElementFilter(elementTypeOperation.ElementType, string.Empty, elementTypeOperation.IsCommonElementType));
                        }
                        break;
                }
            }
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
            if (mWizard.AutoMapElementTypesList.Count > 0)
            {
                bool valueToSet = !mWizard.AutoMapElementTypesList[0].Selected;
                foreach (UIElementFilter elem in mWizard.AutoMapElementTypesList)
                    elem.Selected = valueToSet;
            }
        }

        private void XAgentControlUC_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ucAgentControl.AgentIsRunning))
            {
                if (xAgentControlUC.AgentIsRunning)
                {
                    SetAutoMapElementTypesSection();
                    SetAutoMapElementLocatorssSection();
                }
                else
                {
                    ClearAutoMapElementTypesSection();
                }
                xAutoMapElementTypesExpander.IsExpanded = xAgentControlUC.AgentIsRunning;
                xAutoMapElementTypesExpander.IsEnabled = xAgentControlUC.AgentIsRunning;
                xAutoMapElementLocatorsExpander.IsExpanded = xAgentControlUC.AgentIsRunning;
                xAutoMapElementLocatorsExpander.IsEnabled = xAgentControlUC.AgentIsRunning;
            }
        }

        private void ClearAutoMapElementTypesSection()
        {
            mWizard.AutoMapElementTypesList = new ObservableList<UIElementFilter>();
            xAutoMapElementTypesGrid.DataSourceList = mWizard.AutoMapElementTypesList;
        }

        private void SetAutoMapElementTypesSection()
        {
            xAgentControlUC.xAgentConfigsExpander.IsExpanded = false;

            SetAutoMapElementTypes();
            xAutoMapElementTypesGrid.DataSourceList = mWizard.AutoMapElementTypesList;
        }

        private void SetAutoMapElementLocatorssSection()
        {
            if (mWizard.AutoMapElementLocatorsList.Count == 0)
            {
                mWizard.AutoMapElementLocatorsList = new WebPlatform().GetLearningLocators();
            }
            xAutoMapElementLocatorsGrid.DataSourceList = mWizard.AutoMapElementLocatorsList;
        }

        private void xAutomaticElementConfigurationRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (mWizard != null)
            {
                if ((bool)xManualElementConfigurationRadioButton.IsChecked)
                {
                    mWizard.ManualElementConfiguration = true;
                    RemoveValidations();
                    xAgentControlUC.Visibility = Visibility.Hidden;
                    xAutoMapElementTypesExpander.Visibility = Visibility.Hidden;
                    xAutoMapElementLocatorsExpander.Visibility = Visibility.Collapsed;
                }
                else
                {
                    mWizard.ManualElementConfiguration = false;
                    AddValidations();
                    xAgentControlUC.Visibility = Visibility.Visible;
                    xAutoMapElementTypesExpander.Visibility = Visibility.Visible;
                    xAutoMapElementLocatorsExpander.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
