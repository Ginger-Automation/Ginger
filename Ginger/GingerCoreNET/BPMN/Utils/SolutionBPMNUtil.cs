using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.CoreNET.BPMN.Conversion;
using Amdocs.Ginger.CoreNET.BPMN.Exceptions;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
