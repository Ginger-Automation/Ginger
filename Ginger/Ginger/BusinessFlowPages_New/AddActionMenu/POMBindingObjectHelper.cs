using Amdocs.Ginger.Repository;

namespace Ginger.BusinessFlowPages
{
    public class POMBindingObjectHelper
    {
        public bool IsChecked { get; set; }
        public string ItemName { get; set; }
        public string ContainingFolder { get; set; }
        public ApplicationPOMModel ItemObject { get; set; }
    }
}
