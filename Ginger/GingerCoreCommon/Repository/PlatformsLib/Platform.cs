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

using Amdocs.Ginger.Repository;

namespace GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib
{

    public enum ePlatformType
    {
        //TODO: add Any
        //Null = 0,
        NA,
        Web,
        Mobile,  // TODO: remove and use Android or IOS
        Unix,
        ASCF,
        //Tuxedo = 6,
        //DOTNET,
        DOS,
        Windows,
        VBScript,
        WebServices,
        PowerBuilder,
        Java,
        MainFrame,
        //Android,
        AndroidDevice,  // rename to Android, join with Android
        IOS
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
