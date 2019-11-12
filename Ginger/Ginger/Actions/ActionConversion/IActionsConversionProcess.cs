using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET;
using GingerCore;

namespace Ginger.Actions
{
    public interface IActionsConversionProcess
    {
        ObservableList<BusinessFlowToConvert> ListOfBusinessFlow { get; set; }

        eModelConversionType ModelConversionType { get; }

        void BusinessFlowsActionsConversion(ObservableList<BusinessFlowToConvert> listOfBusinessFlow);

        void StopConversion();

        void ProcessConversion(ObservableList<BusinessFlowToConvert> listOfBusinessFlow, bool v);

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
