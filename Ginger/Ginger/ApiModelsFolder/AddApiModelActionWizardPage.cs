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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.WebServices.WebAPI;
using GingerWPF.WizardLib;
using System.Linq;
using amdocs.ginger.GingerCoreNET;
using System;

namespace Ginger.ApiModelsFolder
{
    public class AddApiModelActionWizardPage : WizardBase
    {
        public Context mContext;
        private ObservableList<ApplicationAPIModel> mAAMList = new ObservableList<ApplicationAPIModel>();

        ObservableList<IAct> mActions;

        public AddApiModelActionWizardPage(Context context, ObservableList<ApplicationAPIModel> APIModelsList = null)
        {
            mContext = context;
            mActions = mContext.BusinessFlow.CurrentActivity.Acts;

            if (APIModelsList != null)
                mAAMList = APIModelsList;

            AddPage(Name: "API Models", Title: "Select API Model", SubTitle: "Choose one or more API's Models to create actions", Page: new APIModelSelectionWizardPage(context));
            AddPage(Name: "API Parameters", Title: "Set API Model Parameters", SubTitle: "set API Model Parameters", Page: new APIModelParamsWizardPage());
        }

        public AddApiModelActionWizardPage(Context context)
        {
            mContext = context;
            mActions = mContext.BusinessFlow.CurrentActivity.Acts;

            AddPage(Name: "API Models", Title: "Select API Model", SubTitle: "Choose one or more API's Models to create actions", Page: new APIModelSelectionWizardPage(context));
            AddPage(Name: "API Parameters", Title: "Set API Model Parameters", SubTitle: "set API Model Parameters", Page: new APIModelParamsWizardPage());
        }

        public override string Title { get { return "Add API Model Wizard"; } }

        public ObservableList<ApplicationAPIModel> AAMList
        {
            get { return mAAMList; }
            set
            {
                if(mAAMList != value)
                {
                    mAAMList = value;
                }
            }
        }

        private ObservableList<EnhancedActInputValue> mEnhancedInputValueList = new ObservableList<EnhancedActInputValue>();
        public ObservableList<EnhancedActInputValue> EnhancedInputValueList
        {
            get { return mEnhancedInputValueList; }
            set
            {
                if (mEnhancedInputValueList != value)
                {
                    mEnhancedInputValueList = value;
                }
            }
        }

        public override void Finish()
        {
            foreach (ApplicationAPIModel aamb in AAMList)
            {
                ActWebAPIModel aNew = new ActWebAPIModel();

                aNew.Description = aamb.Name + "- API Model Execution";
                aNew.Active = true;
                aNew.SupportSimulation = aamb.SupportSimulation;
                aNew.APImodelGUID = aamb.Guid;
                aNew.APIModelParamsValue = GetEnhancedUpdatedParams(aamb.MergedParamsList);
                aNew.ReturnValues = ConvertTemplateReturnValues(aamb.ReturnValues);
                aNew.AddNewReturnParams = true;
                aNew.SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();


                mActions.Add(aNew);

                //adding the new act after the selected action in the grid  
                
                int selectedActIndex = -1;
                if (mActions.CurrentItem != null)
                {
                    selectedActIndex = mActions.IndexOf((Act)mActions.CurrentItem);
                }

                if (selectedActIndex >= 0)
                {
                    mActions.Move(mActions.Count - 1, selectedActIndex + 1);
                }
            }
        }


        private ObservableList<ActReturnValue> ConvertTemplateReturnValues(ObservableList<ActReturnValue> modelReturnValues)
        {
            ObservableList<ActReturnValue> returnValuesList = new ObservableList<ActReturnValue>();
            foreach (ActReturnValue modelRV in modelReturnValues)
            {
                ActReturnValue rv = new ActReturnValue();
                rv.AddedAutomatically = true;
                rv.Guid = modelRV.Guid;
                rv.Active = modelRV.Active;
                rv.Param = modelRV.Param;
                rv.Path = modelRV.Path;
                rv.Expected = modelRV.Expected;
                if (!string.IsNullOrEmpty(modelRV.StoreToValue))
                {
                    rv.StoreTo = ActReturnValue.eStoreTo.ApplicationModelParameter;
                    rv.StoreToValue = modelRV.StoreToValue;
                }
                returnValuesList.Add(rv);
            }
            return returnValuesList;
        }

        private ObservableList<EnhancedActInputValue> GetEnhancedUpdatedParams(ObservableList<AppModelParameter> paramsList)
        {
            ObservableList<EnhancedActInputValue> enhancedParamsList = new ObservableList<EnhancedActInputValue>();
            foreach (AppModelParameter ADP in paramsList)
            {
                if (ADP.RequiredAsInput == true)
                {
                    EnhancedActInputValue AIV = new EnhancedActInputValue();
                    AIV.ParamGuid = ADP.Guid;
                    AIV.Param = ADP.PlaceHolder;
                    AIV.Description = ADP.Description;
                    foreach (OptionalValue optionalValue in ADP.OptionalValuesList)
                        AIV.OptionalValues.Add(optionalValue.Value);

                    EnhancedActInputValue EAIV = null;
                    EAIV = EnhancedInputValueList.Where(x => x.Param == ADP.PlaceHolder).FirstOrDefault();
                    if (EAIV != null)
                    {
                        AIV.Value = EAIV.Value;
                    }
                    else
                    {
                        OptionalValue ov = null;
                        ov = ADP.OptionalValuesList.Where(x => x.IsDefault == true).FirstOrDefault();
                        if (ov != null)
                            AIV.Value = ov.Value;
                        //No Default, and no value selected - what to put
                    }

                    enhancedParamsList.Add(AIV);
                }
            }
            return enhancedParamsList;
        }

    }
}
