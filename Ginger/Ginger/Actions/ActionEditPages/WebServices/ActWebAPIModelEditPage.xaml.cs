#region License
/*
Copyright © 2014-2019 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions.WebServices.WebAPI;
using GingerWPF.TreeViewItemsLib.ApplicationModelsTreeItems;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Ginger.DataSource;

namespace Ginger.Actions.WebServices
{
    /// <summary>
    /// Interaction logic for ActWebAPIModelEditPage.xaml
    /// </summary>
    public partial class ActWebAPIModelEditPage : Page
    {
        private ActWebAPIModel mAct;
        ApplicationAPIModel AAMB;
        SingleItemTreeViewSelectionPage apiModelPage;
        RepositoryFolder<ApplicationAPIModel> mAPIModelFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationAPIModel>();

        public ActWebAPIModelEditPage(ActWebAPIModel Act)
        {
            InitializeComponent();
            mAct = Act;
            if (mAct.APImodelGUID != new Guid())
                AAMB = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<ApplicationAPIModel>(mAct.APImodelGUID);

            bool mappedNew = false;
            if (AAMB == null)//API mapped for this action is missing, Map new API
            {
                mappedNew = true;
                ChangeAPIMapping(mappedNew);
            }

            if (AAMB != null)
            {
                WorkSpace.Instance.RefreshGlobalAppModelParams(AAMB);
                UpdateOptionalValuesAndParams(!mappedNew);
                SetFieldsGrid();
            }
            else
            {
                //Show empty page, action is not mapped to any API
            }
        }

        private void SetFieldsGrid()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            APIModelParamsValueUCGrid.AddToolbarTool(eImageType.DataSource, "Map API Parameters to DataSource", new RoutedEventHandler(MapOutputToDataSource));

            view.GridColsView.Add(new GridColView() { Field = nameof(EnhancedActInputValue.Param), Header = "Parameter", WidthWeight = 20, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(EnhancedActInputValue.Description), Header = "Description", WidthWeight = 20, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(EnhancedActInputValue.Value), Header = "Selected Value", StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(nameof(EnhancedActInputValue.OptionalValues), nameof(EnhancedActInputValue.Value), true), WidthWeight = 20 });
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.MainGrid.Resources["ValueExpressionButton"] });
            APIModelParamsValueUCGrid.SetTitleLightStyle = true;
            APIModelParamsValueUCGrid.SetAllColumnsDefaultView(view);
            APIModelParamsValueUCGrid.InitViewItems();
            APIModelParamsValueUCGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(Refresh));

            APIModelParamsValueUCGrid.DataSourceList = mAct.APIModelParamsValue;
        }

        private bool UpdateParamsEnhancedLists(ObservableList<AppModelParameter> paramsList)
        {
            bool ShowNotification = false;
            if (paramsList != null)
            {
                int currNumOfActParams = mAct.APIModelParamsValue.Count;
                int numOfAPIParams = 0;

                List<EnhancedActInputValue> OldAPIModelParamsValue = new List<EnhancedActInputValue>();
                foreach (EnhancedActInputValue value in mAct.APIModelParamsValue)
                    OldAPIModelParamsValue.Add(value);

                mAct.APIModelParamsValue.Clear();

                foreach (AppModelParameter AMDP in paramsList)
                {
                    if (AMDP.RequiredAsInput == true)
                    {
                        numOfAPIParams++;
                        EnhancedActInputValue paramToUpdate = OldAPIModelParamsValue.Where(x => x.ParamGuid == AMDP.Guid).FirstOrDefault();
                        if (paramToUpdate != null) //Param already been in the action list, just update his values
                        {
                            string OVDefaultValue = string.Empty;
                            foreach (OptionalValue OP in AMDP.OptionalValuesList)
                            {
                                //check if need to clear current optional values
                                paramToUpdate.OptionalValues.Add(OP.Value);
                                if (OP.IsDefault)
                                    OVDefaultValue = OP.Value;
                            }
                            if (string.IsNullOrEmpty(paramToUpdate.Value))
                                paramToUpdate.Value = OVDefaultValue;
                        }
                        else //Param is new add it to the list
                        {
                            ShowNotification = true;
                            paramToUpdate = new EnhancedActInputValue();
                            paramToUpdate.ParamGuid = AMDP.Guid;
                            foreach (OptionalValue OP in AMDP.OptionalValuesList)
                            {
                                paramToUpdate.OptionalValues.Add(OP.Value);
                                if (OP.IsDefault)
                                    paramToUpdate.Value = OP.Value;
                            }
                        }

                        paramToUpdate.Param = AMDP.PlaceHolder;
                        paramToUpdate.Description = AMDP.Description;
                        mAct.APIModelParamsValue.Add(paramToUpdate);
                    }
                }
                if (currNumOfActParams != numOfAPIParams)
                    ShowNotification = true;
            }
            return ShowNotification;
        }

        private void UpdateOptionalValues()
        {
            if (AAMB == null)
                return;

            foreach (EnhancedActInputValue EIV in mAct.APIModelParamsValue)
            {
                string value = EIV.Value;
                EIV.OptionalValues.Clear();
                AppModelParameter AMP = AAMB.AppModelParameters.Where(x => x.Guid == EIV.ParamGuid).FirstOrDefault();
                if (AMP != null)
                {
                    foreach (OptionalValue OV in AMP.OptionalValuesList)
                        EIV.OptionalValues.Add(OV.Value);
                }
                else
                {
                    AppModelParameter AGMP = AAMB.GlobalAppModelParameters.Where(x => x.Guid == EIV.ParamGuid).FirstOrDefault();
                    if (AGMP != null)
                    {
                        foreach (OptionalValue OV in AGMP.OptionalValuesList)
                            EIV.OptionalValues.Add(OV.Value);
                    }
                }

                EIV.Value = value;
            }
        }
        
        private void ValueExpressionButton_Click(object sender, RoutedEventArgs e)
        {
            EnhancedActInputValue AIV = (EnhancedActInputValue)APIModelParamsValueUCGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(AIV, nameof(ActInputValue.Value), Context.GetAsContext(mAct.Context));
            VEEW.ShowAsWindow();
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            UpdateParamsEnhancedLists(AAMB.MergedParamsList);
        }

        private void ChangeButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ChangeAPIMapping())
                UpdateOptionalValuesAndParams();
        }

        private bool ChangeAPIMapping(bool showNewMappingMessage = false)
        {
            ObservableList<ApplicationAPIModel> APIModelsList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationAPIModel>();
            if (APIModelsList.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.NoAPIExistToMappedTo);
                return false;
            }

            if (showNewMappingMessage)
                Reporter.ToUser(eUserMsgKey.APIMappedToActionIsMissing);

            if (apiModelPage == null)
            {
                RepositoryFolder<ApplicationAPIModel> APIModelsFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationAPIModel>();
                AppApiModelsFolderTreeItem apiRoot = new AppApiModelsFolderTreeItem(APIModelsFolder);
                apiModelPage = new SingleItemTreeViewSelectionPage("API Models", eImageType.APIModel, apiRoot, SingleItemTreeViewSelectionPage.eItemSelectionType.Single, true, 
                                                                                                    new Tuple<string, string>(nameof(ApplicationPOMModel.TargetApplicationKey) + "." +
                                                                                                                nameof(ApplicationPOMModel.TargetApplicationKey.ItemName),
                                                                                                                Convert.ToString(AAMB.TargetApplicationKey)));
            }
            List<object> selectedList = apiModelPage.ShowAsWindow();

            if (selectedList != null && selectedList.Count == 1)
            {
                AAMB = (ApplicationAPIModel)selectedList[0];
                mAct.APImodelGUID = AAMB.Guid;
                return true;
            }

            return false;
        }

        private void UpdateOptionalValuesAndParams(bool showParametersUpdatedMessage = false)
        {
            APIModelTextBox.Text = AAMB.FilePath.Substring(0, AAMB.FilePath.LastIndexOf("\\")).Substring(mAPIModelFolder.FolderFullPath.Length) + @"\" + AAMB.ItemName;
            if (UpdateParamsEnhancedLists(AAMB.MergedParamsList) && showParametersUpdatedMessage)
                Reporter.ToUser(eUserMsgKey.APIParametersListUpdated);
            UpdateOptionalValues();
        }

        private void MapOutputToDataSource(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Reporter.ToUser(eUserMsgKey.ParamExportMessage) == Amdocs.Ginger.Common.eUserMsgSelection.No)
                    return;

                DataSourceTablesListPage dataSourceTablesListPage = new DataSourceTablesListPage();
                dataSourceTablesListPage.ShowAsWindow();

                if (dataSourceTablesListPage.DSName == "" || dataSourceTablesListPage.DSTableName == "")
                {
                    Reporter.ToUser(eUserMsgKey.MappedtoDataSourceError);
                    return;
                }

                foreach (EnhancedActInputValue inputVal in mAct.APIModelParamsValue)
                {
                    string sColName = inputVal.Param.Replace("[", "_").Replace("]", "").Replace("{", "").Replace("}", "");
                    inputVal.Value = "{DS Name=" + dataSourceTablesListPage.DSName + " DST=" + dataSourceTablesListPage.DSTableName + " ACT=MASD MASD=N MR=N IDEN=Cust ICOLVAL=" + sColName + " IROW=NxtAvail}";
                }

            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while mapping the API Model params to Data Source", ex);
                Reporter.ToUser(eUserMsgKey.MappedtoDataSourceError);
            }
        }
    }
}
