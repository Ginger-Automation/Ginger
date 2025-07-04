using System;

namespace Amdocs.Ginger.CoreNET.GenAIServices
{
    public class GingerPlayAPITokenResponseInfo
    {

        public string access_token { get; set; }
        public bool IsTokenValid => !string.IsNullOrEmpty(access_token) && DateTime.UtcNow.AddSeconds(expires_in) > DateTime.UtcNow;
        public double expires_in { get; set; }
        public string token_type { get; set; }

    }
}
