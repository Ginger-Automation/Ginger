using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.Repository.TargetLib
{
    public class TargetPlugin : TargetBase
    {
        private string mPluginID;
        [IsSerializedForLocalRepository]
        public string PluginID
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
                    OnPropertyChanged(nameof(PluginID));
                }
            }
        }


        string mAppName;
        public override string AppName
        {
            get
            {
                return mAppName;
            }
            set
            {
                if (mAppName != value)
                {
                    mAppName = value;
                    OnPropertyChanged(nameof(AppName));
                }
            }
        }
    }    
}
