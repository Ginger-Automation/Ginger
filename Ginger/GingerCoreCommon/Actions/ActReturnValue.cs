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
using Amdocs.Ginger.Common.Repository;

namespace Amdocs.Ginger.Repository
{
    public class ActReturnValue : RepositoryItemBase
    {
        public enum eStatus
        {
            Pending,
            Passed,
            Failed,
            NA,
            Skipped
        }
        public enum eItemParts
        {
            All,
            Parameter,
            Path,
            ExpectedValue,
            SimulatedActual,
            StoreTo
        }

        public static partial class Fields
        {
            public static string Active = "Active";
            public static string Param = "Param";
            public static string Path = "Path";
            public static string Actual = "Actual";
            public static string SimulatedActual = "SimulatedActual";
            public static string Expected = "Expected";
            public static string ExpectedCalculated = "ExpectedCalculated";
            public static string Status = "Status";
            public static string StoreToVariable = "StoreToVariable";
            public static string StoreToDataSource = "StoreToDataSource";
            public static string StoreTo = "StoreTo";
            public static string StoreToValue = "StoreToValue";
        }

        [IsSerializedForLocalRepository]
        public bool Active { get; set; }

        private string mParam;
        public string ParamCalculated { get; set; }

        [IsSerializedForLocalRepository]
        public string Param { get { return mParam; } set { mParam = value; OnPropertyChanged(Fields.Param); } }

        public string PathCalculated { get; set; }

        private string mPath;
        [IsSerializedForLocalRepository]
        public string Path { get { return mPath; } set { mPath = value; OnPropertyChanged(Fields.Path); } }

        private string mActual;
        public string Actual { get { return mActual; } set { mActual = value; OnPropertyChanged(Fields.Actual); } }

        private string mSimulatedActual;
        [IsSerializedForLocalRepository]
        public string SimulatedActual { get { return mSimulatedActual; } set { mSimulatedActual = value; OnPropertyChanged(Fields.SimulatedActual); } }

        [IsSerializedForLocalRepository]
        public string mExpected { get; set; }
        public string Expected { get { return mExpected; } set { mExpected = value; OnPropertyChanged(Fields.Expected); } }

        private string mExpectedCalculated;
        public string ExpectedCalculated { get { return mExpectedCalculated; } set { mExpectedCalculated = value; OnPropertyChanged(Fields.ExpectedCalculated); } }

        public string ExpectedCalculatedValue { get; set; }
        private eStatus mStatus { get; set; }
        public eStatus Status { get { return mStatus; } set { mStatus = value; OnPropertyChanged(Fields.Status); } }

        [IsSerializedForLocalRepository]
        public bool AddedAutomatically { get; set; }

        public enum eStoreTo
        {
            None,
            Variable,
            DataSource,
            ApplicationModelParameter
        }
        private eStoreTo mStoreTo;
        [IsSerializedForLocalRepository]
        public eStoreTo StoreTo
        {
            get { return mStoreTo; }

            set
            {
                mStoreTo = value;
                OnPropertyChanged(Fields.StoreTo);
            }
        }

        private string mStoreToVariable = string.Empty;
        /// <summary>
        /// Do not use - check if can be deleted
        /// </summary>
        public string StoreToVariable
        {
            get
            {
                return mStoreToVariable;
            }
            set
            {
                if (value != null && value != "")
                {
                    mStoreTo = eStoreTo.Variable;
                    mStoreToValue = value;
                    OnPropertyChanged(Fields.StoreTo);
                    OnPropertyChanged(Fields.StoreToValue);
                }
            }
        }

        private string mStoreToValue;
        [IsSerializedForLocalRepository]
        public string StoreToValue
        {
            get
            {
                return mStoreToValue;
            }
            set
            {
                mStoreToValue = value;
                OnPropertyChanged(Fields.StoreToValue);
            }
        }

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

        public override string GetNameForFileName()
        {
            return mParam;
        }

        public override bool IsTempItem
        {
            get
            {
                // No need to save to XML if there is no expected value or no StoreToValue and no simulated Actual
                if (DoNotConsiderAsTemp == false && string.IsNullOrEmpty(Expected) && string.IsNullOrEmpty(StoreToValue) && string.IsNullOrEmpty(SimulatedActual))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [IsSerializedForLocalRepository]
        public bool DoNotConsiderAsTemp { get; set; }

    }
}
