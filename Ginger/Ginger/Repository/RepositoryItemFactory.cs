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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.Variables;
using System;
using System.Collections.Generic;

namespace Ginger.Repository
{
    public class RepositoryItemFactory : IRepositoryItemFactory
    {
        
        public IValueExpression CreateValueExpression(ProjEnvironment mProjEnvironment, BusinessFlow mBusinessFlow)
        {
            return new ValueExpression(mProjEnvironment, mBusinessFlow);
        }

        public IValueExpression CreateValueExpression(ProjEnvironment mProjEnvironment, BusinessFlow mBusinessFlow, object DSList)
        {
            return new ValueExpression(mProjEnvironment, mBusinessFlow, (ObservableList<GingerCore.DataSource.DataSourceBase>)DSList);
        }

        public IValueExpression CreateValueExpression(ProjEnvironment Env, BusinessFlow BF, ObservableList<DataSourceBase> DSList = null, bool bUpdate = false, string UpdateValue = "", bool bDone = true, ObservableList<VariableBase> solutionVariables = null)
        {
            return new ValueExpression(Env, BF, (ObservableList<GingerCore.DataSource.DataSourceBase>)DSList, bUpdate, UpdateValue, bDone, solutionVariables);            
        }

        public IValueExpression CreateValueExpression(object obj, string attr)
        {
            return new ValueExpression(obj, attr);
        }


        public IActivitiesGroup CreateActivitiesGroup()
        {
            return new ActivitiesGroup();
        }
        public ObservableList<IDatabase> GetDatabaseList()
        {
            return new ObservableList<IDatabase>();
        }

        public ObservableList<DataSourceBase> GetDatasourceList()
        {
            return new ObservableList<DataSourceBase>();
        }



        //public Type GetRepositoryItemTypeFromInterface(Type interfaceType)
        //{
        //    if (interfaceType.IsInterface)
        //    {
        //        if (interfaceType == typeof(IAct))
        //        {
        //            return typeof(Act);
        //        }                
        //        else if (interfaceType == typeof(IActivitiesGroup))
        //        {
        //            return typeof(ActivitiesGroup);
        //        }
        //        else if (interfaceType == typeof(IAgent))
        //        {
        //            return typeof(Agent);
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //    else
        //    {
        //        return interfaceType;
        //    }
        //}

        public ObservableList<IAgent> GetAllIAgents()
        {
            return new ObservableList<IAgent>( WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>().ListItems.ConvertAll(x => (IAgent)x));
        }
        public ObservableList<ProjEnvironment> GetAllEnvironments()
        {
            return new ObservableList<ProjEnvironment>(WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().ListItems.ConvertAll(x => (ProjEnvironment)x));
        }
    }
}
