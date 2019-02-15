#region License
/*
Copyright © 2014-2018 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.SolutionAutoSaveAndRecover
{
    public enum eRecoveredItemStatus
    {
        PendingRecover=1,
        Recovered=2,
        RecoveredFailed=3,
        Deleted=4,
        DeleteFailed=5
    }
    public class RecoveredItem : INotifyPropertyChanged
    {
        public static string RecoverFolderPath = string.Empty;
        

        public RepositoryItemBase RecoveredItemObject { get; set; }

        
        public eRecoveredItemStatus mStatus { get; set; }
        public eRecoveredItemStatus Status
        {
            get { return mStatus; }
            set
            {
                if (mStatus != value)
                {
                    mStatus = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        private void OnPropertyChanged(object status)
        {
            throw new NotImplementedException();
        }

        string mRecoverPath = null;
        public string RecoverPath 
        {
            get
            {
                if (mRecoverPath == null && RecoveredItemObject != null)
                {
                    mRecoverPath = RecoveredItemObject.ContainingFolder;
                }
                return mRecoverPath;
            }
            set
            {
                mRecoverPath = value;
            }
        }
       
        public string RecoverItemName
        {
            get
            {
                if (RecoveredItemObject!=null)
                {
                    return RecoveredItemObject.ItemName;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public bool Selected { get; set; }
       
                
        public string  RecoveredItemType
        {
            get
            {
                if (RecoveredItemObject != null)
                {
                    return RecoveredItemObject.ObjFileExt;
                }
                else
                {
                    return string.Empty;
                }

            }
        }

        public string RecoverDate { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
