using System;
using Amdocs.Ginger.Common.InterfacesLib;
 using Amdocs.Ginger.Repository;
using GingerCore.Variables;

namespace Amdocs.Ginger.Common
{
    public enum eExecutedFrom
    {
        Automation,
        Run
    }
    

    public interface IRepositoryItemFactory
    { 
        IBusinessFlow CreateBusinessFlow();
        ObservableList<IBusinessFlow> GetListofBusinessFlow();
        IActivitiesGroup CreateActivitiesGroup();
        IValueExpression CreateValueExpression(IProjEnvironment mProjEnvironment, IBusinessFlow mBusinessFlow);

        IValueExpression CreateValueExpression(IProjEnvironment Env, IBusinessFlow BF, ObservableList<IDataSourceBase> DSList = null, bool bUpdate = false, string UpdateValue = "", bool bDone = true, ObservableList<VariableBase> solutionVariables = null);

        IGingerRunner RunExecutioFrom(eExecutedFrom eExecutedFrom);
        
        ObservableList<IDatabase> GetDatabaseList();
        Type GetRepositoryItemTypeFromInterface(Type interfaceType);
    }
}
