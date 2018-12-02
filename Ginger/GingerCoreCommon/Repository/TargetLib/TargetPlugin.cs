using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.Repository.TargetLib
{
    public class TargetPlugin : TargetBase
    {
        public override string Name { get { return mPluginID + "." + mServiceID; } }

        private string mPluginID;
        [IsSerializedForLocalRepository]
        public string PluginId
        {
            get
            {
                return mPluginID;
            }
            set
            {
                if (mPluginID != value)
                {
                    mPluginID = value;
                    OnPropertyChanged(nameof(PluginId));
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private string mServiceID;
        [IsSerializedForLocalRepository]
        public string ServiceId
        {
            get
            {
                return mServiceID;
            }
            set
            {
                if (mServiceID != value)
                {
                    mServiceID = value;
                    OnPropertyChanged(nameof(ServiceId));
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
    }    
}
