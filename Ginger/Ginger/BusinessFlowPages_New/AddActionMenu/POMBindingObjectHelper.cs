using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.BusinessFlowPages_New
{
    public class POMBindingObjectHelper
    {
        public bool IsChecked { get; set; }
        public string ItemName { get; set; }
        public string ContainingFolder { get; set; }
        public ApplicationPOMModel ItemObject { get; set; }
    }
}
