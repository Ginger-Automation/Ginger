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
using Amdocs.Ginger.Common;
using GingerCore;
using GingerCore.Actions;
using System.Collections.Generic;

namespace Ginger.Imports.CDL
{
    public class ImportCDL
    {
        public string CDLText { get; set; }

        //TODO: return list of errors
        public void CheckSyntax()
        {
            
        }

        public void Run()
        {
            ObservableList<BusinessFlow> BFs = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();

            List<Activity> activities = new List<Activity>();

            foreach(BusinessFlow BF in BFs)
            {
                foreach(Activity a in BF.Activities)
                {
                    foreach(Act act in a.Acts)
                    {
                        if(act.LocateValue == "ID123")
                        {
                            act.LocateValue = "ID123New";
                            activities.Add(a);
                        }
                    }
                }
            }

            foreach(Activity a in activities)
            {
                ActGenElement a1 = new ActGenElement();
                a1.Active = true;
                a1.Description = "Validate new label exist";
                a1.LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByID;
                a1.LocateValue = "ID123New";                                
                a.Acts.Add(a1);
            }
        }
    }
}