using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GlobalSolutionLib;
using Ginger.Actions;
using Ginger.UserControls;
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
    public partial class SelectItemImportTypePage : Page, IWizardPage
    {
        ImportItemWizard wiz;

        public SelectItemImportTypePage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    wiz = (ImportItemWizard)WizardEventArgs.Wizard;
                    xGlobalSolutionFolderUC.Init(null, wiz, nameof(ImportItemWizard.SolutionFolder), false, true, UCValueExpression.eBrowserType.Folder);

                    //SetItemsListToImportGridView();
                    //wiz.ItemTypeListToImport = GetItemTypeListToImport();
                    //xItemTypesToImportGrid.DataSourceList = wiz.ItemTypeListToImport;

                    break;
                case EventType.LeavingForNextPage:
                    if (string.IsNullOrEmpty(wiz.SolutionFolder))
                    {
                        Reporter.ToUser(eUserMsgKey.StaticWarnMessage, string.Format("Please select Solution Folder."));
                        WizardEventArgs.CancelEvent = true;
                        return;
                    }
                    break;
            }
        }

        private void ImportFromLocalFolderTypeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (wiz == null)
            {
                return;
            }
            wiz.ImportFromType = GlobalSolution.eImportFromType.LocalFolder;
            ImportFromLocalFolderPanel.Visibility = Visibility.Visible;
            ImportFromSourceControlPanel.Visibility = Visibility.Hidden;
        }

        private void ImportFromSourceControlTypeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (wiz == null)
            {
                return;
            }
            wiz.ImportFromType = GlobalSolution.eImportFromType.SourceControl;
            ImportFromSourceControlPanel.Visibility = Visibility.Visible;
            ImportFromLocalFolderPanel.Visibility = Visibility.Hidden;
        }

        //private void SetItemsListToImportGridView()
        //{
        //    //Set the Data Grid columns            
        //    GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
        //    view.GridColsView = new ObservableList<GridColView>();

        //    view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.Selected), Header = "Select", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.CheckBox });
        //    view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.ItemType), Header = "Item Type", WidthWeight = 100, ReadOnly = true });
        //    view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.ItemExtraInfo), Header = "Item Extra Info", WidthWeight = 100, ReadOnly = true });

        //    xItemTypesToImportGrid.SetAllColumnsDefaultView(view);
        //    xItemTypesToImportGrid.InitViewItems();
            
        //}

        //public ObservableList<GlobalSolutionItem> GetItemTypeListToImport()
        //{
        //    ObservableList<GlobalSolutionItem> ItemTypeListToImport = new ObservableList<GlobalSolutionItem>();
        //    foreach (GlobalSolution.eImportItemType ItemType in GlobalSolution.GetEnumValues<GlobalSolution.eImportItemType>())
        //    {
        //        var description = ((EnumValueDescriptionAttribute[])typeof(GlobalSolution.eImportItemType).GetField(ItemType.ToString()).GetCustomAttributes(typeof(EnumValueDescriptionAttribute), false))[0].ValueDescription;
        //        ItemTypeListToImport.Add(new GlobalSolutionItem(ItemType, description, true, "", false));
        //    }
        //    return ItemTypeListToImport;
        //}
    }
}
