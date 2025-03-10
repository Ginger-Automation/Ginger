#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Ginger.DataSource;
using Ginger.UserControls;
using Ginger.UserControlsLib.TextEditor;
using GingerCore.Actions.WebAPI;
using GingerCore.Actions.WebServices;
using GingerCore.Actions.WebServices.WebAPI;
using GingerWPF.ApplicationModelsLib.APIModels;
using GingerWPF.TreeViewItemsLib.ApplicationModelsTreeItems;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
            {
                AAMB = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<ApplicationAPIModel>(mAct.APImodelGUID);
            }

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
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView = []
            };
            APIModelParamsValueUCGrid.AddToolbarTool(eImageType.DataSource, "Map API Parameters to DataSource", new RoutedEventHandler(MapOutputToDataSource));

            view.GridColsView.Add(new GridColView() { Field = nameof(EnhancedActInputValue.Param), Header = "Parameter", WidthWeight = 20, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(EnhancedActInputValue.Description), Header = "Description", WidthWeight = 20, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(EnhancedActInputValue.Value), Header = "Selected Value", StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(nameof(EnhancedActInputValue.OptionalValues), nameof(EnhancedActInputValue.Value), true), WidthWeight = 40 });
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 5, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.MainGrid.Resources["ValueExpressionButton"] });
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

                List<EnhancedActInputValue> OldAPIModelParamsValue = [.. mAct.APIModelParamsValue];

                mAct.APIModelParamsValue.Clear();

                foreach (AppModelParameter AMDP in paramsList)
                {
                    if (AMDP.RequiredAsInput == true)
                    {
                        numOfAPIParams++;
                        EnhancedActInputValue paramToUpdate = OldAPIModelParamsValue.FirstOrDefault(x => x.ParamGuid == AMDP.Guid);

                        if (paramToUpdate != null) //Param already been in the action list, just update his values
                        {
                            paramToUpdate.OptionalValues.Clear();
                            string OVDefaultValue = string.Empty;
                            foreach (OptionalValue OP in AMDP.OptionalValuesList)
                            {
                                //check if need to clear current optional values
                                paramToUpdate.OptionalValues.Add(OP.Value);
                                if (OP.IsDefault)
                                {
                                    OVDefaultValue = OP.Value;
                                }
                            }
                            if (string.IsNullOrEmpty(paramToUpdate.Value))
                            {
                                paramToUpdate.Value = OVDefaultValue;
                            }
                        }
                        else //Param is new add it to the list
                        {
                            ShowNotification = true;
                            paramToUpdate = new EnhancedActInputValue
                            {
                                ParamGuid = AMDP.Guid
                            };
                            foreach (OptionalValue OP in AMDP.OptionalValuesList)
                            {
                                paramToUpdate.OptionalValues.Add(OP.Value);
                                if (OP.IsDefault)
                                {
                                    paramToUpdate.Value = OP.Value;
                                }
                            }
                        }

                        paramToUpdate.Param = AMDP.PlaceHolder;
                        paramToUpdate.Description = AMDP.Description;
                        //re-use selected value
                        if (OldAPIModelParamsValue.FirstOrDefault(x => x.Param == paramToUpdate.Param) != null)
                        {
                            paramToUpdate.Value = OldAPIModelParamsValue.FirstOrDefault(x => x.Param == paramToUpdate.Param).Value;
                        }
                        mAct.APIModelParamsValue.Add(paramToUpdate);
                    }
                }
                if (currNumOfActParams != numOfAPIParams)
                {
                    ShowNotification = true;
                }
            }
            return ShowNotification;
        }

        private void UpdateOptionalValues()
        {
            if (AAMB == null)
            {
                return;
            }

            foreach (EnhancedActInputValue EIV in mAct.APIModelParamsValue)
            {
                string value = EIV.Value;
                EIV.OptionalValues.Clear();
                AppModelParameter AMP = AAMB.AppModelParameters.FirstOrDefault(x => x.Guid == EIV.ParamGuid);
                if (AMP != null)
                {
                    foreach (OptionalValue OV in AMP.OptionalValuesList)
                    {
                        EIV.OptionalValues.Add(OV.Value);
                    }
                }
                else
                {
                    AppModelParameter AGMP = AAMB.GlobalAppModelParameters.FirstOrDefault(x => x.Guid == EIV.ParamGuid);
                    if (AGMP != null)
                    {
                        foreach (OptionalValue OV in AGMP.OptionalValuesList)
                        {
                            EIV.OptionalValues.Add(OV.Value);
                        }
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
            {
                UpdateOptionalValuesAndParams();
            }
        }

        private bool ChangeAPIMapping(bool showNewMappingMessage = false)
        {
            if (!WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationAPIModel>().Any())
            {
                Reporter.ToUser(eUserMsgKey.NoAPIExistToMappedTo);
                return false;
            }

            if (showNewMappingMessage)
            {
                Reporter.ToUser(eUserMsgKey.APIMappedToActionIsMissing);
            }

            RepositoryFolder<ApplicationAPIModel> APIModelsFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationAPIModel>();
            AppApiModelsFolderTreeItem apiRoot = new AppApiModelsFolderTreeItem(APIModelsFolder);
            if (AAMB != null && AAMB.TargetApplicationKey != null)
            {
                var apisFilterTuple = new Tuple<string, string>(nameof(ApplicationPOMModel.TargetApplicationKey) + "." + nameof(ApplicationPOMModel.TargetApplicationKey.ItemName), Convert.ToString(AAMB.TargetApplicationKey.ItemName));
                apiModelPage = new SingleItemTreeViewSelectionPage("API Models", eImageType.APIModel, apiRoot, SingleItemTreeViewSelectionPage.eItemSelectionType.Single, true, apisFilterTuple, UCTreeView.eFilteroperationType.Equals);
            }
            else
            {
                apiModelPage = new SingleItemTreeViewSelectionPage("API Models", eImageType.APIModel, apiRoot, SingleItemTreeViewSelectionPage.eItemSelectionType.Single, true);
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
            APIModelTextBox.Text = AAMB.FilePath[..AAMB.FilePath.LastIndexOf("\\")][mAPIModelFolder.FolderFullPath.Length..] + @"\" + AAMB.ItemName;
            if (UpdateParamsEnhancedLists(AAMB.MergedParamsList) && showParametersUpdatedMessage)
            {
                Reporter.ToUser(eUserMsgKey.APIParametersListUpdated);
            }

            UpdateOptionalValues();
        }

        private void MapOutputToDataSource(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Reporter.ToUser(eUserMsgKey.ParamExportMessage) == Amdocs.Ginger.Common.eUserMsgSelection.No)
                {
                    return;
                }

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
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while mapping the API Model parameters to Data Source", ex);
                Reporter.ToUser(eUserMsgKey.MappedtoDataSourceError);
            }
        }

        private void xViewAPIBtn_Click(object sender, RoutedEventArgs e)
        {
            APIModelPage mAPIEditPage = new APIModelPage(AAMB);
            mAPIEditPage.ShowAsWindow(eWindowShowStyle.Dialog, e: APIModelPage.eEditMode.Edit, parentWindow: Window.GetWindow(this));
        }

        private void xViewRawRequestBtn_Click(object sender, RoutedEventArgs e)
        {
            ActWebAPIBase actWebAPI = null;
            if (mAct is ActWebAPIModel ActWAPIM)
            {
                //pull pointed API Model
                ApplicationAPIModel AAMB1 = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<ApplicationAPIModel>(mAct.APImodelGUID);
                if (AAMB1 != null)
                {
                    //init matching real WebAPI Action
                    if (AAMB1.APIType == ApplicationAPIUtils.eWebApiType.REST)
                    {
                        actWebAPI = ActWAPIM.CreateActWebAPIREST(AAMB1, ActWAPIM);
                    }
                    else if (AAMB.APIType == ApplicationAPIUtils.eWebApiType.SOAP)
                    {
                        actWebAPI = ActWAPIM.CreateActWebAPISOAP(AAMB1, ActWAPIM);
                    }
                }
            }

            HttpWebClientUtils webAPIUtils = new HttpWebClientUtils();
            string requestContent = webAPIUtils.GetRawRequestContentPreview(actWebAPI);
            if (requestContent != string.Empty)
            {
                string tempFilePath = GingerCoreNET.GeneralLib.General.CreateTempTextFile(requestContent);
                if (System.IO.File.Exists(tempFilePath))
                {
                    DocumentEditorPage docPage = new DocumentEditorPage(tempFilePath, enableEdit: false, UCTextEditorTitle: string.Empty)
                    {
                        Width = 800,
                        Height = 800
                    };
                    docPage.ShowAsWindow("Raw Request Preview");
                    System.IO.File.Delete(tempFilePath);
                    return;
                }
            }
            Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Failed to load raw request preview, see log for details.");
        }
    }
}
