using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET;
using GingerCore;

namespace Ginger.Actions
{
    public interface IConversionProcess
    {
        ObservableList<BusinessFlowToConvert> ListOfBusinessFlow { get; set; }

        eConversionType ConversionType { get; set; }

        bool ParameterizeRequestBody { get; set; }

        void BusinessFlowsActionsConversion(ObservableList<BusinessFlowToConvert> listOfBusinessFlow);

        void StopConversion();

        void ProcessConversion(ObservableList<BusinessFlowToConvert> listOfBusinessFlow, bool v);

        void ConversionProcessEnded();

        void ConversionProcessStarted();

        int GetConvertibleActionsCountFromBusinessFlow(BusinessFlow bf);
    }

    public enum eConversionType
    {
        ActionConversion,
        ApiActionConversion
    }
}
