#region License
/*
Copyright © 2014-2024 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Ginger.AnalyzerLib;
using GingerCore;
using GingerCore.Environments;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace Amdocs.Ginger.CoreNET.AnalyzerLib
{
    public class AnalyzeEnvApplication : AnalyzerItemBase
    {
        public static void AnalyzeEnvAppInBusinessFlows(BusinessFlow businessFlow, List<AnalyzerItemBase> issues)
        {
            businessFlow
                .Activities
                .Select((activity) => activity.TargetApplication)
                .Distinct()
                .ForEach((targetApplication) =>
                {
                    CheckIfTargetApplicationExistInEnvironment(targetApplication, businessFlow, ref issues);
                });
        }
        private static void CheckIfTargetApplicationExistInEnvironment(string TargetApplication, BusinessFlow businessFlow, ref List<AnalyzerItemBase> issues)
        {
            if (!string.IsNullOrEmpty(TargetApplication))
            {
                ProjEnvironment currentEnvironment = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().FirstOrDefault((projEnvironment) => projEnvironment.Name.Equals(businessFlow.Environment));

                if (currentEnvironment == null)
                {
                    return;
                }


                EnvApplication application = currentEnvironment?.Applications?.FirstOrDefault((appName) => appName.Name.Equals(TargetApplication));

                // Commented it as part of Backward support compatibility
                //if (application == null || string.IsNullOrEmpty(application.Name))
                //{
                //    string EnvApps = string.Join(", ", currentEnvironment.Applications.Select(p => p.Name)?.ToList());

                //    AnalyzeEnvApplication AB = new()
                //    {
                //        Description = $"{GingerDicser.GetTermResValue(eTermResKey.Activity)} {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} not found in Environment: {currentEnvironment.Name}",
                //        UTDescription = "MissingApplicationInEnvironment",
                //        Details = $"{GingerDicser.GetTermResValue(eTermResKey.Activity)} {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} = '{TargetApplication}' is missing in Environment '{currentEnvironment.Name}'. Curent Environment app(s) are: '{EnvApps}'",
                //        HowToFix = $"Open the Environment Configurations Page and add/set correct {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)}",
                //        CanAutoFix = eCanFix.No,
                //        Status = eStatus.NeedFix,
                //        IssueType = eType.Info,
                //        ItemParent = currentEnvironment.Name,
                //        ItemName = TargetApplication,
                //        Impact = "Execution probably will fail due to missing input value.",
                //        ItemClass = GingerDicser.GetTermResValue(eTermResKey.TargetApplication),
                //        Severity = eSeverity.High,
                //        Selected = false,
                //        FixItHandler = null
                //    };
                //    issues.Add(AB);
                //    return;
                //}

                if (application != null && !string.IsNullOrEmpty(application.Name))
                {
                    ApplicationPlatform? ExistingApplication = WorkSpace.Instance.Solution.ApplicationPlatforms
                    .FirstOrDefault(applicationPlatform => applicationPlatform.Guid.Equals(application.ParentGuid) || applicationPlatform.AppName.Equals(application.Name));

                    if (ExistingApplication == null)
                    {
                        AnalyzeEnvApplication AB = new()
                        {
                            Description = $"Environment Application does not exist in the {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} of the Solution",
                            UTDescription = "MissingApplicationInTargetApplication",
                            Details = $"Environment Application does not exist in the {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} of the Solution",
                            FixItHandler = null,
                            Status = eStatus.NeedFix,
                            IssueType = eType.Error,
                            ItemParent = currentEnvironment?.Name,
                            ItemName = TargetApplication,
                            Impact = "Execution probably will fail due to missing input value.",
                            ItemClass = GingerDicser.GetTermResValue(eTermResKey.TargetApplication),
                            Severity = eSeverity.High,
                            Selected = false,
                            CanAutoFix = eCanFix.No,
                            HowToFix = $"""
                        Add this application in the {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)}.
                        
                        Note :- That if you want to add the application with the same name as in environment then please delete the existing application from the environment 
                        and add a new one.
                        """
                        };
                        issues.Add(AB);
                    }
                }
            }

        }

        private static void FixMissingEnvApp(object? sender, EventArgs e)
        {
            if (sender is not AnalyzeEnvApplication AnalyzeTargetApplication)
            {
                return;
            }


            string ApplicationName = AnalyzeTargetApplication.ItemName;
            string EnvironmentName = AnalyzeTargetApplication.ItemParent;

            ProjEnvironment SelectedEnvironment = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().FirstOrDefault((proj) => proj.Name.Equals(EnvironmentName));
            ApplicationPlatform ApplicationPlatform = WorkSpace.Instance.Solution.ApplicationPlatforms.FirstOrDefault((appPlat) => appPlat.AppName.Equals(ApplicationName));

            if (SelectedEnvironment == null || ApplicationPlatform == null)
            {
                return;
            }

            SelectedEnvironment.StartDirtyTracking();
            SelectedEnvironment.AddApplications([ApplicationPlatform]);

            AnalyzeTargetApplication.Status = eStatus.Fixed;

        }

        private static readonly object lockObject = new();

        public static bool DoesEnvParamOrURLExistInValueExp(string ValueExp, string CurrentEnvironment)
        {

            if (string.IsNullOrEmpty(ValueExp))
            {
                return true;
            }



            MatchCollection envParamMatches = GingerCore.ValueExpression.rxEnvParamPattern.Matches(ValueExp);

            if (envParamMatches == null || envParamMatches.Count == 0)
            {
                return true;
            }

            ProjEnvironment CurrentProjEnv = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().FirstOrDefault((proj) => proj.Name.Equals(CurrentEnvironment));


            if (CurrentProjEnv == null)
            {
                return true;
            }


            foreach (Match envParamMatch in envParamMatches)
            {
                string AppName = string.Empty, GlobalParamName = string.Empty;

                GingerCore.ValueExpression.GetEnvAppAndParam(envParamMatch.Value, ref AppName, ref GlobalParamName);

                if (string.IsNullOrEmpty(AppName) || string.IsNullOrEmpty(GlobalParamName))
                {
                    return false;
                }

                EnvApplication CurrentEnvApp = CurrentProjEnv.Applications.FirstOrDefault((app) => app.Name.Equals(AppName));

                lock (lockObject)
                {
                    CurrentEnvApp?.ConvertGeneralParamsToVariable();
                }

                bool doesParamExistInCurrEnv = CurrentEnvApp?.Variables?.Any((Param) => Param.Name.Equals(GlobalParamName)) ?? false;

                if (!doesParamExistInCurrEnv)
                {
                    return false;
                }
            }

            MatchCollection envURLMatches = GingerCore.ValueExpression.rxEnvUrlPattern.Matches(ValueExp);

            if (envURLMatches == null || envURLMatches.Count == 0)
            {
                return true;
            }

            foreach (Match envURLMatch in envURLMatches)
            {
                string AppName = string.Empty;
                GingerCore.ValueExpression.GetEnvAppFromEnvURL(envURLMatch.Value, ref AppName);


                if (string.IsNullOrEmpty(AppName))
                {
                    return false;
                }

                return CurrentProjEnv.Applications.Any((envApp) => envApp.Name.Equals(AppName));
            }

            return false;
        }
    }
}
