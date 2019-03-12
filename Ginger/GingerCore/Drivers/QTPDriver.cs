#region License
/*
Copyright © 2014-2019 European Support Limited

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

using System.Collections.Generic;
using System.Threading.Tasks;
using GingerCore.Actions;
using System.Threading;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace GingerCore.Drivers
{
    public class QTPDriver : DriverBase
    {
        public QTPDriver(BusinessFlow BusinessFlow)
        {
            // TODO: Complete member initialization
            this.BusinessFlow = BusinessFlow;
        }

        public override void StartDriver()
        {
            StartQTP();
        }

        public void StartQTP()
        {
            //TODO: maybe better to run a .bat file

            //Start QTP on another Thread
            Task t = Task.Factory.StartNew(() =>
            {
            });

            while (!QTPConnector.QTPConnected)
            {
                Thread.Sleep(100);
            }
        }

        public override void CloseDriver()
        {
         
        }

        public override Act GetCurrentElement()
        {
            return null;
        }

        public override void RunAction(Act act)
        {
        }

        public override string GetURL()
        {
            return "TBD";

        }

        

        public override void HighlightActElement(Act act)
        {
        }
        
        public override ePlatformType Platform { get { return ePlatformType.NA; } }


        public override bool IsRunning()
        {
                return false;
        }
    }
}
