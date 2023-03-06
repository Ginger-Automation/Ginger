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
using Amdocs.Ginger.Common.Repository.ApplicationModelLib;
using Amdocs.Ginger.Repository;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ginger.ApplicationModelsLib.ModelOptionalValue
{
    public class AddModelOptionalValuesWizard : WizardBase
    {
        public enum eSourceType
        {
            [EnumValueDescription("Excel")]
            Excel,
            [EnumValueDescription("XML")]
            XML,
            [EnumValueDescription("JSON")]
            Json,
            [EnumValueDescription("DataBase")]
            DB
        }
        public enum eOptionalValuesTargetType
        {
            ModelLocalParams,GlobalParams
        }
        public  ObservableList<TemplateFile> OVFList = new ObservableList<TemplateFile>();//optional values files list
        public  List<ParameterValues> ParameterValues = new List<ParameterValues>();// EXCEL & DB
        public  ImportOptionalValuesForParameters ImportOptionalValues = new ImportOptionalValuesForParameters();
        public  Dictionary<Tuple<string, string>, List<string>> OptionalValuesPerParameterDict;//XML&JSON
        public  ObservableList<GlobalAppModelParameter> mGlobalParamterList;
        public ApplicationModelBase mAAMB;
        public  ObservableList<AppModelParameter> ParamsList = new ObservableList<AppModelParameter>();//Grid presentation
        public  ObservableList<GlobalAppModelParameter> GlobalParamsList = new ObservableList<GlobalAppModelParameter>();//Grid presentation

        public eSourceType SourceType { get; set; }
        public AddModelOptionalValuesWizard(ApplicationModelBase AAMB)//Local Parameters
        {
            mAAMB = AAMB;
            ImportOptionalValues.ParameterType = ImportOptionalValuesForParameters.eParameterType.Local;
            AddPage(Name: "Select Document", Title: "Import Source", SubTitle: "Select And Import Source For Optional Values", Page: new AddOptionalValuesModelSelectTypePage(eOptionalValuesTargetType.ModelLocalParams));
            AddPage(Name: "Select Parameters", Title: "Select Parameters", SubTitle: "Select Parameters For The Chosen Values", Page: new AddOptionalValuesModelSelectParamPage(eOptionalValuesTargetType.ModelLocalParams));
        }
        public AddModelOptionalValuesWizard(ObservableList<GlobalAppModelParameter> GlobalParamterList)//Global Parameters
        {
            mGlobalParamterList = GlobalParamterList;
            ImportOptionalValues.ParameterType = ImportOptionalValuesForParameters.eParameterType.Global;
            AddPage(Name: "Select Document", Title: "Import Source", SubTitle: "Select And Import Source For Optional Values", Page: new AddOptionalValuesModelSelectTypePage(eOptionalValuesTargetType.GlobalParams));
            AddPage(Name: "Select Parameters", Title: "Select Parameters", SubTitle: "Select Parameters For The Chosen Values", Page: new AddOptionalValuesModelSelectParamPage(eOptionalValuesTargetType.GlobalParams));
        }
        public override string Title { get { return "Import Optional Values For Parameters Wizard"; } }

        public override void Finish()
        {
            ProcessStarted();
            if (SourceType == eSourceType.Excel)
            {
                if (ImportOptionalValues.ParameterType == ImportOptionalValuesForParameters.eParameterType.Local)
                {
                    ImportOptionalValues.PopulateExcelDBOptionalValuesForAPIParametersExcelDB(mAAMB, ParamsList.ToList<AppModelParameter>(), ParameterValues);
                    ParameterValues.Clear();
                }
                else if (ImportOptionalValues.ParameterType == ImportOptionalValuesForParameters.eParameterType.Global)
                {
                    ImportOptionalValues.PopulateExcelDBOptionalValuesForAPIParametersExcelDB(mGlobalParamterList, GlobalParamsList.ToList<GlobalAppModelParameter>(), ParameterValues);
                    ParameterValues.Clear();
                }
            }
            else if (SourceType == eSourceType.DB)
            {
                if (ImportOptionalValues.ParameterType == ImportOptionalValuesForParameters.eParameterType.Local)
                {
                    ImportOptionalValues.PopulateExcelDBOptionalValuesForAPIParametersExcelDB(mAAMB, ParamsList.ToList<AppModelParameter>(), ParameterValues);
                    ParameterValues.Clear();
                }
                else if (ImportOptionalValues.ParameterType == ImportOptionalValuesForParameters.eParameterType.Global)
                {
                    ImportOptionalValues.PopulateExcelDBOptionalValuesForAPIParametersExcelDB(mGlobalParamterList, GlobalParamsList.ToList<GlobalAppModelParameter>(), ParameterValues);
                    ParameterValues.Clear();
                }
            }
            else
            {
                OptionalValuesPerParameterDict = new Dictionary<Tuple<string, string>, List<string>>();
                if (ImportOptionalValues.ParameterType == ImportOptionalValuesForParameters.eParameterType.Local)
                {
                    ImportOptionalValues.GetAllOptionalValuesFromExamplesFiles(mAAMB, OptionalValuesPerParameterDict);
                    ImportOptionalValues.PopulateOptionalValuesForAPIParameters(mAAMB, OptionalValuesPerParameterDict, ParamsList.ToList<AppModelParameter>());
                }
                else if (ImportOptionalValues.ParameterType == ImportOptionalValuesForParameters.eParameterType.Global)
                {

                }
                if(mAAMB is ApplicationAPIModel)
                    ((ApplicationAPIModel)mAAMB).OptionalValuesTemplates.Clear();
            }
            ProcessEnded();
        }
    
        public bool IsDefaultPresentInParameterValues(ParameterValues parm)
        {
            bool isPresent = false;
            foreach (var item in parm.ParameterValuesByNameDic)
            {
                if(item.Contains("*"))
                {
                    isPresent = true;
                }
            }
            return isPresent;
        }

        public bool SetFirstDefaultValueInParameterValues(ParameterValues parm)
        {
            bool isPresent = false;
            if (parm.ParameterValuesByNameDic.Count > 0)
            {
                parm.ParameterValuesByNameDic[0] = string.Format("{0}*", parm.ParameterValuesByNameDic[0]);  
            }
            return isPresent;
        }
    }
}
