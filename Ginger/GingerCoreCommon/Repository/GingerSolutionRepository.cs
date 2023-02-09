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

using Amdocs.Ginger.Common.Run;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.Variables;
using System.IO;

namespace Amdocs.Ginger.CoreNET.Repository
{
    public class GingerSolutionRepository
    {
        public static SolutionRepository CreateGingerSolutionRepository()
        {
            SolutionRepository SR = new SolutionRepository();


            // TODO: replace ~\ with ~ env.seperator !!!!!!!!!!
            SR.AddItemInfo<BusinessFlow>("*.Ginger.BusinessFlow.xml", SolutionRepository.cSolutionRootFolderSign + "BusinessFlows", true, GingerDicser.GetTermResValue(eTermResKey.BusinessFlows), PropertyNameForFileName: nameof(BusinessFlow.Name));

            SR.AddItemInfo<ApplicationAPIModel>("*.Ginger.ApplicationAPIModel.xml", SolutionRepository.cSolutionRootFolderSign + "Applications Models" + Path.DirectorySeparatorChar + "API Models", true, "API Models", PropertyNameForFileName: nameof(ApplicationAPIModel.Name));
            SR.AddItemInfo<GlobalAppModelParameter>("*.Ginger.GlobalAppModelParameter.xml", SolutionRepository.cSolutionRootFolderSign + "Applications Models" + Path.DirectorySeparatorChar + "Global Models Parameters", true, "Global Model Parameters", PropertyNameForFileName: nameof(GlobalAppModelParameter.PlaceHolder));
            SR.AddItemInfo<ApplicationPOMModel>("*.Ginger.ApplicationPOMModel.xml", SolutionRepository.cSolutionRootFolderSign + "Applications Models" + Path.DirectorySeparatorChar + "POM Models", true, "POM Models", PropertyNameForFileName: nameof(ApplicationPOMModel.Name));

            SR.AddItemInfo<ProjEnvironment>("*.Ginger.Environment.xml", SolutionRepository.cSolutionRootFolderSign + "Environments", true, "Environments", PropertyNameForFileName: nameof(ProjEnvironment.Name));
            SR.AddItemInfo<ALMDefectProfile>("*.Ginger.ALMDefectProfile.xml", SolutionRepository.cSolutionRootFolderSign + "ALMDefectProfiles", true, "ALM Defect Profiles", PropertyNameForFileName: nameof(ALMDefectProfile.Name));

            SR.AddItemInfo<Agent>("*.Ginger.Agent.xml", SolutionRepository.cSolutionRootFolderSign + "Agents", true, "Agents", PropertyNameForFileName: nameof(Agent.Name));
            
            SR.AddItemInfo<HTMLReportConfiguration>("*.Ginger.HTMLReportConfiguration.xml", SolutionRepository.cSolutionRootFolderSign + "HTMLReportConfigurations", true, "HTMLReportConfigurations", PropertyNameForFileName: nameof(HTMLReportsConfiguration.Name));
            //SR.AddItemInfo<HTMLReportTemplate>("*.Ginger.HTMLReportTemplate.xml", @"~\HTMLReportConfigurations\HTMLReportTemplate", true, "HTMLReportTemplate", PropertyNameForFileName: nameof(HTMLReportTemplate.Name));
            //SR.AddItemInfo<ReportTemplate>("*.Ginger.ReportTemplate.xml", @"~\HTMLReportConfigurations\ReportTemplates", true, "ReportTemplates", PropertyNameForFileName: nameof(ReportTemplate.Name));

            SR.AddItemInfo<DataSourceBase>("*.Ginger.DataSource.xml", SolutionRepository.cSolutionRootFolderSign + "DataSources", true, "Data Sources", PropertyNameForFileName: nameof(DataSourceBase.Name));

            SR.AddItemInfo<PluginPackage>("*.Ginger.PluginPackage.xml", SolutionRepository.cSolutionRootFolderSign + "Plugins", true, "Plugins", PropertyNameForFileName: nameof(PluginPackage.PluginId));

            SR.AddItemInfo<ActivitiesGroup>("*.Ginger.ActivitiesGroup.xml", SolutionRepository.cSolutionRootFolderSign + "SharedRepository" + Path.DirectorySeparatorChar + "ActivitiesGroup", true, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups, "Shared"), PropertyNameForFileName: nameof(ActivitiesGroup.Name));
            SR.AddItemInfo<Activity>("*.Ginger.Activity.xml", SolutionRepository.cSolutionRootFolderSign + "SharedRepository" + Path.DirectorySeparatorChar + "Activities", true, GingerDicser.GetTermResValue(eTermResKey.Activities, "Shared"), PropertyNameForFileName: nameof(Activity.ActivityName));
            SR.AddItemInfo<Act>("*.Ginger.Action.xml", SolutionRepository.cSolutionRootFolderSign + "SharedRepository" + Path.DirectorySeparatorChar + "Actions", true, "Shared Actions", PropertyNameForFileName: nameof(Act.Description));
            SR.AddItemInfo<VariableBase>("*.Ginger.Variable.xml", SolutionRepository.cSolutionRootFolderSign + "SharedRepository" + Path.DirectorySeparatorChar + "Variables", true, GingerDicser.GetTermResValue(eTermResKey.Variables, "Shared"), PropertyNameForFileName: nameof(VariableBase.Name));

            SR.AddItemInfo<RunSetConfig>("*.Ginger.RunSetConfig.xml", SolutionRepository.cSolutionRootFolderSign + "RunSetConfigs", true, GingerDicser.GetTermResValue(eTermResKey.RunSets), PropertyNameForFileName: nameof(RunSetConfig.Name));

            SR.AddItemInfo<RemoteServiceGrid>("*.Ginger.RemoteServiceGrid.xml", SolutionRepository.cSolutionRootFolderSign + "RemoteServiceGrid", true, "RemoteServiceGrid", PropertyNameForFileName: nameof(RemoteServiceGrid.Name));

            return SR;
        }
    }
}
