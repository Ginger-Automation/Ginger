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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.GeneralLib;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Common;

namespace Amdocs.Ginger
{
    // TODO: move to GingerCoreNET once RIs moved to GingerCoreCommon
    public static class RunSetOperations
    {
        public static RunSetConfig CreateNewRunset(string runSetName="", RepositoryFolder<RunSetConfig> runSetsFolder = null)
        {
            if (string.IsNullOrEmpty(runSetName))
            {

                do
                {
                    if (!string.IsNullOrEmpty(runSetName.Trim()))
                    {
                        Reporter.ToUser(eUserMsgKey.DuplicateRunsetName, runSetName);
                    }

                    bool returnWindow = InputBoxWindow.OpenDialog(string.Format("Add New {0}", GingerDicser.GetTermResValue(eTermResKey.RunSet)),
                                                                        string.Format("{0} Name:", GingerDicser.GetTermResValue(eTermResKey.RunSet)),
                                                                                ref runSetName);

                    if (returnWindow)
                    {
                        if (string.IsNullOrEmpty(runSetName.Trim()))
                        {
                            Reporter.ToUser(eUserMsgKey.ValueIssue, "Value cannot be empty");
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                while (string.IsNullOrEmpty(runSetName.Trim()) || WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>().Where(r => r.ItemName.ToLower() == runSetName.ToLower()).FirstOrDefault() != null);
            }

            RunSetConfig runSetConfig = new RunSetConfig();
            runSetConfig.Name = runSetName;

            GingerRunner gingerRunner = new GingerRunner { Name = "Runner 1" };
            GingerExecutionEngine gingerExecutionEngine = new GingerExecutionEngine(gingerRunner);
            gingerRunner.Executor = gingerExecutionEngine;

            runSetConfig.GingerRunners.Add(gingerRunner);
            runSetConfig.AddCategories();

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
