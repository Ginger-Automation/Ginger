using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET;

namespace Ginger.Actions
{
    public interface IConversionProcess
    {
        ObservableList<BusinessFlowToConvert> ListOfBusinessFlow { get; set; }

        void BusinessFlowsActionsConversion(ObservableList<BusinessFlowToConvert> listOfBusinessFlow);

        void StopConversion();

        void ProcessConversion(ObservableList<BusinessFlowToConvert> listOfBusinessFlow, bool v);
    }
}
