using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository;
using GingerCore;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.BPMN
{
    public interface ISolutionFacade
    {
        /// <summary>
        /// Get all <see cref="Activity"/> from SharedRepository.
        /// </summary>
        /// <returns></returns>
        public ObservableList<Activity> GetActivitiesFromSharedRepository();

        /// <summary>
        /// Get all <see cref="ApplicationPlatform"/> from SharedRepository.
        /// </summary>
        /// <returns></returns>
        public ObservableList<ApplicationPlatform> GetApplicationPlatforms();

        /// <summary>
        /// Get all <see cref="TargetBase"/> from SharedRepository.
        /// </summary>
        /// <returns></returns>
        public ObservableList<TargetBase> GetTargetApplications();
    }
}
