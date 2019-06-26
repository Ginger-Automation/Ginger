using Amdocs.Ginger.Common;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.BusinessFlowPages_New
{
    public class ObjectBindingHelper
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
        public ObjectBindingHelper(Agent agent, object winPage)
        {
            ObjectAgent = agent;
            ObjectWindowPage = winPage;
        }
    }
}
