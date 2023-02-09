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

using System;
using System.Threading.Tasks;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.CoreNET.ActionsLib.ActionsConversion;
using Amdocs.Ginger.Repository;
using Ginger.Actions.ActionConversion;
using Ginger.WizardLib;
using GingerCore;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;

namespace Ginger.Actions.ApiActionsConversion
{
    /// <summary>
    /// This class is used to ApiActionsConversionWizard 
    /// </summary>
    public class ApiActionsConversionWizard : WizardBase, IActionsConversionProcess
    {
        RepositoryFolder<ApplicationAPIModel> mAPIModelFolder;

        public override string Title { get { return "Convert Web services Actions"; } }

        private ObservableList<BusinessFlowToConvert> mListOfBusinessFlow = null;
        public ObservableList<BusinessFlowToConvert> ListOfBusinessFlow
        {
            get {
                return mListOfBusinessFlow;
            }
            set
            {
                mListOfBusinessFlow = value;
            }
        }

        public bool ParameterizeRequestBody { get; set; }

        public bool PullValidations { get; set; }

        public eModelConversionType ModelConversionType 
        { 
            get
            {
                return eModelConversionType.ApiActionConversion;
            }
        }

        public ObservableList<ConvertableActionDetails> ActionToBeConverted = new ObservableList<ConvertableActionDetails>();
        ConversionStatusReportPage mReportPage = null;
        ApiActionConversionUtils mConversionUtils = new ApiActionConversionUtils();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public ApiActionsConversionWizard(RepositoryFolder<ApplicationAPIModel> apiModelFolder)
        {
            mAPIModelFolder = apiModelFolder;
            ListOfBusinessFlow = GetBusinessFlowsToConvert(); 

            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Web services Actions Conversion Introduction", Page: new WizardIntroPage("/ApplicationModelsLib/APIModels/ApiActionsConversionWizard/ApiActionsConversionIntro.md"));

            string businessFlow = GingerDicser.GetTermResValue(eTermResKey.BusinessFlows);
            AddPage(Name: "Select " + businessFlow + " for Conversion", Title: "Select " + businessFlow + " for Conversion", SubTitle: "Select " + businessFlow + " for Conversion", Page: new SelectBusinessFlowWzardPage(ListOfBusinessFlow, new Context()));
            AddPage(Name: "Conversion Configurations", Title: "Conversion Configurations", SubTitle: "Conversion Configurations", Page: new ApiConversionConfigurationWzardPage());

            mReportPage = new ConversionStatusReportPage(ListOfBusinessFlow);
            AddPage(Name: "Conversion Status Report", Title: "Conversion Status Report", SubTitle: "Conversion Status Report", Page: mReportPage);
        }

        /// <summary>
        /// This method is used to ge the businessflows to convert
        /// </summary>
        /// <param name="businessFlows"></param>
        /// <returns></returns>
        private ObservableList<BusinessFlowToConvert> GetBusinessFlowsToConvert()
        {
            ObservableList<BusinessFlow> businessFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
            ObservableList <BusinessFlowToConvert> lst = new ObservableList<BusinessFlowToConvert>();           
            Parallel.ForEach(businessFlows, bf =>
            {
                if (IsWebServiceTargetApplicationInFlow(bf))
                {
                    BusinessFlowToConvert flowToConvert = new BusinessFlowToConvert();
                    flowToConvert.BusinessFlow = bf;
                    flowToConvert.TotalProcessingActionsCount = mConversionUtils.GetConvertibleActionsCountFromBusinessFlow(bf);
                    if (flowToConvert.TotalProcessingActionsCount > 0)
                    {
                        lst.Add(flowToConvert);
                    }
                }
            });
            return lst;
        }

        /// <summary>
        /// This method is used to check if WebService TargetApplication is present in BusinessFlow
        /// </summary>
        /// <param name="bf"></param>
        /// <returns></returns>
        private bool IsWebServiceTargetApplicationInFlow(BusinessFlow bf)
        {
            bool isPresent = false;            
            foreach (var ta in bf.TargetApplications)
            {
                ePlatformType platformType = WorkSpace.Instance.Solution.GetApplicationPlatformForTargetApp(ta.ItemName);
                isPresent = platformType == ePlatformType.WebServices;
                if(isPresent)
                {
                    break;
                }
            }
            return isPresent;
        }

        /// <summary>
        /// This is finish method which does the finish the wizard functionality
        /// </summary>
        public override void Finish()
        {
            base.mWizardWindow.Close();
        }

        /// <summary>
        /// This method is used to Stop the conversion process in between conversion process
        /// </summary>
        public void StopConversion()
        {
            mConversionUtils.StopConversion();
        }

        /// <summary>
        /// This method is used to convert the action in case of Continue & Re-Convert
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="isReConvert"></param>
        public async Task ProcessConversion(ObservableList<BusinessFlowToConvert> lst, bool isReConvert)
        {
            ProcessStarted();
            try
            {
                ObservableList<BusinessFlowToConvert> flowsToConvert = new ObservableList<BusinessFlowToConvert>();
                if (isReConvert)
                {
                    ObservableList<BusinessFlowToConvert> selectedLst = new ObservableList<BusinessFlowToConvert>();
                    foreach (var bf in lst)
                    {
                        if (bf.IsSelected)
                        {
                            bf.BusinessFlow.RestoreFromBackup(true);
                            bf.ConversionStatus = eConversionStatus.Pending;
                            bf.SaveStatus = eConversionSaveStatus.Pending;
                            flowsToConvert.Add(bf);
                        }
                    }
                }
                else
                {
                    flowsToConvert = ListOfBusinessFlow;
                }

                if (flowsToConvert.Count > 0)
                {
                    await Task.Run(() => mConversionUtils.ConvertToApiActionsFromBusinessFlows(flowsToConvert, ParameterizeRequestBody, PullValidations, mAPIModelFolder)).ConfigureAwait(true);
                }
                mReportPage.SetButtonsVisibility(true);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to convert", ex);
            }
            finally
            {
                ProcessEnded();
            }
        }

        /// <summary>
        /// This method is used to convert the actions
        /// </summary>
        /// <param name="lst"></param>
        public async Task BusinessFlowsActionsConversion(ObservableList<BusinessFlowToConvert> lst)
        {
            try
            {
                ProcessStarted();

                await Task.Run(() => mConversionUtils.ConvertToApiActionsFromBusinessFlows(lst, ParameterizeRequestBody, PullValidations, mAPIModelFolder)).ConfigureAwait(true);

                mReportPage.SetButtonsVisibility(true);

                ProcessEnded();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to convert", ex);
            }
            finally
            {
                Reporter.HideStatusMessage();
            }
        }

        /// <summary>
        /// This method is used to get the Convertible Actions Count From BusinessFlow
        /// </summary>
        /// <param name="bf"></param>
        /// <returns></returns>
        public int GetConvertibleActionsCountFromBusinessFlow(BusinessFlow bf)
        {
            return mConversionUtils.GetConvertibleActionsCountFromBusinessFlow(bf);
        }

        /// <summary>
        /// This method is used to cancle the wizard
        /// </summary>
        public override void Cancel()
        {
            base.Cancel();
        }

        public void ConversionProcessEnded()
        {
            ProcessStarted();
        }

        public void ConversionProcessStarted()
        {
            ProcessEnded();
        }
    }
}
