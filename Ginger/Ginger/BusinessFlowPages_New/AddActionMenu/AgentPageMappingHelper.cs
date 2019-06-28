using GingerCore;

namespace Ginger.BusinessFlowPages_New
{
    public class AgentPageMappingHelper
    {
        /// <summary>
        /// Gets/Sets ObjectWindowPage
        /// </summary>
        public object ObjectWindowPage { get; set; }
        
        /// <summary>
        /// Gets/Sets ObjectAgent
        /// </summary>
        public Agent ObjectAgent { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="agent"></param>
        public AgentPageMappingHelper(Agent agent, object winPage)
        {
            ObjectAgent = agent;
            ObjectWindowPage = winPage;
        }
    }
}
