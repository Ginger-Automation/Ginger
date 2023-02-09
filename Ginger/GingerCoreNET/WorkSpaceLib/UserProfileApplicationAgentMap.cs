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

//using Amdocs.Ginger.Repository;
//using Amdocs.Ginger.Common.Repository;
//using Amdocs.Ginger.CoreNET.SolutionRepositoryLib.RepositoryObjectsLib;
//using GingerCoreNET.SolutionRepositoryLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
//using System;

//namespace Amdocs.Ginger.CoreNET.WorkSpaceLib
//{
//    public class UserProfileApplicationAgentMap : RepositoryItem
//    {
//        [IsSerializedForLocalRepository]
//        public RepositoryItemKey ApplicationLink { get; set; }
        
//        [IsSerializedForLocalRepository]
//        public RepositoryItemKey AgentLink { get; set; }

//        public override string ItemName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        
//        public UserProfileApplicationAgentMap(ApplicationPlatform applicationPlatform, NewAgent agent)
//        {
//            ApplicationLink.ItemName = applicationPlatform.AppName;
//            ApplicationLink.Guid = applicationPlatform.Guid;

//            AgentLink.ItemName = agent.Name;
//            AgentLink.Guid = agent.Guid;
//        }
//    }
//}
