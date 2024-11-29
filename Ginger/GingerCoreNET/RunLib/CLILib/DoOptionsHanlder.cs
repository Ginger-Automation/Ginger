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
using Ginger.AnalyzerLib;
using GingerCore;
using System;
using System.IO;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{

    public class DoOptionsHandler
    {
        DoOptions mOpts;
        CLIHelper mCLIHelper = new();
        public void Run(DoOptions opts)
        {
            mOpts = opts;
            switch (opts.Operation)
            {
                case DoOptions.DoOperation.analyze:
                    DoAnalyze();
                    break;
                case DoOptions.DoOperation.clean:
                    // TODO: remove execution folder, backups and more
                    break;
                case DoOptions.DoOperation.info:
                    DoInfo();
                    break;
                case DoOptions.DoOperation.open:
                    DoOpen();
                    break;
            }
        }


        private void DoInfo()
        {
            // TODO: print info on solution, how many BFs etc, try to read all items - for Linux deser test
            WorkSpace.Instance.OpenSolution(mOpts.Solution);
            StringBuilder stringBuilder = new StringBuilder(Environment.NewLine);
            stringBuilder.Append("Solution Name  :").Append(WorkSpace.Instance.Solution.Name).Append(Environment.NewLine);
            stringBuilder.Append("Business Flows :").Append(WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>().Count).Append(Environment.NewLine);
            stringBuilder.Append("Agents         :").Append(WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>().Count).Append(Environment.NewLine);

            // TODO: add more info

            Reporter.ToLog(eLogLevel.INFO, stringBuilder.ToString());
        }

        /// <summary>
        /// Opens the solution specified in the options.
        /// </summary>
        /// <param name="solutionFolder">The folder path of the solution to open.</param>
        /// <param name="encryptionKey">The encryption key for the solution, if any.</param>
        private void DoOpen()
        {
            string solutionFolder = mOpts.Solution;
            string encryptionKey = mOpts.EncryptionKey;
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

                // Attempt to open the solution
                mCLIHelper.AddCLIGitProperties(mOpts);
                mCLIHelper.SetWorkSpaceGitProperties(mOpts);
                mCLIHelper.SetEncryptionKey(encryptionKey);
                if (mOpts.PasswordEncrypted)
                {
                    mCLIHelper.PasswordEncrypted(mOpts.PasswordEncrypted.ToString());
                }
                mCLIHelper.Solution = mOpts.Solution;
                if (!mCLIHelper.LoadSolution())
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to Download/update Solution from source control");
                }
            }
            catch (Exception ex)
            {
                // Handle any other unexpected errors
                Reporter.ToLog(eLogLevel.ERROR, $"An unexpected error occurred while opening the solution in folder '{solutionFolder}'. Error: {ex.Message}");
            }
        }
        private void DoAnalyze()
        {
            WorkSpace.Instance.OpenSolution(mOpts.Solution);

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
