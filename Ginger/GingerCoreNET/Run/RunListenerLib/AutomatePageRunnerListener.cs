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

using System;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Run
{


    public class AutomatePageRunnerListener : RunListenerBase
    {

        public EventHandler AutomatePageRunnerListenerGiveUserFeedback;
        public override void GiveUserFeedback(uint eventTime)
        {
            // TODO: Give user feedback by call to Do events
            // after 500ms - make sure show last message if no new one
            // Get bulk of message update by timer

            // since this is only for doevents we can run on another task and release faster -speed
            Task.Factory.StartNew(() => {
                AutomatePageRunnerListenerGiveUserFeedback.Invoke(this, null);
            });            
        }
    }
}
