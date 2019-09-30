using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.AnalyzerLib;
using Ginger.SolutionGeneral;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    class DoOptionsHanlder
    {
        internal static void Run(DoOptions opts)
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
            }
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

            Reporter.ToConsole(eLogLevel.INFO, stringBuilder.ToString());
        }

        private static void DoAnalyze(string solution)
        {
            WorkSpace.Instance.OpenSolution(solution);

            AnalyzerUtils analyzerUtils = new AnalyzerUtils();
            ObservableList<AnalyzerItemBase> issues = new ObservableList<AnalyzerItemBase>();
            analyzerUtils.RunSolutionAnalyzer(WorkSpace.Instance.Solution, issues);

            if (issues.Count == 0)
            {
                Reporter.ToConsole(eLogLevel.INFO, "No Issues found");
            }
            else
            {
                Reporter.ToConsole(eLogLevel.WARN, "Issues found, Total count: " + issues.Count);
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
                        Reporter.ToConsole(eLogLevel.ERROR, stringBuilder.ToString());
                        break;
                    case AnalyzerItemBase.eType.Info:
                        Reporter.ToConsole(eLogLevel.INFO, stringBuilder.ToString());
                        break;
                    case AnalyzerItemBase.eType.Warning:
                        Reporter.ToConsole(eLogLevel.WARN, stringBuilder.ToString());
                        break;
                }

            }
        }
    }
}
