namespace Amdocs.Ginger.CoreNET.External.GingerPlay
{
    /// <summary>
    /// Represents the response from GingerPlay API token endpoint
    /// </summary>
    public class GingerPlayAPITokenResponseInfo
    {

        public string access_token { get; set; }
        public double expires_in { get; set; }
        public string token_type { get; set; }

        public bool IsTokenValid => !string.IsNullOrEmpty(access_token) && expires_in > 0;
    }
}
