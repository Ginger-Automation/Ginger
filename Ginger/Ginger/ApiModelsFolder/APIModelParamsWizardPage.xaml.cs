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
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using GingerCore.Actions;
using GingerCore.Actions.WebServices.WebAPI;
using GingerWPF.WizardLib;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Amdocs.Ginger.Common.Enums;
using Ginger.DataSource;
using GingerCore;

namespace Ginger.ApiModelsFolder
{
    public partial class APIModelParamsWizardPage : Page, IWizardPage
    {
        public APIModelParamsWizardPage()
        {
            InitializeComponent();
        }

        AddApiModelActionWizardPage mAddApiModelActionWizardPage;
        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:            
                    mAddApiModelActionWizardPage = ((AddApiModelActionWizardPage)WizardEventArgs.Wizard);
                    SetParamsGrid();
                    break;
                case EventType.Active:            
                    ObservableList<EnhancedActInputValue> OldList = new ObservableList<EnhancedActInputValue>();
                    foreach (EnhancedActInputValue value in mAddApiModelActionWizardPage.EnhancedInputValueList)
                    {
                        EnhancedActInputValue oldVal = new EnhancedActInputValue();
                        oldVal.Guid = value.Guid;
                        oldVal.ParamGuid = value.ParamGuid;
                        oldVal.Param = value.Param;
                        oldVal.ParamType = value.ParamType;
                        oldVal.Value = value.Value;
                        oldVal.Description = value.Description;
                        oldVal.ExtraDetails = value.ExtraDetails;
                        OldList.Add(oldVal);
                    }

                    mAddApiModelActionWizardPage.EnhancedInputValueList.Clear();
                    foreach (ApplicationAPIModel aamb in mAddApiModelActionWizardPage.AAMList)
                    {
                        WorkSpace.Instance.RefreshGlobalAppModelParams(aamb);
                        MergeAndConvertToEnhancedList(aamb.MergedParamsList, OldList);
                    }
                    PopulateRelatedAPIsToParamsList();
                    SortDropDownValues();
                    break;
                //case EventType.Finish:            
                  
