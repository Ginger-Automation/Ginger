using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GlobalSolutionLib;
using Amdocs.Ginger.CoreNET.GlobalSolutionLib;
using Amdocs.Ginger.Repository;
using Ginger.Actions;
using Ginger.UserControls;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace Ginger.GlobalSolutionLib.ImportItemWizardLib
{
    /// <summary>
    /// Interaction logic for SelectItemImportTypePage.xaml
    /// </summary>
    public partial class ItemDependancyPage : Page, IWizardPage
    {
        ImportItemWizard wiz;
        NewRepositorySerializer newRepositorySerializer = new NewRepositorySerializer();

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
                    //GlobalSolutionUtils.Instance.EncryptionKey = wiz.EncryptionKey;
                    //GlobalSolutionUtils.Instance.SolutionFolder = wiz.SolutionFolder;
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
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.ItemName), Header = "Item Name", WidthWeight = 50, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.IsDependant), Header = "Is Dependant", WidthWeight = 50, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.ItemExtraInfo), Header = "Item Full Path", WidthWeight = 150, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.Comments), Header = "Comments", WidthWeight = 120, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.ItemImportSetting), Header = "Import Setting", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = GlobalSolution.GetEnumValues<GlobalSolution.eImportSetting>() });

            xDependantItemsToImportGrid.SetAllColumnsDefaultView(view);
            xDependantItemsToImportGrid.InitViewItems();

            xDependantItemsToImportGrid.SetBtnImage(xDependantItemsToImportGrid.btnMarkAll, "@CheckAllColumn_16x16.png");
            xDependantItemsToImportGrid.btnMarkAll.Visibility = Visibility.Visible;
            xDependantItemsToImportGrid.MarkUnMarkAllActive += MarkUnMarkAllItems;
        }

        public ObservableList<GlobalSolutionItem> GetSelectedItemsListToImport()
        {
            ObservableList<GlobalSolutionItem> SelectedItemsListToImport = new ObservableList<GlobalSolutionItem>();
            if (wiz.ItemsListToImport != null)
            {

                foreach (GlobalSolutionItem item in wiz.ItemsListToImport.Where(x => x.Selected).ToList())
                {
                    switch (item.ItemType)
                    {
                        case GlobalSolution.eImportItemType.Documents:
                            GlobalSolutionUtils.Instance.AddItemToSelectedItemsList(item, ref SelectedItemsListToImport);
                            break;
                        case GlobalSolution.eImportItemType.DataSources:
                            GlobalSolutionUtils.Instance.AddItemToSelectedItemsList(item, ref SelectedItemsListToImport);
                            break;

                        case GlobalSolution.eImportItemType.Environments:
                            GlobalSolutionUtils.Instance.AddItemToSelectedItemsList(item, ref SelectedItemsListToImport);
                            GlobalSolutionUtils.Instance.AddDependaciesForEnvironment(item, ref SelectedItemsListToImport, ref wiz.VariableListToImport);
                            break;
                        case GlobalSolution.eImportItemType.SharedRepositoryActivitiesGroup:
                            GlobalSolutionUtils.Instance.AddItemToSelectedItemsList(item, ref SelectedItemsListToImport);
                            GlobalSolutionUtils.Instance.AddDependaciesForSharedActivityGroup(item, ref SelectedItemsListToImport);
                            break;
                        case GlobalSolution.eImportItemType.SharedRepositoryActivities:
                            GlobalSolutionUtils.Instance.AddItemToSelectedItemsList(item, ref SelectedItemsListToImport);
                            GlobalSolutionUtils.Instance.AddDependaciesForSharedActivity(item, ref SelectedItemsListToImport, ref wiz.VariableListToImport, ref wiz.EnvAppListToImport);
                            break;
                        case GlobalSolution.eImportItemType.SharedRepositoryActions:
                            GlobalSolutionUtils.Instance.AddItemToSelectedItemsList(item, ref SelectedItemsListToImport);
                            GlobalSolutionUtils.Instance.AddDependaciesForSharedAction(item, ref SelectedItemsListToImport, ref wiz.VariableListToImport, ref wiz.EnvAppListToImport);
                            break;
                        case GlobalSolution.eImportItemType.SharedRepositoryVariables:
                            GlobalSolutionUtils.Instance.AddItemToSelectedItemsList(item, ref SelectedItemsListToImport);
                            break;
                        case GlobalSolution.eImportItemType.APIModels:
                            GlobalSolutionUtils.Instance.AddItemToSelectedItemsList(item, ref SelectedItemsListToImport);
                            GlobalSolutionUtils.Instance.AddDependaciesForAPIModel(item, ref SelectedItemsListToImport, ref wiz.VariableListToImport, ref wiz.EnvAppListToImport);
                            break;
                        case GlobalSolution.eImportItemType.POMModels:
                            GlobalSolutionUtils.Instance.AddItemToSelectedItemsList(item, ref SelectedItemsListToImport);
                            GlobalSolutionUtils.Instance.AddDependaciesForPOMModel(item, ref SelectedItemsListToImport, ref wiz.VariableListToImport, ref wiz.EnvAppListToImport);
                            break;

                    }
                }
            }
            return SelectedItemsListToImport;
        }

        private void MarkUnMarkAllItems(bool ActiveStatus)
        {
            foreach (GlobalSolutionItem item in xDependantItemsToImportGrid.DataSourceList)
            {
                item.Selected = ActiveStatus;
            }
        }

        //private void AddItemToList(GlobalSolutionItem itemToAdd, ref ObservableList<GlobalSolutionItem> SelectedItemsListToImport)
        //{
        //    bool skipAdd = false;
        //    //check if item already exist in the list -> if duplicate keep dependant item
        //    GlobalSolutionItem listItem = SelectedItemsListToImport.Where(x => x.ItemExtraInfo == itemToAdd.ItemExtraInfo).FirstOrDefault();

        //    if (listItem != null)
        //    {
        //        if (!listItem.IsDependant)
        //        {
        //            SelectedItemsListToImport.Remove(listItem);
        //        }
        //        else
        //        {
        //            skipAdd = true;
        //        }
        //    }

        //    itemToAdd.ItemName = GlobalSolutionUtils.Instance.GetRepositoryItemName(itemToAdd.ItemExtraInfo);

        //    //Check if GUID is already exist
        //    bool isDuplicateGUID = GlobalSolutionUtils.Instance.CheckForItemWithDuplicateGUID(itemToAdd);
        //    if (isDuplicateGUID)
        //    {
        //        itemToAdd.ItemImportSetting = GlobalSolution.eImportSetting.ReplaceExsiting;
        //        itemToAdd.Comments = "Item already exist, with same GUID";
        //    }

        //    //check if file already exist
        //    string targetFile = System.IO.Path.Combine(WorkSpace.Instance.SolutionRepository.SolutionFolder, itemToAdd.ItemType.ToString(), System.IO.Path.GetFileName(itemToAdd.ItemExtraInfo));
        //    if (File.Exists(targetFile) && !isDuplicateGUID)
        //    {
        //        if (GlobalSolutionUtils.Instance.IsGingerRepositoryItem(targetFile))
        //        {
        //            itemToAdd.ItemImportSetting = GlobalSolution.eImportSetting.CreateNew;
        //            string newFileName = GlobalSolutionUtils.Instance.GetUniqFileName(targetFile, false);
        //            itemToAdd.ItemNewName = newFileName;
        //            itemToAdd.Comments = "Item already exist, importing item with new name " + newFileName;
        //        }
        //        else 
        //        {
        //            itemToAdd.ItemImportSetting = GlobalSolution.eImportSetting.ReplaceExsiting;
        //            itemToAdd.Comments = "Item already exist, with same filename";
        //        }
        //    }

        //    if (!skipAdd)
        //    {
        //        SelectedItemsListToImport.Add(itemToAdd);
        //    }
        //}

        
    }
}
