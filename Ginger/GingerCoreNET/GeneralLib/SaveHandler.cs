using Amdocs.Ginger.Common;
using amdocs.ginger.GingerCoreNET;
using Ginger.SolutionGeneral;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.CoreNET.GeneralLib
{
    public static class SaveHandler
    {
        public static void Save(RepositoryItemBase objectToSave)
        {
            if (objectToSave == null)
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectItem); return;
            }
            if (objectToSave is Solution)
            {
                Reporter.ToStatus(eStatusMsgKey.SaveItem, null, objectToSave.ItemName, "item");
                WorkSpace.Instance.Solution.SolutionOperations.SaveSolution();
                Reporter.HideStatusMessage();
            }
            else
            {
                Reporter.ToStatus(eStatusMsgKey.SaveItem, null, objectToSave.ItemName, "item");
                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(objectToSave);
                Reporter.HideStatusMessage();
            }
        }
    }
}
