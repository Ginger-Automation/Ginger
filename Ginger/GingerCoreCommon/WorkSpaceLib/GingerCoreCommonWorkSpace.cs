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

using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.SolutionGeneral;

namespace Amdocs.Ginger.Common.WorkSpaceLib
{
    public class GingerCoreCommonWorkSpace
    {
        private static readonly GingerCoreCommonWorkSpace _instance = new GingerCoreCommonWorkSpace();
        public static GingerCoreCommonWorkSpace Instance
        {
            get
            {
                return _instance;
            }
        }

        public UserProfile UserProfile
        {
            get;
            set;
        }

        public Solution Solution
        {
            get;
            set;
        }

        public SolutionRepository SolutionRepository
        {
            get;
            set;
        }
        public ISharedRepositoryOperations SharedRepositoryOperations
        {
            get;
            set;
        }
    }
}
