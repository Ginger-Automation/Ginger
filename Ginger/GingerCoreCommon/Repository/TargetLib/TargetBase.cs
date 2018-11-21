using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.Repository
{
    public class TargetBase : RepositoryItemBase
    {
        public bool Selected { get; set; }

        public override string ItemName
        {
            get
            {
                return string.Empty;
            }
            set
            {  
                // Do nothing
            }
        }

        // Save the last agent who executed on this App, for reports
        [IsSerializedForLocalRepository]
        public string LastExecutingAgentName { get; set; }

        public virtual string AppName { get; set; }
    }
}
