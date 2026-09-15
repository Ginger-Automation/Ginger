#region License
/*
Copyright © 2014-2026 European Support Limited

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

using System;

namespace GingerCore.Drivers.Common.LegacyAutomation
{
    public enum DesktopOperation
    {
        Click,
        SetValue,
        GetValue
    }

    public sealed class DesktopActionContext
    {
        public DesktopOperation Operation { get; set; }
        public object AutomationElement { get; set; }
        public IntPtr NativeWindowHandle { get; set; }
        public string Value { get; set; }

        /// <summary>
        /// Only the call sites that already moved the real mouse before this engine
        /// existed may enable this. Physical input is the last resort because it
        /// clicks screen coordinates and hits whatever window is on top.
        /// </summary>
        public bool AllowPhysicalInput { get; set; }

        public int TimeoutMs { get; set; } = 5000;
    }

    public enum LayerExecutionStatus
    {
        Skipped,
        Failed,
        Succeeded
    }

    public sealed class LayerResult
    {
        public LayerExecutionStatus Status { get; private set; }
        public string LayerName { get; set; }
        public string Message { get; set; }
        public string OutputValue { get; set; }

        public static LayerResult Skip(string message)
        {
            return new LayerResult { Status = LayerExecutionStatus.Skipped, Message = message };
        }

        public static LayerResult Ok(string layerName, string message, string outputValue = null)
        {
            return new LayerResult { Status = LayerExecutionStatus.Succeeded, LayerName = layerName, Message = message, OutputValue = outputValue };
        }

        public static LayerResult Fail(string layerName, string message)
        {
            return new LayerResult { Status = LayerExecutionStatus.Failed, LayerName = layerName, Message = message };
        }
    }

    public interface IDesktopAutomationLayer
    {
        string Name { get; }
        bool CanHandle(DesktopActionContext context);
        LayerResult TryExecute(DesktopActionContext context);
    }

    public sealed class DesktopEngineResult
    {
        public bool Success { get; set; }
        public string UsedLayer { get; set; }
        public string ExecutionInfo { get; set; }
        public string ErrorMessage { get; set; }
        public string OutputValue { get; set; }
    }
}
