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
using Amdocs.Ginger.Common;
using Ginger;
using Ginger.AnalyzerLib;
using Ginger.SourceControl;
using GingerCore;
using GingerCoreNET.SourceControl;
using System;
using System.IO;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{

    public static class DoOptionsHandler
    {
        public static void Run(DoOptions opts)
        {
            
            switch (opts.Operation)
            {
                case DoOptions.DoOperation.analyze:
                    DoAnalyze(opts.Solution);
                    break;
                case DoOptions.DoOperation.clean:
                    // TODO: remove execution folder, backups and more
                    break;
                case DoOptions.DoOperation.info:
                    DoInfo(opts.Solution);
                    break;
                case DoOptions.DoOperation.open:
                    DoOpen(opts.Solution, opts.EncryptionKey);
                    break;
                case DoOptions.DoOperation.openSourceControl:
                    DoOpenSourceControl(opts);
                    break;
            }
        }
        private static void DoOpenSourceControl(DoOptions runOptions)
        {

            if (WorkSpace.Instance.UserProfile == null)
            {
                WorkSpace.Instance.UserProfile = new UserProfile();
                UserProfileOperations userProfileOperations = new UserProfileOperations(WorkSpace.Instance.UserProfile);
                WorkSpace.Instance.UserProfile.UserProfileOperations = userProfileOperations;
            }
            WorkSpace.Instance.UserProfile.SourceControlURL = runOptions.URL;
            WorkSpace.Instance.UserProfile.SourceControlUser = runOptions.User;
            WorkSpace.Instance.UserProfile.SourceControlType = runOptions.SCMType;
            WorkSpace.Instance.UserProfile.UserProfileOperations.SourceControlIgnoreCertificate = runOptions.ignoreCertificate;
            WorkSpace.Instance.UserProfile.UserProfileOperations.SourceControlUseShellClient = runOptions.useScmShell;
            WorkSpace.Instance.UserProfile.EncryptedSourceControlPass = runOptions.Pass;
            WorkSpace.Instance.UserProfile.SourceControlPass = runOptions.Pass;

            if (runOptions.PasswordEncrypted)
            {
                PasswordEncrypted(runOptions);
            }


            Reporter.ToLog(eLogLevel.INFO, "Downloading/updating Solution from source control");
            string solutionFolder = runOptions.Solution;
            if (!SourceControlIntegration.DownloadSolution(solutionFolder))
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to Download/update Solution from source control");
            }
           

            if (!Directory.Exists(solutionFolder))
            {
                Reporter.ToLog(eLogLevel.ERROR, $"The provided folder path '{solutionFolder}' does not exist.");
                return;
            }
            // Check if the directory exists
            if (string.IsNullOrEmpty(runOptions.EncryptionKey))
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Encryption key is empty, please provide it and try again.");
                return;
            }
            WorkSpace.Instance.OpenSolution(solutionFolder, runOptions.EncryptionKey);
        }
        private static void PasswordEncrypted(DoOptions runOptions)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"PasswordEncrypted: '{runOptions.PasswordEncrypted.ToString()}'");
            string pswd = WorkSpace.Instance.UserProfile.SourceControlPass;
            if (runOptions.PasswordEncrypted.ToString() is "Y" or "true" or "True")
            {
                try
                {
                    pswd = EncryptionHandler.DecryptwithKey(WorkSpace.Instance.UserProfile.SourceControlPass, runOptions.EncryptionKey);
                }
                catch (Exception ex)
                {
                    string mess = ex.Message; //To avoid warning of ex not used
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to decrypt the source control password");//not showing ex details for not showing the password by mistake in log
                }
            }


            WorkSpace.Instance.UserProfile.SourceControlPass = pswd;
        }
        private static void DoInfo(string solution)
        {
            // TODO: print info on solution, how many BFs etc, try to read all items - for Linux deser test
            WorkSpace.Instance.OpenSolution(solution);
            StringBuilder stringBuilder = new StringBuilder(Environment.NewLine);
            stringBuilder.Append("Solution Name  :").Append(WorkSpace.Instance.Solution.Name).Append(Environment.NewLine);
            stringBuilder.Append("Business Flows :").Append(WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>().Count).Append(Environment.NewLine);
            stringBuilder.Append("Agents         :").Append(WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>().Count).Append(Environment.NewLine);

            // TODO: add more info

            Reporter.ToLog(eLogLevel.INFO, stringBuilder.ToString());
        }

        private static void DoOpen(string solutionFolder, string encryptionKey)
        {
            try
            {
                // Check if solutionFolder is null or empty
                if (string.IsNullOrWhiteSpace(solutionFolder))
                {
                    Reporter.ToLog(eLogLevel.ERROR, "The provided solution folder path is null or empty.");
                    return;
                }
                    // Check if the folder path contains the solution file name
                    if (solutionFolder.Contains("Ginger.Solution.xml"))
                {
                    solutionFolder = Path.GetDirectoryName(solutionFolder)?.Trim() ?? string.Empty;

                    if (string.IsNullOrEmpty(solutionFolder))
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Invalid solution folder path derived from the solution file.");
                        return;
                    }
                }

                // Check if the directory exists
                if (!Directory.Exists(solutionFolder))
                {
                    Reporter.ToLog(eLogLevel.ERROR, "The specified solution folder path does not exist. Please check the path and try again.");
                    return;
                }
                // Check if the directory exists
                if (string.IsNullOrEmpty(encryptionKey))
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Encryption key is empty, please provide it and try again.");
                    return;
                }
                // Attempt to open the solution
                WorkSpace.Instance.OpenSolution(solutionFolder, encryptionKey);
            }
            catch (Exception ex)
            {
                // Handle any other unexpected errors
                Reporter.ToLog(eLogLevel.ERROR, $"An unexpected error occurred while opening the solution in folder '{solutionFolder}'. Error: {ex.Message}");
            }
        }
        private static void DoAnalyze(string solution)
        {
            WorkSpace.Instance.OpenSolution(solution);

            AnalyzerUtils analyzerUtils = new AnalyzerUtils();
            ObservableList<AnalyzerItemBase> issues = [];
            analyzerUtils.RunSolutionAnalyzer(WorkSpace.Instance.Solution, issues);

            if (issues.Count == 0)
            {
                Reporter.ToLog(eLogLevel.INFO, "Analyzer- No Issues found");
            }
            else
            {
                Reporter.ToLog(eLogLevel.WARN, "Analyzer- Issues found, Total count: " + issues.Count);
            }

            foreach (AnalyzerItemBase issue in issues)
            {
                StringBuilder stringBuilder = new StringBuilder(Environment.NewLine);
                stringBuilder.Append("Description :").Append(issue.Description).Append(Environment.NewLine);
                stringBuilder.Append("Details     :").Append(issue.Details).Append(Environment.NewLine);
                stringBuilder.Append("Class       :").Append(issue.ItemClass).Append(Environment.NewLine);
                stringBuilder.Append("Name        :").Append(issue.ItemName).Append(Environment.NewLine);
                stringBuilder.Append("Impact      :").Append(issue.Impact).Append(Environment.NewLine);

                switch (issue.IssueType)
                {
                    case AnalyzerItemBase.eType.Error:
                        Reporter.ToLog(eLogLevel.ERROR, stringBuilder.ToString());
                        break;
                    case AnalyzerItemBase.eType.Info:
                        Reporter.ToLog(eLogLevel.INFO, stringBuilder.ToString());
                        break;
                    case AnalyzerItemBase.eType.Warning:
                        Reporter.ToLog(eLogLevel.WARN, stringBuilder.ToString());
                        break;
                }

            }
        }
    }
}
