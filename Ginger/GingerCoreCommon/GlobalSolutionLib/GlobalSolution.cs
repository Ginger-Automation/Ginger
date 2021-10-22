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
            Variables
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
