using amdocs.ginger.GingerCoreNET;
using Ginger.Run;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger
{
    // TODO: move to GingerCoreNET once RIs moved to GingerCoreCommon
    public class RunSetOperations
    {
        public static RunSetConfig CreateNewRunset(string runSetName, string runSetFolderPath = null)
        {
            RunSetConfig runSetConfig = new RunSetConfig();
            runSetConfig.Name = runSetName;
            runSetConfig.GingerRunners.Add(new GingerRunner() { Name = "Runner 1" });
            WorkSpace.Instance.SolutionRepository.AddRepositoryItem(runSetConfig);
            return runSetConfig;
        }
    }
}
