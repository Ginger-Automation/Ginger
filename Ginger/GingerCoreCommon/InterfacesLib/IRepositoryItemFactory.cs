using System;
using System.Collections.Generic;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.Variables;

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

namespace Amdocs.Ginger.Common
{
    public enum eExecutedFrom
    {
        Automation,
        Run
    }
    

    public interface IRepositoryItemFactory
    { 
        //BusinessFlow CreateBusinessFlow();
        //ObservableList<BusinessFlow> GetListofBusinessFlow();
        IActivitiesGroup CreateActivitiesGroup();
        IValueExpression CreateValueExpression(ProjEnvironment mProjEnvironment, BusinessFlow mBusinessFlow);

        IValueExpression CreateValueExpression(ProjEnvironment Env, BusinessFlow BF, ObservableList<DataSourceBase> DSList = null, bool bUpdate = false, string UpdateValue = "", bool bDone = true, ObservableList<VariableBase> solutionVariables = null);

        IValueExpression CreateValueExpression(Object obj, string attr);

        ObservableList<IDatabase> GetDatabaseList();
        ObservableList<IAgent> GetAllIAgents();
        ObservableList<ProjEnvironment> GetAllEnvironments();
    }
}
