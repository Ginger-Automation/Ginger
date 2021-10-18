using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GlobalSolutionLib;
using Amdocs.Ginger.CoreNET.GlobalSolutionLib;
using Amdocs.Ginger.Repository;
using Ginger.Actions;
using Ginger.SolutionGeneral;
using Ginger.UserControls;
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
using System.Windows.Shapes;

namespace Ginger.GlobalSolutionLib.ImportItemWizardLib
{
    /// <summary>
    /// Interaction logic for SelectItemTypesToImportPage.xaml
    /// </summary>
    public partial class SelectItemTypesToImportPage : Page, IWizardPage
    {
        ImportItemWizard wiz;
        public SelectItemTypesToImportPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    wiz = (ImportItemWizard)WizardEventArgs.Wizard;
                    SetItemsListToImportGridView();
                    wiz.ItemTypeListToImport = GetItemTypeListToImport();
                    xItemTypesToImportGrid.DataSourceList = wiz.ItemTypeListToImport;
                    break;
                case EventType.Active:
                    ((WizardWindow)wiz.mWizardWindow).ShowFinishButton(false);

                    UCEncryptionKey.mSolution = GlobalSolutionUtils.Instance.GetSolution();
                    if (!string.IsNullOrEmpty(wiz.EncryptionKey))
                    {
                        UCEncryptionKey.EncryptionKeyPasswordBox.Password = wiz.EncryptionKey;
                        UCEncryptionKey.ValidateKey();
                    }
                    break;
                case EventType.LeavingForNextPage:
                    if (string.IsNullOrEmpty(UCEncryptionKey.EncryptionKeyPasswordBox.Password))
                    {
                        Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Please provide Solution Encryption Key.");
                        WizardEventArgs.CancelEvent = true;
                        return;
                    }
                    if (UCEncryptionKey.ValidateKey())
                    {
                        wiz.EncryptionKey = UCEncryptionKey.EncryptionKeyPasswordBox.Password;
                        GlobalSolutionUtils.Instance.EncryptionKey = wiz.EncryptionKey;
                    }
                    else 
                    {
                        Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Loading Solution- Error: Encryption key validation failed.");
                        WizardEventArgs.CancelEvent = true;
                        return;
                    }
                    break;
                default:
                    //Nothing to do
                    break;
            }
        }

        private void SetItemsListToImportGridView()
        {
            //Set the Data Grid columns            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.Selected), Header = "Select", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.ItemType), Header = "Item Type", WidthWeight = 100, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.ItemExtraInfo), Header = "Item Extra Info", WidthWeight = 100, ReadOnly = true });

            xItemTypesToImportGrid.SetAllColumnsDefaultView(view);
            xItemTypesToImportGrid.InitViewItems();
            xItemTypesToImportGrid.SetBtnImage(xItemTypesToImportGrid.btnMarkAll, "@CheckAllColumn_16x16.png");
            xItemTypesToImportGrid.btnMarkAll.Visibility = Visibility.Visible;
            xItemTypesToImportGrid.MarkUnMarkAllActive += MarkUnMarkAllItems;
        }

        public ObservableList<GlobalSolutionItem> GetItemTypeListToImport()
        {
            ObservableList<GlobalSolutionItem> ItemTypeListToImport = new ObservableList<GlobalSolutionItem>();
            foreach (GlobalSolution.eImportItemType ItemType in GlobalSolution.GetEnumValues<GlobalSolution.eImportItemType>())
            {
                if (ItemType == GlobalSolution.eImportItemType.Variables)
                {
                    continue;
                }
                var description = ((EnumValueDescriptionAttribute[])typeof(GlobalSolution.eImportItemType).GetField(ItemType.ToString()).GetCustomAttributes(typeof(EnumValueDescriptionAttribute), false))[0].ValueDescription;
                ItemTypeListToImport.Add(new GlobalSolutionItem(ItemType, "",description, true, "", ""));
            }
            return ItemTypeListToImport;
        }
        private void MarkUnMarkAllItems(bool ActiveStatus)
        {
            foreach (GlobalSolutionItem item in xItemTypesToImportGrid.DataSourceList)
            {
                item.Selected = ActiveStatus;
            }
        }

    }
}
