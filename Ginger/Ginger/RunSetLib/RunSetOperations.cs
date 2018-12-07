using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.GeneralLib;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger
{
    // TODO: move to GingerCoreNET once RIs moved to GingerCoreCommon
    public static class RunSetOperations
    {
        public static RunSetConfig CreateNewRunset(string runSetName="", RepositoryFolder<RunSetConfig> runSetsFolder = null)
        {
            if (string.IsNullOrEmpty(runSetName))
            {
                if (!InputBoxWindow.GetInputWithValidation(string.Format("Add New {0}", GingerDicser.GetTermResValue(eTermResKey.RunSet)), string.Format("{0} Name:", GingerDicser.GetTermResValue(eTermResKey.RunSet)), ref runSetName, System.IO.Path.GetInvalidPathChars()))
                {
                    return null;
                }

                while (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>().Where(r => r.ItemName.ToLower() == runSetName.ToLower()).FirstOrDefault() != null)
                {
                    Reporter.ToUser(eUserMsgKeys.DuplicateRunsetName, string.Format("'{0}' already exists, please use different name", runSetName));

                    if (!InputBoxWindow.GetInputWithValidation(string.Format("Add New {0}", GingerDicser.GetTermResValue(eTermResKey.RunSet)), string.Format("{0} Name:", GingerDicser.GetTermResValue(eTermResKey.RunSet)), ref runSetName, System.IO.Path.GetInvalidPathChars()))
                    {
                        return null;
                    }
                }
            }

            RunSetConfig runSetConfig = new RunSetConfig();
            runSetConfig.Name = runSetName;
            runSetConfig.GingerRunners.Add(new GingerRunner() { Name = "Runner 1" });

            if (runSetsFolder == null)
            {
                WorkSpace.Instance.SolutionRepository.AddRepositoryItem(runSetConfig);
            }
            else
            {
                runSetsFolder.AddRepositoryItem(runSetConfig);
            }

            return runSetConfig;
        }
    }
}
