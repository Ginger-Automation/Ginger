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

using System;
using System.Collections.Generic;
using Amdocs.Ginger.Common;
using Ginger.UserControlsLib.ActionInputValueUserControlLib;
using Newtonsoft.Json;

namespace Amdocs.Ginger.Repository
{
    public class ActInputValue : RepositoryItemBase
    {
        //TODO: Add Param type and valid value, if it is number , date etc... so we can have controls created for it 

        public static partial class Fields
        {
            public static string Param = "Param";
            public static string Value = "Value";
            public static string ValueForDriver = "ValueForDriver";
            public static string StoreToVariable = "StoreToVariable";
            public static string StoreToDataSource = "StoreToDataSource";
        }


        public static explicit operator ActInputValue(string Value)
        {
            ActInputValue AIV = new ActInputValue();
            AIV.Value = Value;
            return AIV;
        }

        
        [IsSerializedForLocalRepository]
        public string Param { get; set; }

        public Type ParamType { get; set; }        

        private string mValue;

        [IsSerializedForLocalRepository]        
        public string Value { get { return mValue; }                  
            set
            {
                if (mValue != value)
                {
                    mValue = value;
                    OnPropertyChanged(Fields.Value);
                }
            }
        }


        public bool BoolValue
        {
            get
            {
                bool b = false;  // in case of err we return false
                bool.TryParse(mValue, out b);
                return b;
            }

            set
            {
                mValue = value.ToString();
                OnPropertyChanged(Fields.Value);
            }
        }

        public int IntValue
        {
            get
            {
                int i = 0;
                int.TryParse(mValue, out i);
                return i;
            }

            set
            {
                mValue = value.ToString();
                OnPropertyChanged(Fields.Value);
            }
        }

        
        DynamicListWrapper mDynamicListWrapper = null;

        public List<string> GetListItemProperties()
        {
            return mDynamicListWrapper.GetListItemProperties();
        }
        
        public ObservableList<dynamic> ListDynamicValue
        {
            get
            {
                if (mDynamicListWrapper == null)
                {
                    if (string.IsNullOrEmpty(mValue))
                    {
                        // Create empty list with one dummy item
                        mDynamicListWrapper = new DynamicListWrapper(ParamTypeEX, true);
                    }
                    else
                    {
                        dynamic dynList = JsonConvert.DeserializeObject(mValue);
                        mDynamicListWrapper = new DynamicListWrapper(ParamTypeEX);                        
                        foreach (dynamic item in dynList)
                        {
                            mDynamicListWrapper.Items.Add(item);
                        }
                    }
                }

                return mDynamicListWrapper.Items;
            }

            set
            {
                // Keep the list as string
                mValue = JsonConvert.SerializeObject(value, Formatting.None);
                // Keep the actual list objects
                mDynamicListWrapper.Items = value;
                OnPropertyChanged(nameof(Value));
            }
        }

      


        private string mValueForDriver;

        public string ValueForDriver { get { return mValueForDriver; } set { mValueForDriver = value; OnPropertyChanged(Fields.ValueForDriver); } }

        //TODO: fix me soemthing wrong here !!!!!!!!!!!!!!!!!!!!!!!!!!!!!! - not needed - commented out - delete me later
        // when delete - reading exisitng flow will not load the Value!!!!!!!!!!!!!!!! be careful
        //private string mStoreToVariable;
        [IsSerializedForLocalRepository]
        public virtual string StoreToVariable { get { return mValue; } set { mValue = value; OnPropertyChanged(Fields.ValueForDriver); } }

        public override string ItemName
        {
            get
            {
                return this.Param;
            }
            set
            {
                this.Param = value;
            }
        }

        // For List<T> keep the type of list item
        [IsSerializedForLocalRepository]
        public string ParamTypeEX { get; set; }

        public override string GetNameForFileName()
        {
            return Param;
        }

    }
}
