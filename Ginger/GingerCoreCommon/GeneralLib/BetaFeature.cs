#region License
/*
Copyright © 2014-2025 European Support Limited

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

using System.ComponentModel;
using Newtonsoft.Json;

namespace Amdocs.Ginger.Common.GeneralLib
{

    [JsonObject(MemberSerialization.OptIn)]
    public class BetaFeature : INotifyPropertyChanged
    {
        bool mSelected = false;

        [JsonProperty]
        public bool Selected { get { return mSelected; } set { if (mSelected != value) { mSelected = value; OnPropertyChanged(ID); } } }

        public string Group { get; set; }

        [JsonProperty]
        public string ID { get; set; }


        public string Description { get; set; }


        public string Warning { get; set; }

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
