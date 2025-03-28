#region License
/*
Copyright © 2014-2025 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

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
