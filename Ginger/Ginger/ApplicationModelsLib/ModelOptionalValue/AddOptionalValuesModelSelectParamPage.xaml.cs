#region License
/*
Copyright © 2014-2023 European Support Limited

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
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static Ginger.ApplicationModelsLib.ModelOptionalValue.AddModelOptionalValuesWizard;

namespace Ginger.ApplicationModelsLib.ModelOptionalValue
{
    /// <summary>
    /// Interaction logic for AddOptionalValuesModelSelectParamPage.xaml
    /// </summary>
    public partial class AddOptionalValuesModelSelectParamPage : Page, IWizardPage
    {
        public AddModelOptionalValuesWizard AddModelOptionalValuesWizard;
        
        ObservableList<AppModelParameter> OriginalAppModelParameters = new ObservableList<AppModelParameter>();
        eOptionalValuesTargetType mOptionalValuesTargetType;


        string GridPlaceholderHeader = "Place Holder";

        public AddOptionalValuesModelSelectParamPage(eOptionalValuesTargetType OptionalValuesTargetType)
        {
            InitializeComponent();
            mOptionalValuesTargetType = OptionalValuesTargetType;
            switch (mOptionalValuesTargetType)
            {
                case eOptionalValuesTargetType.ModelLocalParams:
                    
                    break;
                case eOptionalValuesTargetType.GlobalParams:
                   
                    break;
            }
            
            
        }
       
        private void InitParametersListForGrid()
        {
            if (AddModelOptionalValuesWizard.ImportOptionalValues.ParameterType == ImportOptionalValuesForParameters.eParameterType.Global)
            {
                InitGlobalParametersListForGrid();
                xModelParametersGrid.DataSourceList = AddModelOptionalValuesWizard.GlobalParamsList;
            }
            else if (AddModelOptionalValuesWizard.ImportOptionalValues.ParameterType == ImportOptionalValuesForParameters.eParameterType.Local)
            {
                InitLocalParametersListForGrid();
                xModelParametersGrid.DataSourceList = AddModelOptionalValuesWizard.ParamsList;
            }
        }
        private void InitLocalParametersListForGrid()
        {
            AddModelOptionalValuesWizard.ParamsList = new ObservableList<AppModelParameter>();
            foreach (AppModelParameter AMP in AddModelOptionalValuesWizard.mAAMB.AppModelParameters)
            {
                if (AddModelOptionalValuesWizard.SourceType == eSourceType.Excel || AddModelOptionalValuesWizard.SourceType == eSourceType.DB)
                {
                    var item = AddModelOptionalValuesWizard.ParameterValues.FirstOrDefault(o => o.ParamName == AMP.ItemName);
                    if (item == null)
                    { 
                        continue;
                    }
                }
                AppModelParameter temp = new AppModelParameter(AMP.PlaceHolder, AMP.Description, AMP.TagName, AMP.Path, new ObservableList<OptionalValue>());
                AddModelOptionalValuesWizard.ParamsList.Add(temp);
            }

            foreach (var parm in AddModelOptionalValuesWizard.ParameterValues)
            {
                if (!AddModelOptionalValuesWizard.IsDefaultPresentInParameterValues(parm))
                {
                    AddModelOptionalValuesWizard.SetFirstDefaultValueInParameterValues(parm);
                }
                var item = AddModelOptionalValuesWizard.mAAMB.AppModelParameters.SingleOrDefault(x => x.Key.ItemName == parm.ParamName);
                if (item == null)
                {
                    AppModelParameter temp = new AppModelParameter();
                    temp.Guid = Guid.NewGuid();
                    temp.ExecutionValue = string.Empty;
                    temp.PlaceHolder = parm.ParamName;
                    temp.Description = parm.Description;
                    temp.OptionalValuesList = new ObservableList<OptionalValue>();
                    SetOptionalValueList(parm, temp);
                    AddModelOptionalValuesWizard.ParamsList.Add(temp);
                }
                else
                {                    
                    item.ItemName = parm.ParamName;
                    item.Description = parm.Description;
                    item.OptionalValuesList = new ObservableList<OptionalValue>();
                    SetOptionalValueList(parm, item);                    
                }
            }
        }        

        private void InitGlobalParametersListForGrid()
        {
            AddModelOptionalValuesWizard.GlobalParamsList = new ObservableList<GlobalAppModelParameter>();
            foreach (GlobalAppModelParameter GAMP in AddModelOptionalValuesWizard.mGlobalParamterList)
            {
                if (AddModelOptionalValuesWizard.SourceType == eSourceType.Excel || AddModelOptionalValuesWizard.SourceType == eSourceType.DB)
                {
                    var item = AddModelOptionalValuesWizard.ParameterValues.FirstOrDefault(o => o.ParamName == GAMP.ItemName);
                    if (item == null)
                    { 
                        continue;
                    }
                }
                GlobalAppModelParameter temp = new GlobalAppModelParameter();
                temp.Guid = GAMP.Guid;                
                temp.CurrentValue = GAMP.CurrentValue;
                temp.PlaceHolder = GAMP.PlaceHolder;
                temp.Description = GAMP.Description;
                temp.OptionalValuesList = new ObservableList<OptionalValue>();
                AddModelOptionalValuesWizard.GlobalParamsList.Add(temp);
            }

            foreach (var parm in AddModelOptionalValuesWizard.ParameterValues)
            {
                if (!AddModelOptionalValuesWizard.IsDefaultPresentInParameterValues(parm))
                {
                    AddModelOptionalValuesWizard.SetFirstDefaultValueInParameterValues(parm);
                }
                var item = AddModelOptionalValuesWizard.mGlobalParamterList.SingleOrDefault(x => x.Key.ItemName == parm.ParamName);
                if (item == null)
                {                    
                    GlobalAppModelParameter temp = new GlobalAppModelParameter();
                    temp.Guid = Guid.NewGuid();
                    temp.CurrentValue = string.Empty;
                    temp.PlaceHolder = parm.ParamName;
                    temp.Description = parm.Description;
                    temp.OptionalValuesList = new ObservableList<OptionalValue>() { new OptionalValue { Value = "{Current Value}", IsDefault = true } };
                    SetOptionalValueList(parm, temp);
                    AddModelOptionalValuesWizard.GlobalParamsList.Add(temp);
                }
                else
                {
                    item.ItemName = parm.ParamName;
                    item.Description = parm.Description;
                    item.OptionalValuesList = new ObservableList<OptionalValue>();
                    SetOptionalValueList(parm, item);
                }
            }
        }

        /// <summary>
        /// This method is used to set the optional value list
        /// </summary>
        /// <param name="parm"></param>
        /// <param name="temp"></param>
        private void SetOptionalValueList(ParameterValues parm, AppModelParameter temp)
        {
            if (parm.ParameterValuesByNameDic != null && parm.ParameterValuesByNameDic.Count > 0)
            {
                foreach (var val in parm.ParameterValuesByNameDic)
                {
                    temp.OptionalValuesList.Add(new OptionalValue { Value = val });
                }
            }            
        }

        private void InitxModelParametersGrid()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            if (AddModelOptionalValuesWizard.ImportOptionalValues.ParameterType == ImportOptionalValuesForParameters.eParameterType.Local)
            {
                view.GridColsView.Add(new GridColView() { Field = nameof(AppModelParameter.RequiredAsInput), Header = "Selected", WidthWeight = 30, MaxWidth = 220, StyleType = GridColView.eGridColStyleType.CheckBox });
                view.GridColsView.Add(new GridColView() { Field = nameof(AppModelParameter.PlaceHolder), Header = GridPlaceholderHeader, WidthWeight = 100, ReadOnly = true });
            }
            else if (AddModelOptionalValuesWizard.ImportOptionalValues.ParameterType == ImportOptionalValuesForParameters.eParameterType.Global)
            {
                view.GridColsView.Add(new GridColView() { Field = nameof(GlobalAppModelParameter.RequiredAsInput), Header = "Selected", WidthWeight = 30, MaxWidth = 220, StyleType = GridColView.eGridColStyleType.CheckBox });
                view.GridColsView.Add(new GridColView() { Field = nameof(GlobalAppModelParameter.PlaceHolder), Header = GridPlaceholderHeader, WidthWeight = 100, ReadOnly = true });
            }

            xModelParametersGrid.SetAllColumnsDefaultView(view);
            xModelParametersGrid.InitViewItems();

            xModelParametersGrid.Grid.CanUserDeleteRows = false;
            xModelParametersGrid.ShowTitle = Visibility.Collapsed;
            xModelParametersGrid.ShowRefresh = Visibility.Collapsed;
            xModelParametersGrid.ShowUpDown = Visibility.Collapsed;
            xModelParametersGrid.ShowAdd = Visibility.Collapsed;
            xModelParametersGrid.ShowClearAll = Visibility.Collapsed;
            xModelParametersGrid.ShowDelete = Visibility.Collapsed;
            xModelParametersGrid.ShowEdit = Visibility.Collapsed;
            xModelParametersGrid.ShowCopyCutPast = Visibility.Collapsed;
            xModelParametersGrid.btnMarkAll.Visibility = Visibility.Visible;
            xModelParametersGrid.MarkUnMarkAllActive += MarkUnMarkAllActions;
        }
        private void MarkUnMarkAllActions(bool ActiveStatus)
        {
            if (xModelParametersGrid.DataSourceList.Count <= 0) return;
            if (xModelParametersGrid.DataSourceList.Count > 0)
            {
                if (AddModelOptionalValuesWizard.ImportOptionalValues.ParameterType == ImportOptionalValuesForParameters.eParameterType.Local)
                {
                    ObservableList<AppModelParameter> lstMarkUnMarkAPI = (ObservableList<AppModelParameter>)xModelParametersGrid.DataSourceList;
                    foreach (AppModelParameter AAMB in lstMarkUnMarkAPI)
                    {
                        AAMB.RequiredAsInput = ActiveStatus;
                    }
                    xModelParametersGrid.DataSourceList = lstMarkUnMarkAPI;
                }
                else if (AddModelOptionalValuesWizard.ImportOptionalValues.ParameterType == ImportOptionalValuesForParameters.eParameterType.Global)
                {
                    ObservableList<GlobalAppModelParameter> lstMarkUnMarkAPI = (ObservableList<GlobalAppModelParameter>)xModelParametersGrid.DataSourceList;
                    foreach (GlobalAppModelParameter AAMB in lstMarkUnMarkAPI)
                    {
                        AAMB.RequiredAsInput = ActiveStatus;
                    }
                    xModelParametersGrid.DataSourceList = lstMarkUnMarkAPI;
                }
            }
        }
        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            if (WizardEventArgs.EventType == EventType.Init)
            {
                AddModelOptionalValuesWizard = ((AddModelOptionalValuesWizard)WizardEventArgs.Wizard);
                InitxModelParametersGrid();
            }
            else if (WizardEventArgs.EventType == EventType.Active)
            {
                InitParametersListForGrid();
                // AddModelOptionalValuesWizard.FinishEnabled = true;
                // AddModelOptionalValuesWizard.NextEnabled = false;
            }
           
            
            else if (WizardEventArgs.EventType == EventType.Validate)
            {
            }
            else if (WizardEventArgs.EventType == EventType.Cancel)
            {
                if (AddModelOptionalValuesWizard.ImportOptionalValues.ParameterType == ImportOptionalValuesForParameters.eParameterType.Local)
                {
                    if(AddModelOptionalValuesWizard.mAAMB is ApplicationAPIModel)
                        ((ApplicationAPIModel)AddModelOptionalValuesWizard.mAAMB).OptionalValuesTemplates.Clear();
                    AddModelOptionalValuesWizard.ParameterValues.Clear();
                }
                else if (AddModelOptionalValuesWizard.ImportOptionalValues.ParameterType == ImportOptionalValuesForParameters.eParameterType.Global)
                {
                    AddModelOptionalValuesWizard.ParameterValues.Clear();
                }
            }
        }
    }
}
