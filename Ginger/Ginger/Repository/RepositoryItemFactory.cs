using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
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
