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
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common;
using System.Diagnostics;
using Amdocs.Ginger.Common.InterfacesLib;
using Ginger.Reports;
using amdocs.ginger.GingerCoreNET;
using GingerCore;
using static Ginger.Run.RunSetActions.RunSetActionBase;

namespace Ginger.Run.RunSetActions
{
    public class RunSetActionBaseOperations : IRunSetActionBaseOperations
    {
        public RunSetActionBase runsetActionBase;
        public RunSetActionBaseOperations(RunSetActionBase runsetActionBase)
        {
            this.runsetActionBase = runsetActionBase;
            this.runsetActionBase.runSetActionBaseOperations = this;
        }


        public void ExecuteWithRunPageBFES()
        {
            ReportInfo RI = new ReportInfo(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, WorkSpace.Instance.RunsetExecutor);
            RunAction(RI);
        }

        public void RunAction(IReportInfo RI)
        {
            Reporter.ToStatus(eStatusMsgKey.ExecutingRunSetAction, null, runsetActionBase.Name);
            try
            {
                Reporter.ToLog(eLogLevel.INFO, string.Format("--> Execution Started for {0} Operation from Type '{1}', Operation Name= '{2}', Operation ID= '{3}'", GingerDicser.GetTermResValue(eTermResKey.RunSet), runsetActionBase.Type, runsetActionBase.Name, runsetActionBase.Guid));
                runsetActionBase.Status = eRunSetActionStatus.Running;
                runsetActionBase.Errors = null;

                Stopwatch st = new Stopwatch();
                st.Reset();
                st.Start();
                runsetActionBase.Execute(RI);
                st.Stop();
                runsetActionBase.Elapsed = st.ElapsedMilliseconds;

                // we change to completed only if still running and not changed to fail or soemthing else            
                if (runsetActionBase.Status == eRunSetActionStatus.Running)
                {
                    runsetActionBase.Status = eRunSetActionStatus.Completed;
                }

                Reporter.ToLog(eLogLevel.INFO, string.Format("<-- Execution Ended for {0} Operation from Type '{1}' and Name '{2}'", GingerDicser.GetTermResValue(eTermResKey.RunSet), runsetActionBase.Type, runsetActionBase.Name) + Environment.NewLine
                                                                + "Details:" + Environment.NewLine
                                                                + string.Format("Status= {0}", runsetActionBase.Status) + Environment.NewLine
                                                                + string.Format("Errors= {0}", runsetActionBase.Errors) + Environment.NewLine
                                                                + string.Format("Elapsed= {0}", runsetActionBase.Elapsed));
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, string.Format("<-- Execution Failed with exception for {0} Operation from Type '{1}' and Name '{2}' Exception: " + ex.Message, GingerDicser.GetTermResValue(eTermResKey.RunSet), runsetActionBase.Type, runsetActionBase.Name), ex);
                runsetActionBase.Status = eRunSetActionStatus.Failed;
            }
            finally
            {
                Reporter.HideStatusMessage();
            }
        }

    }
}

