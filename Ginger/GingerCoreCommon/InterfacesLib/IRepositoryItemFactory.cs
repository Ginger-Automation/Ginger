using Amdocs.Ginger.Common.InterfacesLib;
 using Amdocs.Ginger.Repository;
using GingerCore.Variables;

namespace Amdocs.Ginger.Common
{
    public interface IRepositoryItemFactory
    {
        IBusinessFlow CreateBusinessFlow();
        IActivitiesGroup CreateActivitiesGroup();
        IValueExpression CreateValueExpression(IProjEnvironment mProjEnvironment, IBusinessFlow mBusinessFlow);

        IValueExpression CreateValueExpression(IProjEnvironment Env, IBusinessFlow BF, ObservableList<IDataSourceBase> DSList = null, bool bUpdate = false, string UpdateValue = "", bool bDone = true, ObservableList<VariableBase> solutionVariables = null);


        ObservableList<IDatabase> GetDatabaseList();
    }
}
