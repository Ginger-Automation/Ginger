using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GlobalSolutionLib;
using Ginger.Actions;
using Ginger.UserControls;
using GingerWPF.UserControlsLib.UCTreeView;
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

namespace Ginger.GlobalSolutionLib.ImportItemWizardLib
{
    /// <summary>
    /// Interaction logic for SelectItemImportTypePage.xaml
    /// </summary>
    public partial class ItemDependancyPage : Page, IWizardPage
    {
        ImportItemWizard wiz;

        public ItemDependancyPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    wiz = (ImportItemWizard)WizardEventArgs.Wizard;
                    break;
                case EventType.Active:
                    SetDependantItemsListToImportGridView();
                    wiz.SelectedItemTypeListToImport = GetSelectedItemsListToImport();
                    xDependantItemsToImportGrid.DataSourceList = wiz.SelectedItemTypeListToImport;
                    break;
            }
        }

        
        private void SetDependantItemsListToImportGridView()
        {
            //Set the Data Grid columns            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.Selected), Header = "Select", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.ItemType), Header = "Item Type", WidthWeight = 50, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.ItemExtraInfo), Header = "Item Extra Info", WidthWeight = 150, ReadOnly = true });

            xDependantItemsToImportGrid.SetAllColumnsDefaultView(view);
            xDependantItemsToImportGrid.InitViewItems();
        }

        public ObservableList<GlobalSolutionItem> GetSelectedItemsListToImport()
        {
            ObservableList<GlobalSolutionItem> SelectedItemsListToImport = new ObservableList<GlobalSolutionItem>();
            if (wiz.SelectedItems != null)
            {
                foreach (ITreeViewItem item in wiz.SelectedItems)
                {
                    if (item.GetType() == typeof(SolutionWindows.TreeViewItems.DocumentTreeItem))
                    {
                        SelectedItemsListToImport.Add(new GlobalSolutionItem(GlobalSolution.ImportItemType.Documents, ((SolutionWindows.TreeViewItems.DocumentTreeItem)item).NodePath(), true, "", ""));
                    }
                    if (item.GetType() == typeof(SolutionWindows.TreeViewItems.EnvironmentTreeItem))
                    {
                        SelectedItemsListToImport.Add(new GlobalSolutionItem(GlobalSolution.ImportItemType.Environments, ((SolutionWindows.TreeViewItems.EnvironmentTreeItem)item).NodePath(), true, "", ""));
                    }
                    if (item.GetType() == typeof(SolutionWindows.TreeViewItems.DataSourceTreeItem))
                    {
                        SelectedItemsListToImport.Add(new GlobalSolutionItem(GlobalSolution.ImportItemType.DataSources, ((SolutionWindows.TreeViewItems.DataSourceTreeItem)item).NodePath(), true, "", ""));
                    }
                }
            }
            return SelectedItemsListToImport;
        }
    }
}
