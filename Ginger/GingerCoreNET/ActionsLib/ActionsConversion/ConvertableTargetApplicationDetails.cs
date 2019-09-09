#region License
/*
Copyright © 2014-2019 European Support Limited

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

using System.Collections.Generic;
using System.Linq;
using System;
using GingerCore.Actions.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerCore.Actions;
using System.ComponentModel;

namespace Amdocs.Ginger.CoreNET
{
    public class ConvertableTargetApplicationDetails : INotifyPropertyChanged
    {
        // whether selected to be converted or not
        bool mSelected;
        public bool Selected
        {
            get { return mSelected; }
            set
            {
                if (mSelected != value)
                {
                    mSelected = value;
                    OnPropertyChanged(nameof(ConvertableActionDetails.Selected));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

        public string SourceTargetApplicationName { get; set; }

        public string TargetTargetApplicationName { get; set;}
    }
}
