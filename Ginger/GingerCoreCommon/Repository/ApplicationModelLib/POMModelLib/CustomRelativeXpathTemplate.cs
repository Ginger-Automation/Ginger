#region License
/*
Copyright © 2014-2023 European Support Limited

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
namespace Amdocs.Ginger.Repository
{
    public class CustomRelativeXpathTemplate : RepositoryItemBase
    {
        public enum SyntaxValidationStatus
        {
            Passed,
            Failed
        }

        private string mValue;

        [IsSerializedForLocalRepository]
        public string Value
        {
            get
            {
                return mValue;
            }
            set
            {
                if (mValue != value)
                {
                    mValue = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        private SyntaxValidationStatus mStatus;
        public SyntaxValidationStatus Status
        {
            get
            {
                return mStatus;
            }
            set
            {
                if (mStatus != value)
                {
                    mStatus = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }
        public override string ItemName
        {
            get { return this.Value; }
            set
            {
                //do nothing 
            }
        }
    }
}
