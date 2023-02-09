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
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Amdocs.Ginger.Common.GlobalSolutionLib
{
    public class GlobalSolution
    {
        public enum eImportFromType
        {
            LocalFolder,
            SourceControl,
            Package//Need to discuss
        }
        public enum eImportItemType
        {
            [EnumValueDescription("~\\BusinessFlows\\")]
            BusinessFlows,
            [EnumValueDescription("~\\Shared Repository\\Activities Group\\")]
            SharedRepositoryActivitiesGroup,
            [EnumValueDescription("~\\Shared Repository\\Activities\\")]
            SharedRepositoryActivities,
            [EnumValueDescription("~\\Shared Repository\\Actions\\")]
            SharedRepositoryActions,
            [EnumValueDescription("~\\Shared Repository\\Variables\\")]
            SharedRepositoryVariables,
            [EnumValueDescription("~\\Applications Models\\API Models\\")]
            APIModels,
            [EnumValueDescription("~\\Applications Models\\POM Models\\")]
            POMModels,
            [EnumValueDescription("~\\Documents\\")]
            Documents,
            [EnumValueDescription("~\\Environments\\")]
            Environments,
            [EnumValueDescription("~\\DataSources\\")]
            DataSources,
            [EnumValueDescription("~\\Agents\\")]
            Agents,
            [EnumValueDescription("~\\Ginger.Solution.xml")]
            TargetApplication,
            [EnumValueDescription("~\\Ginger.Solution.xml")]
            Variables,
            [EnumValueDescription("~\\Ginger.Solution.xml")]
            ExtrnalIntegrationConfigurations
        }
        
        public static IEnumerable<T> GetEnumValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public enum eImportSetting
        {
            New,
            Replace
        }

    }
}
