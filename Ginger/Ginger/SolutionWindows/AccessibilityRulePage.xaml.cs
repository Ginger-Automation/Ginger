using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.ActionsLib.UI.Web;
using Amdocs.Ginger.CoreNET.GeneralLib;
using Amdocs.Ginger.Repository;
using Ginger.Configurations;
using Ginger.Run;
using Ginger.SolutionGeneral;
using Ginger.UserControls;
using GingerCore;
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

namespace Ginger.SolutionWindows
{
    /// <summary>
    /// Interaction logic for AccessibilityRulePage.xaml
    /// </summary>
    public partial class AccessibilityRulePage : Page
    {
        Solution mSolution;
        string AppName;
        List<string> DefaultExcludeRulesList;
        private AccessibilityConfiguration mAccessibilityConfiguration;
        public AccessibilityRulePage()
        {
            InitializeComponent();
            mSolution = WorkSpace.Instance.Solution;
            string allProperties = string.Empty;
            WorkSpace.Instance.PropertyChanged += WorkSpacePropertyChanged;
            LoadGridData();
            SetAppsGrid();
            mAccessibilityConfiguration = WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<AccessibilityConfiguration>();
            mAccessibilityConfiguration.DefaultExcludeRule = mAccessibilityConfiguration.DefaultExcludeRule != null ? mAccessibilityConfiguration.DefaultExcludeRule : new ();
        }
        private void WorkSpacePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WorkSpace.Solution))
            {
                mSolution = WorkSpace.Instance.Solution;
                LoadGridData();
            }
        }

        private void SetAppsGrid()
        {
            xAccessibilityRulesGrid.SetGridEnhancedHeader(Amdocs.Ginger.Common.Enums.eImageType.Accessibility, $"{GingerCore.General.GetEnumValueDescription(typeof(eTermResKey), nameof(eTermResKey.AccessibilityRules))}", saveAllHandler: SaveHandler, null, true);
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView()
            {
                Field = nameof(AccessibilityRuleData.Active),
                WidthWeight = 2,
                StyleType = GridColView.eGridColStyleType.Template,
                CellTemplate = (DataTemplate)FindResource("CheckBoxTemplate"),
                AllowSorting = true
            });
            view.GridColsView.Add(new GridColView()
            {
                Field = nameof(AccessibilityRuleData.RuleID),
                Header = "RuleID",
                ReadOnly = true,
                WidthWeight = 15,
                AllowSorting = true
            });
            view.GridColsView.Add(new GridColView()
            {
                Field = nameof(AccessibilityRuleData.Description),
                Header = "Description",
                WidthWeight = 40,
                ReadOnly = true
            });
            view.GridColsView.Add(new GridColView()
            {
                Field = nameof(AccessibilityRuleData.Impact),
                Header = "Severity",
                WidthWeight = 5,
                ReadOnly = true,
                AllowSorting = true
            });
            view.GridColsView.Add(new GridColView()
            {
                Field = nameof(AccessibilityRuleData.Tags),
                WidthWeight = 30,
                ReadOnly = true
            });

            xAccessibilityRulesGrid.AddLabel("Note: We will analyze the active items from the list below for accessibility. Testing and deactivating items won't be taken into account for accessibility testing.");

            xAccessibilityRulesGrid.SetAllColumnsDefaultView(view);
            xAccessibilityRulesGrid.InitViewItems();
        }

        private void LoadGridData()
        {
            ActAccessibilityTesting actAccessibilityTesting = new ActAccessibilityTesting();
            List<AccessibilityRuleData> sortedList = actAccessibilityTesting.RulesItemsdata.OrderByDescending(data => !data.Active).ToList();
            ObservableList<AccessibilityRuleData> accessibilityRuleDatas = [.. sortedList];
            xAccessibilityRulesGrid.DataSourceList = accessibilityRuleDatas;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null && checkBox.DataContext is AccessibilityRuleData data)
            {
                // Do something with data, for example:
                data.Active = checkBox.IsChecked ?? false;
                mAccessibilityConfiguration.StartDirtyTracking();
                if (!data.Active)
                {
                    if (!mAccessibilityConfiguration.DefaultExcludeRule.Any(x => x.Equals(data.RuleID)))
                    {
                        mAccessibilityConfiguration.DefaultExcludeRule.Add(data);
                    }
                    
                }
                else
                {
                    if (mAccessibilityConfiguration.DefaultExcludeRule.Any(x => x.Equals(data.RuleID)))
                    {
                        mAccessibilityConfiguration.DefaultExcludeRule.Remove(data);
                        
                    }
                }
            }
        }

        private void SaveHandler(object sender, RoutedEventArgs e)
        {
            mSolution.SolutionOperations.SaveSolution(true, Solution.eSolutionItemToSave.DefaultExcludeRule);
        }
    }
}
