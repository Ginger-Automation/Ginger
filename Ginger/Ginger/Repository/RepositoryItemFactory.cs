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
using Amdocs.Ginger.Common.InterfacesLib;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Environments;
using GingerCore.Variables;
using System;

namespace Ginger.Repository
{
    public class RepositoryItemFactory : IRepositoryItemFactory
    {
        public IBusinessFlow CreateBusinessFlow()
        {
            return new BusinessFlow();
        }

        public ObservableList<IBusinessFlow> GetListofBusinessFlow()
        {
            return new ObservableList<IBusinessFlow>();
        }
        public IValueExpression CreateValueExpression(IProjEnvironment mProjEnvironment, IBusinessFlow mBusinessFlow)
        {
            return new ValueExpression(mProjEnvironment, mBusinessFlow);
        }

        public IValueExpression CreateValueExpression(IProjEnvironment Env, IBusinessFlow BF, ObservableList<IDataSourceBase> DSList = null, bool bUpdate = false, string UpdateValue = "", bool bDone = true, ObservableList<VariableBase> solutionVariables = null)
        {
            throw new System.NotImplementedException();
        }
        public IActivitiesGroup CreateActivitiesGroup()
        {
            return new ActivitiesGroup();
        }
        public ObservableList<IDatabase> GetDatabaseList()
        {
            return new ObservableList<IDatabase>();
        }

        public IGingerRunner RunExecutioFrom(Amdocs.Ginger.Common.eExecutedFrom executedFrom)
        {
            return new GingerRunner(executedFrom);
        }

        public Type GetRepositoryItemTypeFromInterface(Type interfaceType)
        {
            if (interfaceType.IsInterface)
            {
                if (interfaceType == typeof(IAct))
                {
                    return typeof(Act);
                }
                else if (interfaceType == typeof(IActivity))
                {
                    return typeof(Activity);
                }
                else if (interfaceType == typeof(IActivitiesGroup))
                {
                    return typeof(ActivitiesGroup);
                }
                else if (interfaceType == typeof(IAgent))
                {
                    return typeof(Agent);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return interfaceType;
            }
        }

        
    }
}
