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
