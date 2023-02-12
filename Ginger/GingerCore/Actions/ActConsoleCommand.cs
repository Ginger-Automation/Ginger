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

using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using GingerCore.Helpers;
using GingerCore.Properties;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Enums;

namespace GingerCore.Actions
{
    public class ActConsoleCommand : Act
    {
        public override string ActionDescription { get { return "Console Command Action"; } }
        public override string ActionUserDescription { get { return "Run commands on Dos/Unix system"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you need to run command/s on a Dos/Unix system." + Environment.NewLine
                + Environment.NewLine + "1. This action contains list of options which will allow you to run simple or " +
                "complicated commands on the relevant system." + Environment.NewLine + "2. If you want to execute " +
                "a Job with Double Enter, use command -> {echo \\n |R <Job>}. ");
        }

        public override string ActionEditPage { get { return "ActConsoleCommandEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return true; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.DOS);
                    mPlatforms.Add(ePlatformType.Unix);
                }
                return mPlatforms;
            }
        }

        public enum eConsoleCommand
        {
            FreeCommand = 1,
            CopyFile = 2,
            IsFileExist = 6,
            Script = 18,
            ParametrizedCommand = 19
        }

        [IsSerializedForLocalRepository]
        public eConsoleCommand ConsoleCommand { get; set; }

        [IsSerializedForLocalRepository]
        public string ScriptName { get; set; }

        [IsSerializedForLocalRepository]
        public string Command { get; set; }

        [IsSerializedForLocalRepository]
        public int? WaitTime { get; set; }

        [IsSerializedForLocalRepository]
        public string ExpString { get; set; }

        [IsSerializedForLocalRepository]
        public string Delimiter { get; set; }


        public override String ActionType
        {
            get
            {
                return "Console Command - " + ConsoleCommand.ToString();
            }
        }

        public new static partial class Fields
        {
            public static string ConsoleCommand = "ConsoleCommand";

            public static string ScriptName = "ScriptName";

            public static string WaitTime = "WaitTime";
            public static string ExpString = "ExpString";

            public static string Command = "Command";

        }

        //TODO: find icon for console
        public override eImageType Image { get { return eImageType.CodeFile; } }

    }
}