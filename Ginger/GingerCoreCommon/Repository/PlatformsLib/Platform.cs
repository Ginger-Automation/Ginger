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

using System.ComponentModel;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;

namespace GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib
{

    public enum ePlatformType
    {
        [EnumValueDescription("NA")]
        NA,
        [EnumValueDescription("Web")]
        Web,
        [EnumValueDescription("Mobile")]
        Mobile,  
        [EnumValueDescription("Unix")]
        Unix,
        [EnumValueDescription("ASCF")]
        ASCF,
        [EnumValueDescription("DOS")]
        DOS,
        [EnumValueDescription("Windows")]
        Windows,
        [EnumValueDescription("VB Script")]
        VBScript,
        [EnumValueDescription("Web Services")]
        WebServices,
        [EnumValueDescription("Power Builder")]
        PowerBuilder,
        [EnumValueDescription("Java")]
        Java,
        [EnumValueDescription("Mainframe")]
        MainFrame,
        [EnumValueDescription("Ginger Service")]       
        Service
        //AndroidDevice,  // rename to Android, join with Android
    }

    //public class Platform // : RepositoryItem
    //{
    //    // Move to Plugin
    //    // TODO: cleanup


    //    //[IsSerializedForLocalRepository]
    //    //public bool Active { get; set; }

    //    //[IsSerializedForLocalRepository]
    //    //public ePlatformType PlatformType { get; set; }

    //    //private string mAgentName;

    //    //[IsSerializedForLocalRepository]
    //    //public string AgentName
    //    //{
    //    //    get
    //    //    {
    //    //        if (Agent != null)
    //    //        {
    //    //            return this.Agent.Name;
    //    //        }
    //    //        else
    //    //        {
    //    //            return mAgentName;
    //    //        }
    //    //    }
    //    //    set
    //    //    {
    //    //        mAgentName = value;
    //    //    }
    //    //}

    //    //// Used when running after mapping done and user click run
    //    //private NewAgent mAgent;

    //    //public NewAgent Agent
    //    //{
    //    //    get { return mAgent; }
    //    //    set
    //    //    {
    //    //        mAgent = value;
    //    //        OnPropertyChanged(nameof(Agent));
    //    //        OnPropertyChanged(nameof(AgentName));
    //    //    }
    //    //}

    //    //public string Description
    //    //{
    //    //    get
    //    //    {
    //    //        //TODO: switch case, retur nice desc
    //    //        return PlatformType.ToString();
    //    //    }
    //    //}

    //    //public override string ItemName
    //    //{
    //    //    get
    //    //    {
    //    //        return string.Empty;
    //    //    }
    //    //    set
    //    //    {
    //    //        return;
    //    //    }
    //    //}
    //}
}
