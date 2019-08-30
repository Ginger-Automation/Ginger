using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.ApplicationModelsLib
{
    /// <summary>
    /// This class is used to handle the POM binding on the grid
    /// </summary>
    public class POMBindingObjectHelper
    {
        public string ContainingFolder { get; set; }

        public string ItemName { get; set; }

        public bool IsChecked { get; set; }

        public ApplicationPOMModel ItemObject { get; set; }
    }
}
