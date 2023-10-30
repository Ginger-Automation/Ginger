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
        public ObservableList<Activity> GetActivitiesFromSharedRepository();

        public ObservableList<ApplicationPlatform> GetApplicationPlatforms();

        public ObservableList<TargetBase> GetTargetApplications();
    }
}