                //    break;            
            }
        }

        private void SortDropDownValues()
        {
            foreach (EnhancedActInputValue EAIV in mAddApiModelActionWizardPage.EnhancedInputValueList)
            {
                List<string> TempList = GingerCore.General.ConvertObservableListToList(EAIV.OptionalValues);
                TempList.Sort();
                EAIV.OptionalValues = GingerCore.General.ConvertListToObservableList(TempList);
            }
        }

        private void PopulateRelatedAPIsToParamsList()
        {
            foreach (EnhancedActInputValue EAIV in mAddApiModelActionWizardPage.EnhancedInputValueList)
            {
                EAIV.ExtraDetails = string.Empty ;
                foreach (ApplicationAPIModel aamb in mAddApiModelActionWizardPage.AAMList)
                {
                    EAIV.ExtraDetails += aamb.Name ;
                    if (mAddApiModelActionWizardPage.AAMList.IndexOf(aamb) != mAddApiModelActionWizardPage.AAMList.Count() - 1)
                        EAIV.ExtraDetails += " | ";
                }
            }
        }
        
        private void MergeAndConvertToEnhancedList(ObservableList<AppModelParameter> AppModelParameters, ObservableList<EnhancedActInputValue> OldList)
        {
            if (AppModelParameters != null)
            {
                foreach (AppModelParameter AMDP in AppModelParameters)
                {
                    if (AMDP.RequiredAsInput == true)
                    {
                        //Check if already exists in the list, if yes - add only its optional values and description
                        EnhancedActInputValue EAIV = null;
                        EAIV = mAddApiModelActionWizardPage.EnhancedInputValueList.Where(x => x.Param == AMDP.PlaceHolder).FirstOrDefault();
                        if (EAIV != null)
                        {
                            foreach (OptionalValue optionalValue in AMDP.OptionalValuesList)
                                if (!EAIV.OptionalValues.Contains(optionalValue.Value))
                                {
                                    if (optionalValue.IsDefault)
                                        EAIV.Value = optionalValue.Value;
                                    EAIV.OptionalValues.Add(optionalValue.Value);
                                }

                            if (!string.IsNullOrEmpty(EAIV.Description) && !EAIV.Description.Contains(AMDP.Description))
                                EAIV.Description += " | " + AMDP.Description;
                        }
                        else
                        {
                            //Not exists in the list, add it, his optional values and description
                            EnhancedActInputValue AIV = new EnhancedActInputValue();
                            AIV.ParamGuid = AMDP.Guid;
                            AIV.Param = AMDP.PlaceHolder;
                            AIV.Description = AMDP.Description;
                            string OldValue = string.Empty;
                            if (OldList != null && OldList.Count() > 0)
                            {
                                EnhancedActInputValue oldAIV = OldList.Where(x => x.ParamGuid == AMDP.Guid).FirstOrDefault();
                                if (oldAIV != null)
                                {
                                    OldValue = oldAIV.Value;
                                }
                                else
                                {
                                    OldValue = null;
                                }
                            }
                            foreach (OptionalValue optionalValue in AMDP.OptionalValuesList)
                            {
                                AIV.OptionalValues.Add(optionalValue.Value);

                                if (optionalValue.IsDefault)
                                    AIV.Value = optionalValue.Value;
                                else if (string.IsNullOrEmpty(AIV.Value))
                                    AIV.Value = optionalValue.Value;

                            }
                            if (!string.IsNullOrEmpty(OldValue))
                                AIV.Value = OldValue;

                            mAddApiModelActionWizardPage.EnhancedInputValueList.Add(AIV);
                        }
                    }
                }
            }
        }
              
        private void SetParamsGrid()
        {
            xAPIModelParamsValueUCGrid.Title = "API Parameters Consolidation";
            xAPIModelParamsValueUCGrid.SetTitleStyle((Style)TryFindResource("@ucGridTitleLightStyle"));
            xAPIModelParamsValueUCGrid.AddToolbarTool(eImageType.DataSource, "Map API Parameters to DataSource", new RoutedEventHandler(MapOutputToDataSource));

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(EnhancedActInputValue.Param), Header = "Parameter", WidthWeight = 20, ReadOnly = true, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(EnhancedActInputValue.Description), Header = "Description", WidthWeight = 20, ReadOnly = true, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(EnhancedActInputValue.ExtraDetails), Header = "Related API's", WidthWeight = 20, ReadOnly = true, AllowSorting = true});
            view.GridColsView.Add(new GridColView() { Field = nameof(EnhancedActInputValue.Value), Header = "Selected Value", StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(nameof(EnhancedActInputValue.OptionalValues), nameof(EnhancedActInputValue.Value), true), WidthWeight = 20, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xMainGrid.Resources["ValueExpressionButton"] });
            xAPIModelParamsValueUCGrid.SetAllColumnsDefaultView(view);
            xAPIModelParamsValueUCGrid.InitViewItems();
            xAPIModelParamsValueUCGrid.DataSourceList = mAddApiModelActionWizardPage.EnhancedInputValueList;
        }
        
        private void ValueExpressionButton_Click(object sender, RoutedEventArgs e)
        {
            EnhancedActInputValue AIV = (EnhancedActInputValue)xAPIModelParamsValueUCGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(AIV, nameof(ActInputValue.Value), mAddApiModelActionWizardPage.mContext);
            VEEW.ShowAsWindow();
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

                foreach (EnhancedActInputValue inputVal in mAddApiModelActionWizardPage.EnhancedInputValueList)
                {
                    string sColName = inputVal.Param.Replace("[", "_").Replace("]", "").Replace("{", "").Replace("}", "").Replace("<","").Replace(">","");
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
