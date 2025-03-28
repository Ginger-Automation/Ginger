using Amdocs.Ginger.Repository;
using GingerCoreNET.SourceControl;
using System;
namespace Amdocs.Ginger.Common.SourceControlLib
{
    public class GingerSolution : RepositoryItemBase
    {
        private Guid mSolutionGuid;
        [IsSerializedForLocalRepository]
        public Guid SolutionGuid
        {
            get
            {
                return mSolutionGuid;
            }
            set
            {
                if (mSolutionGuid != value)
                {
                    mSolutionGuid = value;
                    OnPropertyChanged(nameof(SolutionGuid));
                }
            }
        }

        private SourceControlInfo mSourceControlInfo;
        [IsSerializedForLocalRepository]
        public SourceControlInfo SourceControlInfo
        {
            get
            {
                return mSourceControlInfo;
            }
            set
            {
                if (mSourceControlInfo != value)
                {
                    mSourceControlInfo = value;
                    OnPropertyChanged(nameof(SourceControlInfo));
                }
            }
        }

        public override string ItemName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    public class SourceControlInfo : RepositoryItemBase
    {
        private string mUrl;
        [IsSerializedForLocalRepository]
        public string Url
        {
            get
            {
                return mUrl;
            }
            set
            {
                if (mUrl != value)
                {
                    mUrl = value;
                    OnPropertyChanged(nameof(Url));
                }
            }
        }

        private string mUsername;
        [IsSerializedForLocalRepository]
        public string Username
        {
            get
            {
                return mUsername;
            }
            set
            {
                if (mUsername != value)
                {
                    mUsername = value;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

        private SourceControlBase.eSourceControlType mType;
        [IsSerializedForLocalRepository]
        public SourceControlBase.eSourceControlType Type
        {
            get
            {
                return mType;
            }
            set
            {
                if (mType != value)
                {
                    mType = value;
                    OnPropertyChanged(nameof(Type));
                }
            }
        }

        private string mAuthorName;
        [IsSerializedForLocalRepository]
        public string AuthorName
        {
            get
            {
                return mAuthorName;
            }
            set
            {
                if (mAuthorName != value)
                {
                    mAuthorName = value;
                    OnPropertyChanged(nameof(AuthorName));
                }
            }
        }

        private string mAuthorEmail;
        [IsSerializedForLocalRepository]
        public string AuthorEmail
        {
            get
            {
                return mAuthorEmail;
            }
            set
            {
                if (mAuthorEmail != value)
                {
                    mAuthorEmail = value;
                    OnPropertyChanged(nameof(AuthorEmail));
                }
            }
        }

        private string mBranch;
        [IsSerializedForLocalRepository]
        public string Branch
        {
            get
            {
                return mBranch;
            }
            set
            {
                if (mBranch != value)
                {
                    mBranch = value;
                    OnPropertyChanged(nameof(Branch));
                }
            }
        }

        private string mLocalFolderPath;
        [IsSerializedForLocalRepository]
        public string LocalFolderPath
        {
            get
            {
                return mLocalFolderPath;
            }
            set
            {
                if (mLocalFolderPath != value)
                {
                    mLocalFolderPath = value;
                    OnPropertyChanged(nameof(LocalFolderPath));
                }
            }
        }

        private Guid mGuid;
        [IsSerializedForLocalRepository]
        public new Guid Guid
        {
            get
            {
                return mGuid;
            }
            set
            {
                if (mGuid != value)
                {
                    mGuid = value;
                    OnPropertyChanged(nameof(Guid));
                }
            }
        }

        private bool mIsProxyConfigured;
        [IsSerializedForLocalRepository]
        public bool IsProxyConfigured
        {
            get
            {
                return mIsProxyConfigured;
            }
            set
            {
                if (mIsProxyConfigured != value)
                {
                    mIsProxyConfigured = value;
                    OnPropertyChanged(nameof(IsProxyConfigured));
                }
            }
        }

        private string mProxyAddress;
        [IsSerializedForLocalRepository]
        public string ProxyAddress
        {
            get
            {
                return mProxyAddress;
            }
            set
            {
                if (mProxyAddress != value)
                {
                    mProxyAddress = value;
                    OnPropertyChanged(nameof(ProxyAddress));
                }
            }
        }

        private string mProxyPort;
        [IsSerializedForLocalRepository]
        public string ProxyPort
        {
            get
            {
                return mProxyPort;
            }
            set
            {
                if (mProxyPort != value)
                {
                    mProxyPort = value;
                    OnPropertyChanged(nameof(ProxyPort));
                }
            }
        }

        private int mTimeout;
        [IsSerializedForLocalRepository(80)]
        public int Timeout
        {
            get
            {
                return mTimeout;
            }
            set
            {
                if (mTimeout != value)
                {
                    mTimeout = value;
                    OnPropertyChanged(nameof(Timeout));
                }
            }
        }

        private string mPassword;
        public string Password
        {
            get
            {
                return mPassword;
            }
            set
            {
                if (mPassword != value)
                {
                    mPassword = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        private string mEncryptedPassword;
        public string EncryptedPassword
        {
            get
            {
                return mEncryptedPassword;
            }
            set
            {
                if (mEncryptedPassword != value)
                {
                    mEncryptedPassword = value;
                    OnPropertyChanged(nameof(EncryptedPassword));
                }
            }
        }

        public override string ItemName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

}




