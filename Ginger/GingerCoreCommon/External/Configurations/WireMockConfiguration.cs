using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.External.Configurations
{
    public class WireMockConfiguration : RepositoryItemBase
    {
        private string mName = "WireMockConfig";
        [IsSerializedForLocalRepository]
        public string Name
        {
            get { return mName; }
            set
            {
                if (mName != value)
                {
                    mName = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private string mWireMockUrl;
        [IsSerializedForLocalRepository]
        public string WireMockUrl
        {
            get
            {
                return mWireMockUrl;
            }
            set
            {
                if (mWireMockUrl != value)
                {
                    mWireMockUrl = NormalizeUrl(value);

                    OnPropertyChanged(nameof(WireMockUrl));
                }
            }
        }

        private static string NormalizeUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }

            // Remove trailing slash
            if (url.EndsWith("/"))
            {
                url = url.TrimEnd('/');
            }

            // Add __admin if not present
            if (!url.EndsWith("__admin"))
            {
                url += "/__admin";
            }

            return url;
        }
        public override string ItemName
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }
    }
}
