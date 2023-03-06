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
