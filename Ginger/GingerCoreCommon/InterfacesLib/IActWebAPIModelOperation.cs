using Amdocs.Ginger.Repository;
using GingerCore.Actions.WebServices;
using GingerCore.Actions.WebServices.WebAPI;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public interface IActWebAPIModelOperation
    {
        void FillAPIBaseFields(ApplicationAPIModel AAMB, ActWebAPIBase actWebAPIBase, ActWebAPIModel actWebAPIModel);
    }
}
