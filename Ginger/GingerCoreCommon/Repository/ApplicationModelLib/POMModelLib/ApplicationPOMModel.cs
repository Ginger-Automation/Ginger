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
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.UIElement;
using System.Linq;

namespace Amdocs.Ginger.Repository
{
    public class ApplicationPOMModel : ApplicationModelBase
    {
        public ApplicationPOMModel()
        {
            UIElements = new ObservableList<ElementInfo>();
        }


        [IsSerializedForLocalRepository]
        public ObservableList<ElementInfo> UIElements;

        //[IsSerializedForLocalRepository]
        //public ObservableList<ElementInfo> MappedUIElements
        //{
        //    get
        //    {
        //        // TODO: temp fix me to be connected and not get new all time
        //        //return new ObservableList<ElementInfo>(from x in UIElements where x.Active == true select x);
        //    }
        //}


        // ******************************************************************************
        //DO NOT  DELETE Temp commented for moving to GingerCoreCommon
        // ******************************************************************************
        //public Bitmap ScreenShot { get; set; }


        //TODO: add how to auto detect this POM: from Title = 'login', from Title= "Hello - * - zz system", or...
        //TODO: add activitiesPArams - 
    }
}
