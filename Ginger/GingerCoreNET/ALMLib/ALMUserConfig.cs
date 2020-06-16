using Amdocs.Ginger.Repository;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace GingerCoreNET.ALMLib
{
    public class ALMUserConfig : RepositoryItemBase
    {
        private string mALMUserName;
        [IsSerializedForLocalRepository]
        public string ALMUserName
        {
            get { return mALMUserName; }
            set
            {
                if (mALMUserName != value)
                {
                    mALMUserName = value;
                    OnPropertyChanged(nameof(ALMUserName));
                }
            }
        }

        [IsSerializedForLocalRepository]
        public string EncryptedALMPassword { get; set; }
        public string ALMPassword
        {
            get
            {
                bool res = false;
                string pass = EncryptionHandler.DecryptString(EncryptedALMPassword, ref res);
                if (res && !String.IsNullOrEmpty(pass))
                {
                    return pass;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                bool res = false;
                EncryptedALMPassword = EncryptionHandler.EncryptString(value, ref res);
            }
        }
        private ALMIntegration.eALMType mAlmType = ALMIntegration.eALMType.QC;
        [IsSerializedForLocalRepository]
        public ALMIntegration.eALMType AlmType
        {
            get
            {
                return mAlmType;
            }
            set
            {
                mAlmType = value;
            }
        }
        public override string ItemName { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    }
}
