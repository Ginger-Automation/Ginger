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
            //[EnumValueDescription("Business Flows")]
            //BusinessFlows,
            [EnumValueDescription("Shared Repository - Activities Group")]
            SharedRepositoryActivitiesGroup,
            [EnumValueDescription("Shared Repository - Activities")]
            SharedRepositoryActivities,
            [EnumValueDescription("Shared Repository - Actions")]
            SharedRepositoryActions,
            [EnumValueDescription("Shared Repository - Variables")]
            SharedRepositoryVariables,
            [EnumValueDescription("API Models")]
            APIModels,
            [EnumValueDescription("Page Object Models")]
            POMModels,
            [EnumValueDescription("Documents")]
            Documents,
            [EnumValueDescription("Environments")]
            Environments,
            [EnumValueDescription("DataSources")]
            DataSources,
            [EnumValueDescription("Solution Variables")]
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
