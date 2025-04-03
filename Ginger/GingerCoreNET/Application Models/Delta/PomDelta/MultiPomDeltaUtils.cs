﻿#region License
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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Application_Models;
using Amdocs.Ginger.CoreNET.NewSelfHealing;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace GingerCoreNET.Application_Models
{
    
    public class MultiPomDeltaUtils
    {
        RepositoryFolder<ApplicationPOMModel> mPomModelsFolder;
        public ApplicationPOMModel POM = null;
        public PomLearnUtils PomLearnUtils = null;
        public Agent Agent = null;
        public ObservableList<ApplicationPOMModel> mPOMModels = new ObservableList<ApplicationPOMModel>();

        public ObservableList<MultiPomRunSetMapping> MultiPomRunSetMappingList = new ObservableList<MultiPomRunSetMapping>();

        public MultiPomDeltaUtils(ApplicationPOMModel pom, Agent agent = null, RepositoryFolder<ApplicationPOMModel> pomModelsFolder = null)
        {
            POM = pom;
            Agent = agent;
            mPomModelsFolder = pomModelsFolder;
            PomLearnUtils = new PomLearnUtils(pom, agent);
        }

    }

    
}
