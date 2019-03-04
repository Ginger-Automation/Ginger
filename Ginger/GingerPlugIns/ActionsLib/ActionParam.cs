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

using System.ComponentModel;

// OLD CLEANUP !!!!!!!!!!!!!!

namespace GingerPlugIns.ActionsLib
{
    public class ActionParam
    {
        public enum eParamType
        {
            eString,
            eBool,
            eList
        }

        public string Name { get; set; }

        public eParamType ParamType { get; set; }
        
        private object mValue;
        public object Value
        {
            get
            {
                return mValue;
            }
            set
            {
                mValue = value;
                OnPropertyChanged(nameof(mValue));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public string GetValueAsString()
        {
            if (Value != null)
            {
                return Value.ToString();
            }
            else
            {
                return null;
            }
        }

        public void SetValueFromString(string s)
        {
            
        }
    }
}
