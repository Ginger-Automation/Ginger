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

using System;
using Amdocs.Ginger.Common;

namespace Amdocs.Ginger.Repository
{
    public class EnhancedActInputValue : ActInputValue
    {
        [IsSerializedForLocalRepository]
        public Guid ParamGuid { get; set; }

        [IsSerializedForLocalRepository]
        public string Description { get; set; }

        //[IsSerializedForLocalRepository]
        //public ObservableList<string> OptionalValues = new ObservableList<string>();

        ObservableList<string> mOptionalValues = [];
        public ObservableList<string> OptionalValues
        {
            get
            {
                return mOptionalValues;
            }
            set
            {
                mOptionalValues = value;
                OnPropertyChanged(nameof(OptionalValues));
            }
        }

        public string ExtraDetails { get; set; }

        /// <summary>
        /// Compares this instance with another EnhancedActInputValue instance for equality.
        /// </summary>
        /// <param name="other">The other EnhancedActInputValue instance to compare with.</param>
        /// <returns>True if both instances are equal; otherwise, false.</returns>
        public bool AreEqual(EnhancedActInputValue other)
        {
            if (other == null)
            {
                return false;
            }

            return this.Param == other.Param &&
                string.Equals(this.Description, other.Description) &&
                this.ParamGuid == other.ParamGuid &&
                ((this.Value == null && string.IsNullOrEmpty(other.Value))
                || this.Value == other.Value);

        }

        /// <summary>
        /// Compares this instance with another object for equality.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>True if the object is an EnhancedActInputValue instance and both instances are equal; otherwise, false.</returns>
        public new bool AreEqual(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals(obj as EnhancedActInputValue);
        }
    }
}
