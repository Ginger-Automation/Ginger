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

using Ginger.Run;
using GingerCore;
using GingerCore.FlowControlLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ginger.AnalyzerLib
{
    public class AnalyzeRunnerBusinessFlow : AnalyzerItemBase
    {
        public BusinessFlow mBusinessFlow { get; set; }
        public static List<AnalyzerItemBase> Analyze(GingerExecutionEngine GR, BusinessFlow BusinessFlow)
        {
            List<AnalyzerItemBase> IssuesList = new List<AnalyzerItemBase>();

            //code added to analyze for BFFlowControls in Runner BF.
            if (BusinessFlow.BFFlowControls.Count > 0)
            {
                foreach (FlowControl f in BusinessFlow.BFFlowControls)
                {
                    if (f.Active == true)
                    {
                        if (f.BusinessFlowControlAction == eBusinessFlowControlAction.GoToBusinessFlow)
                        {
                            string GoToBusinessFlow = f.GetNameFromValue();
                            BusinessFlow bf = null;
                            Guid guidToLookBy = Guid.Empty;

                            if (!string.IsNullOrEmpty(f.GetGuidFromValue().ToString()))
                            {
                                guidToLookBy = Guid.Parse(f.GetGuidFromValue().ToString());
                            }

                            List<BusinessFlow> lstBusinessFlow = null;
                            if (guidToLookBy != Guid.Empty)
                                lstBusinessFlow =  GR.BusinessFlows.Where(x => x.InstanceGuid == guidToLookBy).ToList();

                            if (lstBusinessFlow == null || lstBusinessFlow.Count == 0)
                                bf = null;
                            else if (lstBusinessFlow.Count == 1)
                                bf =(BusinessFlow) lstBusinessFlow[0];
                            else//we have more than 1
                            {
                                BusinessFlow firstActive =(BusinessFlow) lstBusinessFlow.Where(x => x.Active == true).FirstOrDefault();
                                if (firstActive != null)
                                    bf = firstActive;
                                else
                                    bf =(BusinessFlow) lstBusinessFlow[0];//no one is Active so returning the first one
                            }
                            if (bf == null)
                            {
                                AnalyzeRunnerBusinessFlow ABF = CreateNewIssue(IssuesList, GR, BusinessFlow);
                                ABF.Description = "Flow control is mapped to " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " which does not exist";
                                ABF.Details = "'" + GoToBusinessFlow + "' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " does not exist in the '" + GR.GingerRunner.Name + " ' " + GingerDicser.GetTermResValue(eTermResKey.RunSet);
                                ABF.HowToFix = "Remap the Flow Control Action";
                                ABF.IssueType = eType.Error;
                                ABF.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                                ABF.Impact = "Flow Control will fail on run time";
                                ABF.Severity = eSeverity.High;
                            }
                        }
                        if (f.BusinessFlowControlAction == eBusinessFlowControlAction.SetVariableValue)
                        {
                            if (string.IsNullOrEmpty(f.Value) || ValueExpression.IsThisDynamicVE(f.Value) == false)
                            {
                                string SetVariableValue = f.GetNameFromValue();
                                string[] vals = SetVariableValue.Split(new char[] { '=' });
                                if ((BusinessFlow.GetAllHierarchyVariables().Where(x => x.Name == vals[0].Trim()).Select(x => x.Name).FirstOrDefault() == null))
                                {
                                    AnalyzeRunnerBusinessFlow ABF = CreateNewIssue(IssuesList, GR, BusinessFlow);
                                    ABF.Description = "Flow control mapped to " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " which does not exist"; ;
                                    ABF.Details = "'" + vals[0].Trim() + "' " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " does not exist in parent items";
                                    ABF.HowToFix = "Remap the Flow Control Action";
                                    ABF.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                                    ABF.IssueType = eType.Error;
                                    ABF.Impact = "Flow Control will fail on run time";
                                    ABF.Severity = eSeverity.High;
                                }
                            }
                        }
                    }
                }
            }

            IssuesList.AddRange(AnalyzeBusinessFlow.AnalyzeForMissingMandatoryInputValues(BusinessFlow));

            return IssuesList;
        }

        private static AnalyzeRunnerBusinessFlow CreateNewIssue(List<AnalyzerItemBase> IssuesList, GingerExecutionEngine gr, BusinessFlow BusinessFlow)
        {
            AnalyzeRunnerBusinessFlow ABF = new AnalyzeRunnerBusinessFlow();
            ABF.Status = AnalyzerItemBase.eStatus.NeedFix;
            ABF.mBusinessFlow = BusinessFlow;
            ABF.ItemName = BusinessFlow.Description;
            ABF.ItemParent = gr.GingerRunner.Name + " > " + BusinessFlow.Name;
            ABF.mBusinessFlow = BusinessFlow;
            ABF.ItemClass = "BusinessFlow";
            IssuesList.Add(ABF);
            return ABF;
        }
    }
}
