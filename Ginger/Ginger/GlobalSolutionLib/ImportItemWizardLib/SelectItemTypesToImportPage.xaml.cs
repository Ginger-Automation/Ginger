#region License
/*
Copyright © 2014-2026 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GlobalSolutionLib;
using Amdocs.Ginger.CoreNET.GlobalSolutionLib;
using Ginger.SolutionGeneral;
using Ginger.UserControls;
using GingerWPF.WizardLib;
using System.Windows;
using System.Windows.Controls;

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
                    UCEncryptionKey.mSolution.SolutionOperations = new SolutionOperations(UCEncryptionKey.mSolution);
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
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(GlobalSolutionItem.Selected), Header = "Select", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.CheckBox },
                new GridColView() { Field = nameof(GlobalSolutionItem.ItemType), Header = "Item Type", WidthWeight = 100, ReadOnly = true },
                new GridColView() { Field = nameof(GlobalSolutionItem.ItemExtraInfo), Header = "Item Extra Info", WidthWeight = 100, ReadOnly = true },
            ]
            };

            xItemTypesToImportGrid.SetAllColumnsDefaultView(view);
            xItemTypesToImportGrid.InitViewItems();
            xItemTypesToImportGrid.SetBtnImage(xItemTypesToImportGrid.btnMarkAll, "@CheckAllColumn_16x16.png");
            xItemTypesToImportGrid.btnMarkAll.Visibility = Visibility.Visible;
            xItemTypesToImportGrid.MarkUnMarkAllActive += MarkUnMarkAllItems;
        }

        public ObservableList<GlobalSolutionItem> GetItemTypeListToImport()
        {
            ObservableList<GlobalSolutionItem> ItemTypeListToImport = [];
            foreach (GlobalSolution.eImportItemType ItemType in GlobalSolution.GetEnumValues<GlobalSolution.eImportItemType>())
            {
                if (ItemType is GlobalSolution.eImportItemType.Variables or GlobalSolution.eImportItemType.TargetApplication or GlobalSolution.eImportItemType.ExtrnalIntegrationConfigurations)
                {
                    continue;
                }
                var description = ((EnumValueDescriptionAttribute[])typeof(GlobalSolution.eImportItemType).GetField(ItemType.ToString()).GetCustomAttributes(typeof(EnumValueDescriptionAttribute), false))[0].ValueDescription;
                ItemTypeListToImport.Add(new GlobalSolutionItem(ItemType, "", description, true, "", ""));
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
