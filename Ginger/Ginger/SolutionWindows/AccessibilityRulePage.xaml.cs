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
        private AccessibilityConfiguration mAccessibilityConfiguration;
        private static ActAccessibilityTesting actAccessibilityTesting = new ActAccessibilityTesting(); 
        public AccessibilityRulePage()
        {
            InitializeComponent();
            mSolution = WorkSpace.Instance.Solution;
            string allProperties = string.Empty;
            LoadGridData();
            SetAppsGrid();
            mAccessibilityConfiguration = !WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<AccessibilityConfiguration>().Any() ? new AccessibilityConfiguration() : WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<AccessibilityConfiguration>();
            mAccessibilityConfiguration.ExcludedRules = mAccessibilityConfiguration.ExcludedRules != null ? mAccessibilityConfiguration.ExcludedRules : new ();
        }

        private void SetAppsGrid()
        {
            xAccessibilityRulesGrid.SetGridEnhancedHeader(Amdocs.Ginger.Common.Enums.eImageType.Accessibility, $"{GingerCore.General.GetEnumValueDescription(typeof(eTermResKey), nameof(eTermResKey.AccessibilityRules))}", null, null, true);
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

            xAccessibilityRulesGrid.AddLabel("Note: Ginger will only analyze the active accessibility testing rules; inactive rules will not be considered.");

            xAccessibilityRulesGrid.SetAllColumnsDefaultView(view);
            xAccessibilityRulesGrid.InitViewItems();
        }

        private void LoadGridData()
        {
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
                    if(!mAccessibilityConfiguration.ExcludedRules.Any())
                    {
                        GingerCoreNET.GeneralLib.General.CreateDefaultAccessiblityconfiguration();
                        mAccessibilityConfiguration = WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<AccessibilityConfiguration>();
                        mAccessibilityConfiguration.ExcludedRules = mAccessibilityConfiguration.ExcludedRules != null ? mAccessibilityConfiguration.ExcludedRules : new();
                    }

                    if (!mAccessibilityConfiguration.ExcludedRules.Any(x => x.RuleID.Equals(data.RuleID,StringComparison.CurrentCultureIgnoreCase)))
                    {
                        mAccessibilityConfiguration.ExcludedRules.Add(data);
                    }
                }
                else
                {
                    if (mAccessibilityConfiguration.ExcludedRules.Any(x => x.RuleID.Equals(data.RuleID,StringComparison.CurrentCultureIgnoreCase)))
                    {
                        mAccessibilityConfiguration.ExcludedRules.RemoveItem(data);
                    }
                }
            }
        }
    }
}
