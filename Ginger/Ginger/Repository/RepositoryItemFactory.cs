using Amdocs.Ginger.Common;
using GingerCore;

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

    }
}
