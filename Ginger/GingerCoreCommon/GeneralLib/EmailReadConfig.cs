using System;

namespace GingerCore.GeneralLib
{
    public struct EmailReadConfig : IEquatable<EmailReadConfig>
    {
        public string UserEmail { get; set; }
        public string UserPassword { get; set; }
        public string ClientId { get; set; }
        public string TenantId { get; set; }

        public bool Equals(EmailReadConfig other)
        {
            return base.Equals(other);
        }
    }
}
