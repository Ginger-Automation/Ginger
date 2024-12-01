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

using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.CoreNET.BPMN.Conversion;
using Amdocs.Ginger.CoreNET.BPMN.Exceptions;
using GingerCore;
using System.Collections.Generic;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.BPMN.Utils
{
    internal static class SolutionBPMNUtil
    {
        /// <summary>
        /// Get <see cref="TargetBase"/> whose name matches the given <paramref name="targetAppName"/>.
        /// </summary>
        /// <param name="targetAppName">Name of the <see cref="TargetBase"/> to search for.</param>
        /// <returns><see cref="TargetBase"/> with name matching the given <paramref name="targetAppName"/>.</returns>
        /// <exception cref="BPMNConversionException">If no <see cref="TargetBase"/> is found with name matching the given <paramref name="targetAppName"/>.</exception>
        internal static TargetBase GetTargetApplicationByName(string targetAppName, ISolutionFacadeForBPMN solutionFacade)
        {
            IEnumerable<TargetBase> targetApplications = solutionFacade.GetTargetApplications();
            TargetBase targetApp = targetApplications.FirstOrDefault(targetApp => string.Equals(targetApp.Name, targetAppName));

            if (targetApp == null)
            {
                throw new BPMNConversionException($"No {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} found with name '{targetAppName}'");
            }

            return targetApp;
        }
    }
}
