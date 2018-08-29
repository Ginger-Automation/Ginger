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
using System.Linq;

namespace Amdocs.Ginger.Repository
{


    public class AppModelParameter : RepositoryItemBase
    {
        public override bool UseNewRepositorySerializer { get { return true; } }
        public virtual string ParamLevel { get { return "Local"; } set { } }
       
        [IsSerializedForLocalRepository]
        public string PlaceHolder { get; set; } = string.Empty;

        [IsSerializedForLocalRepository]
        public string Description { get; set; }

        [IsSerializedForLocalRepository]
        public bool RequiredAsInput { get; set; } = true;

        [IsSerializedForLocalRepository]
        public string TagName { get; set; }

        [IsSerializedForLocalRepository]
        public string Path { get; set; }

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

        [IsSerializedForLocalRepository]
        public ObservableList<OptionalValue> OptionalValuesList = new ObservableList<OptionalValue>();



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



    }

    /// <summary>
    /// This class is used in the Export Process
    /// </summary>
    public class AppParameters
    {     
        public string ItemName { get; set; }

        [IsSerializedForLocalRepository]
        public ObservableList<OptionalValue> OptionalValuesList = new ObservableList<OptionalValue>();
        
        public string OptionalValuesString { get; set; }

        public string Description { get; set; }
    }
}
