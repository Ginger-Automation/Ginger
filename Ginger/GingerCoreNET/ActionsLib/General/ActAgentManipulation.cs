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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using GingerCore.Helpers;
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

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
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
                    if (((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent)==null)
                    {
                        this.Error= "Agent is not mapped for the activity";
                    }

                    if(((AgentOperations)((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations).Status == Agent.eStatus.Completed || ((AgentOperations)((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations).Status == Agent.eStatus.NotStarted)
                    {
                        this.ExInfo = "Agent is not running";
                    }
                    if (((AgentOperations)((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations).Status == Agent.eStatus.FailedToStart)
                    {
                        ((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations.ResetAgentStatus(((AgentOperations)((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations).Status);
                        this.ExInfo = "Agent is not running, failed to start status is reset.";
                    }

                    if (((AgentOperations)((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations).Status != Agent.eStatus.FailedToStart)
                        ((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations.Close();
                    break;
                case eAgenTManipulationActionType.StartAgent:
                    if (((AgentOperations)((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations).Status == Agent.eStatus.Running)
                        break;
                    else if(((AgentOperations)((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations).Status == Agent.eStatus.Starting)
                    {
                        ((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations.Close();
                    }else if (((AgentOperations)((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations).Status == Agent.eStatus.FailedToStart)
                    {
                        ((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations.ResetAgentStatus(((AgentOperations)((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations).Status);
                        this.ExInfo = "Agent is not running, failed to start status is reset.";
                    }
                    ((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).DSList = DSList;
                    ((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations.StartDriver();
                        
                    break;
                case eAgenTManipulationActionType.RestartAgent:
                    if (((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent) == null)
                    {
                        this.Error = "Agent is not mapped for the activity";
                    }

                    if (((AgentOperations)((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations).Status == Agent.eStatus.Completed || ((AgentOperations)((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations).Status == Agent.eStatus.NotStarted)
                    {
                        this.ExInfo = "Agent is not running";
                    }
                    if (((AgentOperations)((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations).Status == Agent.eStatus.FailedToStart)
                    {
                        ((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations.ResetAgentStatus(((AgentOperations)((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations).Status);
                        this.ExInfo = "Agent is not running, failed to start status is reset.";
                    }
                    if (((AgentOperations)((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations).Status != Agent.eStatus.FailedToStart)
                        ((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations.Close();
                    ((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).DSList = DSList;
                    ((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations.StartDriver();              
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
            ((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).DSList = DSList;
            ((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations.StartDriver();
            ((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations.WaitForAgentToBeReady();
            if (((AgentOperations)((Agent)RunOnBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations).Status != Agent.eStatus.Running)
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

        public override eImageType Image { get { return eImageType.Agent; } }
    }
}
