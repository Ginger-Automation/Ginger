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
                            AddItemToList(item, ref SelectedItemsListToImport);
                            break;

                        case GlobalSolution.eImportItemType.Environments:
                            AddItemToList(item, ref SelectedItemsListToImport);

                            //find dependant item and add to list
                            ProjEnvironment importedEnv = (ProjEnvironment)newRepositorySerializer.DeserializeFromFile(item.ItemExtraInfo);

                            var dsList = new List<string>();
                            foreach (EnvApplication app in importedEnv.Applications)
                            {
                                List<GeneralParam> generalParams = app.GeneralParams.Where(x => x.Value != null).ToList();
                                generalParams = generalParams.Where(x => x.Value.Contains("{DS Name")).ToList();
                                if (generalParams.Count > 0)
                                {
                                    foreach (GeneralParam generalParam in generalParams)
                                    {
                                        //Get the DataSource name
                                        string[] Token = generalParam.Value.Split(new[] { "{DS Name=", " " }, StringSplitOptions.None);
                                        if (!dsList.Contains(Token[1]))
                                        {
                                            dsList.Add(Token[1]);
                                        }
                                    }
                                }
                                
                            }

                            string[] filePaths = Directory.GetFiles(System.IO.Path.Combine(wiz.SolutionFolder, "DataSources"), "*.xml", SearchOption.AllDirectories);
                            foreach (string file in filePaths)
                            {
                                if (dsList.Contains(System.IO.Path.GetFileNameWithoutExtension(file).Replace(".Ginger.DataSource", "")))
                                {
                                    //check if datasource is already added to list
                                    if (SelectedItemsListToImport.Where(x => x.ItemExtraInfo == file).ToList().Count == 0)
                                    {
                                        GlobalSolutionItem newItem = new GlobalSolutionItem(GlobalSolution.eImportItemType.DataSources, file, true, "", true);
                                        AddItemToList(newItem, ref SelectedItemsListToImport);
                                    }
                                }
                            }

                            break;
                        case GlobalSolution.eImportItemType.DataSources:
                            AddItemToList(item, ref SelectedItemsListToImport);
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

        private void AddItemToList(GlobalSolutionItem itemToAdd, ref ObservableList<GlobalSolutionItem> SelectedItemsListToImport)
        {
            bool skipAdd = false;
            //check if item already exist in the list -> if duplicate keep dependant item
            GlobalSolutionItem listItem = SelectedItemsListToImport.Where(x => x.ItemExtraInfo == itemToAdd.ItemExtraInfo).FirstOrDefault();

            if (listItem != null)
            {
                if (!listItem.IsDependant)
                {
                    SelectedItemsListToImport.Remove(listItem);
                }
                else
                {
                    skipAdd = true;
                }
            }

            itemToAdd.ItemName = GlobalSolutionUtils.Instance.GetRepositoryItemName(itemToAdd.ItemExtraInfo);

            //Check if GUID is already exist
            bool isDuplicateGUID = GlobalSolutionUtils.Instance.CheckForItemWithDuplicateGUID(itemToAdd);
            if (isDuplicateGUID)
            {
                itemToAdd.ItemImportSetting = GlobalSolution.eImportSetting.ReplaceExsiting;
                itemToAdd.Comments = "Item already exist, with same GUID";
            }

            //check if file already exist
            string targetFile = System.IO.Path.Combine(WorkSpace.Instance.SolutionRepository.SolutionFolder, itemToAdd.ItemType.ToString(), System.IO.Path.GetFileName(itemToAdd.ItemExtraInfo));
            if (File.Exists(targetFile) && !isDuplicateGUID)
            {
                if (GlobalSolutionUtils.Instance.IsGingerRepositoryItem(targetFile))
                {
                    itemToAdd.ItemImportSetting = GlobalSolution.eImportSetting.CreateNew;
                    string newFileName = GlobalSolutionUtils.Instance.GetUniqFileName(targetFile, false);
                    itemToAdd.ItemNewName = newFileName;
                    itemToAdd.Comments = "Item already exist, importing item with new name " + newFileName;
                }
                else 
                {
                    itemToAdd.ItemImportSetting = GlobalSolution.eImportSetting.ReplaceExsiting;
                    itemToAdd.Comments = "Item already exist, with same filename";
                }
            }

            if (!skipAdd)
            {
                SelectedItemsListToImport.Add(itemToAdd);
            }
        }

        
    }
}
