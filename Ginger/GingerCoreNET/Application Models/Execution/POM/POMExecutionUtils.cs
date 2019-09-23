using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Application_Models.Execution.POM
{
    public class POMExecutionUtils
    {
        ActUIElement mAct = null;
        public POMExecutionUtils(Act act)
        {
            mAct = (ActUIElement) act;
        }

        private string[] PomElementGUID => mAct.ElementLocateValue.ToString().Split('_');

        public ApplicationPOMModel CurrentPOM { get { return GetCurrentPOM(); } }

        public ElementInfo CurrentPOMElementInfo { get { return GetCurrentPOMElementDetails(); } }

        private ApplicationPOMModel GetCurrentPOM()
        {
            Guid selectedPOMGUID = new Guid(PomElementGUID[0]);
            ApplicationPOMModel currentPOM = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<ApplicationPOMModel>(selectedPOMGUID);

            if (currentPOM == null)
            {
                mAct.ExInfo = string.Format("Failed to find the mapped element Page Objects Model with GUID '{0}'", selectedPOMGUID.ToString());
                return null;
            }
            return currentPOM;

        }

        private ElementInfo GetCurrentPOMElementDetails()
        {
            Guid currentPOMElementInfoGUID = new Guid(PomElementGUID[1]);
            ElementInfo selectedPOMElementInfo = GetCurrentPOM().MappedUIElements.Where(z => z.Guid == currentPOMElementInfoGUID).FirstOrDefault();

            if (selectedPOMElementInfo == null)
            {
                mAct.ExInfo = string.Format("Failed to find the mapped element with GUID '{0}' inside the Page Objects Model", selectedPOMElementInfo.ToString());
                return null;
            }

            return selectedPOMElementInfo;
        }
    }
}
