using amdocs.ginger.GingerCoreNET;
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
    public sealed class WorkSpaceToSolutionFacadeAdapter : ISolutionFacadeForBPMN
    {
        private readonly WorkSpace _workspace;

        public WorkSpaceToSolutionFacadeAdapter(WorkSpace workspace)
        {
            _workspace = workspace;
        }

        public ObservableList<Activity> GetActivitiesFromSharedRepository()
        {
            return _workspace.SolutionRepository.GetAllRepositoryItems<Activity>();
        }

        public ObservableList<ApplicationPlatform> GetApplicationPlatforms()
        {
            return _workspace.Solution.ApplicationPlatforms;
        }

        public ObservableList<TargetBase> GetTargetApplications()
        {
            return _workspace.Solution.GetSolutionTargetApplications();
        }
    }
}
