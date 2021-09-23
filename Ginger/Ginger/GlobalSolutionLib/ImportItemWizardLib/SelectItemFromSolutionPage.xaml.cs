using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.GlobalSolutionLib;
using Amdocs.Ginger.CoreNET.GlobalSolutionLib;
using Ginger.Actions;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.UserControls;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.IO;
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
//using System.Windows.Shapes;

namespace Ginger.GlobalSolutionLib.ImportItemWizardLib
{
    /// <summary>
    /// Interaction logic for SelectItemFromSolutionPage.xaml
    /// </summary>
    public partial class SelectItemFromSolutionPage : Page, IWizardPage
    {
        ImportItemWizard wiz;
        public SelectItemFromSolutionPage()
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
                    SetItemsListToImportGridView();
                    wiz.ItemsListToImport = GetItemsListToImport();
                    xItemsToImportGrid.DataSourceList = wiz.ItemsListToImport;

                    GingerCore.General.FillComboFromList(ItemTypeListComboBox, wiz.ItemTypesList);
                    ItemTypeListComboBox.Items.Insert(0, "All");
                    ItemTypeListComboBox.SelectedIndex = 0;

                    break;
            }
            
        }

        private void SetItemsListToImportGridView()
        {
            //Set the Data Grid columns            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.Selected), Header = "Select", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.ItemType), Header = "Item Type", WidthWeight = 50, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.ItemName), Header = "Item Name", WidthWeight = 50, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.ItemExtraInfo), Header = "Item Full Path", WidthWeight = 150, ReadOnly = true });

            xItemsToImportGrid.SetAllColumnsDefaultView(view);
            xItemsToImportGrid.InitViewItems();

            xItemsToImportGrid.SetBtnImage(xItemsToImportGrid.btnMarkAll, "@CheckAllColumn_16x16.png");
            xItemsToImportGrid.btnMarkAll.Visibility = Visibility.Visible;
            xItemsToImportGrid.MarkUnMarkAllActive += MarkUnMarkAllItems;
        }

        public ObservableList<GlobalSolutionItem> GetItemsListToImport()
        {
            ObservableList<GlobalSolutionItem> ItemsListToImport = new ObservableList<GlobalSolutionItem>();

            //foreach (string importItemType in wiz.ItemTypesList)
            //foreach (GlobalSolution.ImportItemType ItemType in GlobalSolution.GetEnumValues<GlobalSolution.ImportItemType>())
            foreach (GlobalSolutionItem item in wiz.ItemTypeListToImport.Where(x => x.Selected))
            {
                string[] filePaths = null;
                if (item.ItemType == GlobalSolution.eImportItemType.Documents)
                {
                    filePaths = Directory.GetFiles(Path.Combine(wiz.SolutionFolder, item.ItemType.ToString()), "*", SearchOption.AllDirectories);
                }
                else if (item.ItemType == GlobalSolution.eImportItemType.SharedRepositoryActivitiesGroup)
                {
                    filePaths = Directory.GetFiles(Path.Combine(wiz.SolutionFolder, "SharedRepository", "ActivitiesGroup"), "*.xml", SearchOption.AllDirectories);
                }
                else if (item.ItemType == GlobalSolution.eImportItemType.SharedRepositoryActivities)
                {
                    filePaths = Directory.GetFiles(Path.Combine(wiz.SolutionFolder, "SharedRepository", "Activities"), "*.xml", SearchOption.AllDirectories);
                }
                else if (item.ItemType == GlobalSolution.eImportItemType.SharedRepositoryActions)
                {
                    filePaths = Directory.GetFiles(Path.Combine(wiz.SolutionFolder, "SharedRepository", "Actions"), "*.xml", SearchOption.AllDirectories);
                }
                else if (item.ItemType == GlobalSolution.eImportItemType.SharedRepositoryVariables)
                {
                    filePaths = Directory.GetFiles(Path.Combine(wiz.SolutionFolder, "SharedRepository", "Variables"), "*.xml", SearchOption.AllDirectories);
                }
                else if (item.ItemType == GlobalSolution.eImportItemType.APIModels)
                {
                    filePaths = Directory.GetFiles(Path.Combine(wiz.SolutionFolder, "Applications Models", "API Models"), "*.xml", SearchOption.AllDirectories);
                }
                else if (item.ItemType == GlobalSolution.eImportItemType.POMModels)
                {
                    filePaths = Directory.GetFiles(Path.Combine(wiz.SolutionFolder, "Applications Models", "POM Models"), "*.xml", SearchOption.AllDirectories);
                }
                else
                {
                    filePaths = Directory.GetFiles(Path.Combine(wiz.SolutionFolder, item.ItemType.ToString()), "*.xml", SearchOption.AllDirectories);
                }
                foreach (string file in filePaths)
                {
                    string itemName = GlobalSolutionUtils.Instance.GetRepositoryItemName(file);
                    ItemsListToImport.Add(new GlobalSolutionItem(item.ItemType, file, true, itemName, false));
                }
            }
            return ItemsListToImport;
        }

        private void ItemTypeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemTypeListComboBox.SelectedValue == null)
            {
                xItemsToImportGrid.DataSourceList = wiz.ItemsListToImport;
                return;
            }
            bool result = Enum.TryParse(ItemTypeListComboBox.SelectedValue.ToString(), out GlobalSolution.eImportItemType eImportItemType);
            if (result)
            {
                xItemsToImportGrid.DataSourceList = FilterItemsListToImport(eImportItemType);
            }
            else
            {
                xItemsToImportGrid.DataSourceList = wiz.ItemsListToImport;
            }
        }

        ObservableList<GlobalSolutionItem> FilterItemsListToImport(GlobalSolution.eImportItemType importItemType)
        {
            ObservableList<GlobalSolutionItem> ItemsListToImport = GingerCore.General.ConvertListToObservableList(wiz.ItemsListToImport.Where(x => x.ItemType == importItemType).ToList());
            return ItemsListToImport;
        }

        private void MarkUnMarkAllItems(bool ActiveStatus)
        {
            foreach (GlobalSolutionItem item in xItemsToImportGrid.DataSourceList)
            {
                item.Selected = ActiveStatus;
            }
        }
    }
}
