using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.Repository
{
    public class TargetBase : RepositoryItemBase
    {        
        public virtual string Name { get; }// impl in subclass
        public bool Selected { get; set; }

        public override string ItemName
        {
            get
            {
                return Name;
            }
            set
            {
                // Do nothing
            }
        }

        // Save the last agent who executed on this Target
        [IsSerializedForLocalRepository]
        public string LastExecutingAgentName { get; set; }
    }
}
