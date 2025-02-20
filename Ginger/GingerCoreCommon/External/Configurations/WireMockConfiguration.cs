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
                    mWireMockUrl = value;

                    OnPropertyChanged(nameof(WireMockUrl));
                }
            }
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
