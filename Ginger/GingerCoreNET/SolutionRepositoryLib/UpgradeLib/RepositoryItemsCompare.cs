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

//using Amdocs.Ginger.Common;
//using Amdocs.Ginger.Common.Repository;
//using Amdocs.Ginger.Repository;
//using GingerCoreNET.GeneralLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActivitiesLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.BusinessFlowLib;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;

//namespace GingerCoreNET.SolutionRepositoryLib.UpgradeLib
//{
//    public class RepositoryItemsCompare
//    {
//        List<CompareDiff> list = new List<CompareDiff>();
//        public void CompareIt(RepositoryItem RILeft, RepositoryItem RIRight)
//        {
//            Compare<BusinessFlow>((BusinessFlow)RILeft, (BusinessFlow)RIRight);
//        }

//        public List<CompareDiff> GetDiffs()
//        {
//            return list;
//        }

//        private void CheckDiff(Object LeftObject, Object RightObject, string PropertyName, object left, object right)
//        {
//            CompareDiff CD = new CompareDiff();
//            CD.LeftObject = LeftObject;
//            CD.RightObject = RightObject;
//            CD.PropertyName = PropertyName;
//            CD.LeftValue = GetObjValueAsString(left);
//            CD.RightValue = GetObjValueAsString(right);
            
//            list.Add(CD);
//        }

//        string GetObjValueAsString(object obj)
//        {
//            // We might want to do smarter obj to string based on the obj type 
//            if (obj == null) return null;
//            return obj.ToString();
//        }

//        void Compare<T>(T Object1, T object2)
//        {
//            //Get the type of the object
//            Type type = typeof(T);

//            //return false if any of the object is null
//            if (Object1 == null || object2 == null)
//            {
//                throw new Exception("Cannot compare null object");
//            }

//            // Comapre Properties
//            var properties = type.GetMembers().Where(x => x.MemberType == MemberTypes.Property).OrderBy(x => x.Name);
//            foreach (MemberInfo mi in properties)
//            {
//                IsSerializedForLocalRepositoryAttribute token = Attribute.GetCustomAttribute(mi, typeof(IsSerializedForLocalRepositoryAttribute), false) as IsSerializedForLocalRepositoryAttribute;
//                if (token == null) continue;

//                //Get tha attr value
//                var v1 = type.GetProperty(mi.Name).GetValue(Object1);
//                var v2 = type.GetProperty(mi.Name).GetValue(object2);

//                CheckDiff(Object1, object2, mi.Name, v1, v2);
//            }

//            // Comapre Fields
//            var Fields = type.GetMembers().Where(x => x.MemberType == MemberTypes.Field).OrderBy(x => x.Name);

//            foreach (MemberInfo fi in Fields)
//            {
//                IsSerializedForLocalRepositoryAttribute token = Attribute.GetCustomAttribute(fi, typeof(IsSerializedForLocalRepositoryAttribute), false) as IsSerializedForLocalRepositoryAttribute;
//                if (token == null) continue;

//                if (fi.Name == nameof(BusinessFlow.Activities))
//                {
//                }

//                var v1 = type.GetField(fi.Name).GetValue(Object1);
//                var v2 = type.GetField(fi.Name).GetValue(object2);

//                if (v1 is IObservableList && v2 is IObservableList)
//                {
//                    // Comapre Lists
//                    IObservableList vv = (IObservableList)v1;
//                    foreach(var vv1 in vv)
//                    {
//                    }
//                }
//            }
//        }
//    }
//}
