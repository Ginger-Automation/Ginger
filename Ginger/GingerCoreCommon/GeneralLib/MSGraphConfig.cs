using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerCore.GeneralLib
{
    public struct MSGraphConfig : IEquatable<MSGraphConfig>
    {
        public string UserEmail { get; set; }
        public string UserPassword { get; set; }
        public string ClientId { get; set; }
        public string TenantId { get; set; }

        public bool Equals(MSGraphConfig other)
        {
            return base.Equals(other);
        }
    }
}
