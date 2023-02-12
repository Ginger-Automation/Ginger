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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository;
using System.Linq;

namespace Amdocs.Ginger.Repository
{
    public class AppModelParameter : RepositoryItemBase, IParentOptionalValuesObject
    {
        
        public virtual string ParamLevel { get { return "Local"; } set { } }

        string mPlaceHolder = string.Empty;
        [IsSerializedForLocalRepository]
        public string PlaceHolder
        {
            get
            {
                return mPlaceHolder;
            }
            set
            {
                if (mPlaceHolder != value)
                {
                    mPlaceHolder = value;
                    OnPropertyChanged(nameof(PlaceHolder));
                }
            }
        }

        string mDescription;
        [IsSerializedForLocalRepository]
        public string Description
        {
            get
            {
                return mDescription;
            }
            set
            {
                if(mDescription != value)
                {
                    mDescription = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        bool mRequiredAsInput = true;
        [IsSerializedForLocalRepository(true)]
        public bool RequiredAsInput
        {
            get
            {
                return mRequiredAsInput;
            }
            set
            {
                if (mRequiredAsInput != value)
                {
                    mRequiredAsInput = value;
                    OnPropertyChanged(nameof(RequiredAsInput));
                }
            }
        }

        [IsSerializedForLocalRepository]
        public string TagName { get; set; }


        string mPath;
        [IsSerializedForLocalRepository]
        public string Path
        {
            get
            {
                return mPath;
            }
            set
            {
                if (mPath != value)
                {
                    mPath = value;
                    OnPropertyChanged(nameof(Path));
                }
            }
        }

        /// <summary>
        /// Backward Support- please use 'Path' field instead
        /// </summary>
        public string XPath
        {
            get
            {
                return Path;
            }

            set
            {
                Path = value;
            }
        }


        public override string GetNameForFileName()
        {
            return this.PlaceHolder;
        }


        public override string ItemName
        {
            get
            {
                return this.PlaceHolder;
            }
            set
            {
                this.PlaceHolder = value;
            }
        }

        ObservableList<OptionalValue> mOptionalValuesList = new ObservableList<OptionalValue>();
        [IsSerializedForLocalRepository]
        public ObservableList<OptionalValue> OptionalValuesList
        {
            get
            {
                return mOptionalValuesList;
            }
            set
            {
                mOptionalValuesList = value;
            }
        }



        public string OptionalValuesString {
            get
            {
                string OptionalValuesString = string.Empty;
                foreach (OptionalValue optionalValue in OptionalValuesList)
                {
                    if(optionalValue.IsDefault)
                        OptionalValuesString += optionalValue.Value + "*,";
                    else
                        OptionalValuesString += optionalValue.Value + ",";
                }
                OptionalValuesString = OptionalValuesString.TrimEnd(',');

                return OptionalValuesString;
            }
        }

        public AppModelParameter()
        {
        }

        public AppModelParameter(string PlaceHolder, string Description, string TagName,string NodeXpath, ObservableList<OptionalValue> OptionalValuesList)
        {
            this.PlaceHolder = PlaceHolder;
            this.Description = Description;
            this.TagName = TagName;
            this.OptionalValuesList = OptionalValuesList;
            Path = NodeXpath;
        }

        public string GetDefaultValue()
        {
            OptionalValue defaultValue = OptionalValuesList.Where(x => x.IsDefault == true).FirstOrDefault();
            if (defaultValue != null)
                return defaultValue.Value;
            return string.Empty;
        }

        public string ExecutionValue { get; set; }//to be used in execution time for setting the template data


        /// <summary>
        /// Added for backward support- do not use
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Gets and sets ElementName for title on modeloption page
        /// </summary>
        public string ElementName
        {
            get
            {
                return ItemName;
            }
        }

        /// <summary>
        /// OnPropertyChanged Event Handler to raise the dirtystatus
        /// </summary>
        public void PropertyChangedEventHandler()
        {
            OnPropertyChanged(nameof(AppModelParameter.OptionalValuesString));
        }
    }   
}
