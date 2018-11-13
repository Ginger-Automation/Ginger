//#region License
///*
//Copyright © 2014-2018 European Support Limited

//Licensed under the Apache License, Version 2.0 (the "License")
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License. 
//*/
//#endregion

//using Amdocs.Ginger.Repository;
//using Amdocs.Ginger.Common;
//using Amdocs.Ginger.Common.Repository;
//using GingerCoreNET.GeneralLib;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActivitiesLib
//{
//    /// <summary>
//    /// ActivitiesGroup used to store the ID's and execution order of few Activities
//    /// </summary>
//    public class ActivitiesGroup : RepositoryItem
//    {
//        public enum eItemParts
//        {
//            All,
//            Details,
//            Activities
//        }

//        public static class Fields
//        {
//            public static string Name = "Name";
//            public static string Description = "Description";
//            public static string ActivitiesIdentifiers = "ActivitiesIdentifiers";
//            public static string AutomationPrecentage = "AutomationPrecentage";
//        }

//        public ActivitiesGroup()
//        {
//            Tags = new ObservableList<Guid>();
//        }

//        private string mName;
//        [IsSerializedForLocalRepository]
//        public string Name
//        {
//            get { return mName; }
//            set
//            {
//                if (mName != value)
//                {
//                    mName = value;

//                    foreach (ActivityIdentifiers aIdent in ActivitiesIdentifiers)
//                        if (aIdent.IdentifiedActivity != null)
//                            aIdent.IdentifiedActivity.ActivitiesGroupID = mName;

//                    OnPropertyChanged(Fields.Name);
//                }
//            }
//        }

//        private string mDescription;
//        [IsSerializedForLocalRepository]
//        public string Description
//        {
//            get { return mDescription; }
//            set
//            {
//                if (mDescription != value)
//                {
//                    mDescription = value;
//                    OnPropertyChanged(Fields.Description);
//                }
//            }
//        }

//        [IsSerializedForLocalRepository]
//        public ObservableList<ActivityIdentifiers> ActivitiesIdentifiers = new ObservableList<ActivityIdentifiers>();

//        [IsSerializedForLocalRepository]
//        public ObservableList<Guid> Tags = new ObservableList<Guid>();

//        public override string GetNameForFileName() { return Name; }

//        public void AddActivityToGroup(Activity activity)
//        {
//            ActivityIdentifiers actIdents = new ActivityIdentifiers();
//            actIdents.IdentifiedActivity = activity;
//            activity.ActivitiesGroupID = this.Name;
//            this.ActivitiesIdentifiers.Add(actIdents);
//        }

//        [IsSerializedForLocalRepository]
//        public string ExternalID2 { get; set; } // used to store the actual TC ID when importing it from QC in case the TC type is linked TC


//        public string AutomationPrecentage 
//        { 
//            get
//            {
//                foreach (ActivityIdentifiers actIdent in ActivitiesIdentifiers) actIdent.RefreshActivityIdentifiers();
//                List<ActivityIdentifiers> automatedActsInGroup = ActivitiesIdentifiers.Where(x=>x.ActivityAutomationStatus ==
//                                                                               Activity.eActivityAutomationStatus.Automated).ToList();
//                double automatedActsPrecanteg;
//                if (automatedActsInGroup == null || automatedActsInGroup.Count == 0)
//                {
//                    automatedActsPrecanteg=0;
//                }
//                else
//                {
//                    automatedActsPrecanteg = ((double)automatedActsInGroup.Count / (double)ActivitiesIdentifiers.Count);
//                    automatedActsPrecanteg = Math.Floor(automatedActsPrecanteg *100);                    
//                }

//                return automatedActsPrecanteg.ToString() + "%";
//            }
//        }

//        public override string ItemName
//        {
//            get
//            {
//                return this.Name;
//            }
//            set
//            {
//                this.Name = value;
//            }
//        }
//    }
//}
