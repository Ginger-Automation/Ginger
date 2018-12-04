#region License
/*
Copyright © 2014-2018 European Support Limited

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
using GingerCore.Helpers;
using GingerCore.Properties;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace GingerCore.Actions
{
    public class ActAgentManipulation : ActWithoutDriver
    {
        public new static partial class Fields
        {
            public static string AgentManipulationActionType = "AgentManipulationActionType";
        }

        public override string ActionDescription { get { return "Agent Manipulation Action"; } }
        public override string ActionUserDescription { get { return "Use this action to Start, Stop, Restart the agent in middle of the flow"; } }

        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
        {
            TBH.AddText("Use this action to Start, Stop, Restart the agent in middle of the flow");
            TBH.AddLineBreak();
        }

        public eAgenTManipulationActionType AgentManipulationActionType
        {
            get
            {
                eAgenTManipulationActionType eVal = eAgenTManipulationActionType.CloseAgent;
                if (Enum.TryParse<eAgenTManipulationActionType>(GetInputParamValue(Fields.AgentManipulationActionType), out eVal))
                    return eVal;
                else
                    return eAgenTManipulationActionType.CloseAgent;  //default value          
            }
        }

        public override void Execute()
        {
            switch ( AgentManipulationActionType)
            {
                case eAgenTManipulationActionType.CloseAgent:
                    if (RunOnBusinessFlow.CurrentActivity.CurrentAgent==null)
                    {
                        this.Error="Agent not running";
                    }

                    if(RunOnBusinessFlow.CurrentActivity.CurrentAgent.Status == Agent.eStatus.Completed || RunOnBusinessFlow.CurrentActivity.CurrentAgent.Status == Agent.eStatus.NotStarted)
                    {
                        this.Error = "Agent not running";
                    }
                    if (RunOnBusinessFlow.CurrentActivity.CurrentAgent.Status == Agent.eStatus.FailedToStart)
                    {
                        RunOnBusinessFlow.CurrentActivity.CurrentAgent.ResetAgentStatus(RunOnBusinessFlow.CurrentActivity.CurrentAgent.Status);
                        this.ExInfo = "Agent is not running, failed to start flag is reset.";
                    }

                    if (RunOnBusinessFlow.CurrentActivity.CurrentAgent.Status != Agent.eStatus.FailedToStart)
                        RunOnBusinessFlow.CurrentActivity.CurrentAgent.Close();
                    break;
                case eAgenTManipulationActionType.StartAgent:
                    if (RunOnBusinessFlow.CurrentActivity.CurrentAgent.Status == Agent.eStatus.Running)
                        break;
                    else if(RunOnBusinessFlow.CurrentActivity.CurrentAgent.Status == Agent.eStatus.Starting)
                    {
                        RunOnBusinessFlow.CurrentActivity.CurrentAgent.Close();
                    }
                    else if (RunOnBusinessFlow.CurrentActivity.CurrentAgent.Status == Agent.eStatus.FailedToStart)
                    {
                        RunOnBusinessFlow.CurrentActivity.CurrentAgent.ResetAgentStatus(RunOnBusinessFlow.CurrentActivity.CurrentAgent.Status);
                        this.ExInfo = "Failed to start flag is reset.";
                    }

                    StartAndValidateAgentStatus();
                    break;
                case eAgenTManipulationActionType.RestartAgent:
                    if (RunOnBusinessFlow.CurrentActivity.CurrentAgent == null)
                    {
                        this.Error = "Agent not running";
                    }

                    if (RunOnBusinessFlow.CurrentActivity.CurrentAgent.Status == Agent.eStatus.Completed || RunOnBusinessFlow.CurrentActivity.CurrentAgent.Status == Agent.eStatus.NotStarted)
                    {
                        this.Error = "Agent not running";
                    }
                    if (RunOnBusinessFlow.CurrentActivity.CurrentAgent.Status == Agent.eStatus.FailedToStart)
                    {
                        RunOnBusinessFlow.CurrentActivity.CurrentAgent.ResetAgentStatus(RunOnBusinessFlow.CurrentActivity.CurrentAgent.Status);
                        this.ExInfo = "Agent is not running, failed to start flag is reset.";
                    }
                    if (RunOnBusinessFlow.CurrentActivity.CurrentAgent.Status != Agent.eStatus.FailedToStart)
                        RunOnBusinessFlow.CurrentActivity.CurrentAgent.Close();
                    StartAndValidateAgentStatus();
                    break;
            }
        }

        public override string ActionEditPage { get { return "ActAgentManipulationEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

        public enum eAgenTManipulationActionType
        {
          CloseAgent,
          StartAgent,
          RestartAgent,
        }

        private void StartAndValidateAgentStatus()
        {
            RunOnBusinessFlow.CurrentActivity.CurrentAgent.DSList = DSList;
            RunOnBusinessFlow.CurrentActivity.CurrentAgent.StartDriver();
            RunOnBusinessFlow.CurrentActivity.CurrentAgent.WaitForAgentToBeReady();
            if (RunOnBusinessFlow.CurrentActivity.CurrentAgent.Status != Agent.eStatus.Running)
            {
                this.Error = "Failed to start agent";
            }
        }

        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    AddAllPlatforms();
                }
                return mPlatforms;
            }
        }

        public override String ActionType
        {
            get
            {
                return "AgentManipulationActionType";
            }
        }

        public override System.Drawing.Image Image { get { return Resources.ASCF16x16; } }
    }
}