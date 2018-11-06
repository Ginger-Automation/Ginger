namespace Amdocs.Ginger.Common
{
    public interface IRepositoryItemFactory
    {
        IBusinessFlow CreateBusinessFlow();
        IValueExpression CreateValueExpression(IProjEnvironment mProjEnvironment, IBusinessFlow mBusinessFlow);
    }
}
