using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.ActionsLib.UI.Web;
using Amdocs.Ginger.CoreNET.GeneralLib;
using Ginger.Configurations;
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
    /// Interaction logic for AccessiblityRulePage.xaml
    /// </summary>
    public partial class AccessiblityRulePage : Page
    {
        Solution mSolution;
        string AppName;
        List<string> DefaultExcludeRulesList;
        private AccessibilityConfiguration mAccessibilityConfiguration;
        public AccessiblityRulePage()
        {
            InitializeComponent();
            mSolution = WorkSpace.Instance.Solution;
            string allProperties = string.Empty;
            WorkSpace.Instance.PropertyChanged += WorkSpacePropertyChanged;
            LoadGridData();
            SetAppsGrid();
            mAccessibilityConfiguration = new();
            DefaultExcludeRulesList = WorkSpace.Instance.Solution.DefaultExcludeRule.DefaultExcludeRules != null ? WorkSpace.Instance.Solution.DefaultExcludeRule.DefaultExcludeRules.Split(',').ToList() : new();
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
            xAccessiblityRulesGrid.SetGridEnhancedHeader(Amdocs.Ginger.Common.Enums.eImageType.Accessibility, $"{GingerCore.General.GetEnumValueDescription(typeof(eTermResKey), nameof(eTermResKey.AccessibilityRules))}", saveAllHandler: SaveHandler, null, true);
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
                Field = nameof(AccessibilityRuleData.Tags),
                WidthWeight = 30,
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
                Field = nameof(AccessibilityRuleData.Description),
                Header = "Description",
                WidthWeight = 40,
                ReadOnly = true
            });


            xAccessiblityRulesGrid.SetAllColumnsDefaultView(view);
            xAccessiblityRulesGrid.InitViewItems();
        }

        private void LoadGridData()
        {
            ActAccessibilityTesting actAccessibilityTesting = new ActAccessibilityTesting();
            List<AccessibilityRuleData> sortedList = actAccessibilityTesting.RulesItemsdata.OrderByDescending(data => !data.Active).ToList();
            ObservableList<AccessibilityRuleData> accessibilityRuleDatas = [.. sortedList];
            xAccessiblityRulesGrid.DataSourceList = accessibilityRuleDatas;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null && checkBox.DataContext is AccessibilityRuleData data)
            {
                // Do something with data, for example:
                data.Active = checkBox.IsChecked ?? false;
                WorkSpace.Instance.Solution.DefaultExcludeRule.StartDirtyTracking();
                if (data.Active == false)
                {
                    if (!DefaultExcludeRulesList.Any(x => x.Equals(data.RuleID)))
                    {
                        DefaultExcludeRulesList.Add(data.RuleID);

                        mAccessibilityConfiguration.DefaultExcludeRules = String.Join(",", DefaultExcludeRulesList.Select(x => x));
                        WorkSpace.Instance.Solution.DefaultExcludeRule = mAccessibilityConfiguration;
                    }
                }
                else
                {
                    if (DefaultExcludeRulesList.Any(x => x.Equals(data.RuleID)))
                    {
                        DefaultExcludeRulesList.Remove(data.RuleID);
                        mAccessibilityConfiguration.DefaultExcludeRules = String.Join(",", DefaultExcludeRulesList.Select(x => x));
                        WorkSpace.Instance.Solution.DefaultExcludeRule = mAccessibilityConfiguration;
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
