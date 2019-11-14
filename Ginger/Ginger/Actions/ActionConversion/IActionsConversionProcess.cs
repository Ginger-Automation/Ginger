using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET;
using GingerCore;
using System.Threading.Tasks;

namespace Ginger.Actions
{
    public interface IActionsConversionProcess
    {
        ObservableList<BusinessFlowToConvert> ListOfBusinessFlow { get; set; }

        eModelConversionType ModelConversionType { get; }

        Task BusinessFlowsActionsConversion(ObservableList<BusinessFlowToConvert> listOfBusinessFlow);

        void StopConversion();

        Task ProcessConversion(ObservableList<BusinessFlowToConvert> listOfBusinessFlow, bool v);

        void ConversionProcessEnded();

        void ConversionProcessStarted();

        int GetConvertibleActionsCountFromBusinessFlow(BusinessFlow bf);
    }

    public enum eModelConversionType
    {
        ActionConversion,
        ApiActionConversion
    }
}
