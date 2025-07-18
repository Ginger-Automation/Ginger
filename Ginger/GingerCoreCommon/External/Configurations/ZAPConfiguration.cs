using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.External.Configurations
{
    public class ZAPConfiguration : RepositoryItemBase
    {
        private string mZAPApiKey;
        public string ZAPApiKey
        {
            get { return mZAPApiKey; }
            set
            {
                if (mZAPApiKey != value)
                {
                    mZAPApiKey = value;
                    OnPropertyChanged(nameof(ZAPApiKey));
                }
            }
        }

        private string mName;
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

        private string mZAPUrl;
        [IsSerializedForLocalRepository]
        public string ZAPUrl
        {
            get
            {
                return mZAPUrl;
            }
            set
            {
                if (value != null && value.EndsWith("/"))
                {
                    value = value.TrimEnd('/');
                }

                if (mZAPUrl != value)
                {
                    mZAPUrl = value;
                    OnPropertyChanged(nameof(ZAPUrl));
                }
            }
        }
        public override string ItemName { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    }
}
