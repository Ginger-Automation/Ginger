#region License
/*
Copyright © 2014-2026 European Support Limited

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
        public static explicit operator ActInputValue(string Value)
        {
            ActInputValue AIV = new ActInputValue
            {
                Value = Value
            };
            return AIV;
        }

        private string mParam;

        [IsSerializedForLocalRepository]
        public string Param
        {
            get
            {
                return mParam;
            }
            set
            {
                if (mParam != value)
                {
                    mParam = value;
                    OnPropertyChanged(nameof(Param));
                }
            }
        }

        public Type ParamType { get; set; }

        private string mValue;

        [IsSerializedForLocalRepository]
        public string Value
        {
            get { return mValue; }
            set
            {
                if (mValue != value)
                {
                    mValue = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public string mDisplayValue;
        public string DisplayValue
        {
            get
            {
                if (mDisplayValue != null)
                {
                    return mDisplayValue;
                }
                return "";
            }
            set
            {
                mDisplayValue = value;
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
                if (mValue != value.ToString())
                {
                    mValue = value.ToString();
                    OnPropertyChanged(nameof(Value));
                }
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
                if (mValue != value.ToString())
                {
                    mValue = value.ToString();
                    OnPropertyChanged(nameof(Value));
                }
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
                        dynamic dynList;

                        try
                        {
                            dynList = JsonConvert.DeserializeObject(mValue);
                        }
                        catch (Exception exc)
                        {
                            Reporter.ToLog(eLogLevel.WARN, "An error occurred while Deserializing JSON object", exc);
                            dynList = JsonConvert.DeserializeObject(string.Format("{0}{1}{0}", "\"", mValue));
                        }

                        mDynamicListWrapper = new DynamicListWrapper(ParamTypeEX);
                        if (dynList != null)
                        {
                            foreach (dynamic item in dynList)
                            {
                                mDynamicListWrapper.Items.Add(item);
                            }
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

        public string ValueForDriver { get { return mValueForDriver; } set { if (mValueForDriver != value) { mValueForDriver = value; OnPropertyChanged(ValueForDriver); } } }


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

        public override string GetItemType()
        {
            return "ActInputValue";
        }

        public override string ItemNameField
        {
            get
            {
                return this.Param;
            }
        }

        /// <summary>
        /// Compares this instance with another ActInputValue instance for equality.
        /// </summary>
        /// <param name="other">The other ActInputValue instance to compare with.</param>
        /// <returns>True if both instances are equal; otherwise, false.</returns>
        public bool AreEqual(ActInputValue other)
        {
            if (other == null)
            {
                return false;
            }

            return this.Param == other.Param &&
                ((this.Value == null && string.IsNullOrEmpty(other.Value))
                || this.Value == other.Value);

        }

        /// <summary>
        /// Compares this instance with another object for equality.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>True if the object is an ActInputValue instance and both instances are equal; otherwise, false.</returns>
        public bool AreEqual(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals(obj as ActInputValue);
        }


    }
}
