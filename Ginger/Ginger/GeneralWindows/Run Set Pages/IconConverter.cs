#region License
/*
Copyright © 2014-2019 European Support Limited

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

using Amdocs.Ginger.Common.Enums;
using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using static Ginger.Run.GingerRunner;

namespace Ginger.MoveToGingerWPF.Run_Set_Pages
{
    public class StatusIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {            
            eImageType ico = eImageType.Pending;//default icon

            if (value != null && value.GetType() == typeof(Amdocs.Ginger.CoreNET.Execution.eRunStatus))
            {
                Amdocs.Ginger.CoreNET.Execution.eRunStatus status = (Amdocs.Ginger.CoreNET.Execution.eRunStatus)value;
                switch (status)
                {
                    case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed:
                        ico = eImageType.Passed;
                        break;
                    case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed:
                        ico = eImageType.Failed;
                        break;
                    case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running:
                        ico = eImageType.Running;
                        break;
                    case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending:
                        ico = eImageType.Pending;
                        break;
                    case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped:
                        ico = eImageType.Stopped;
                        break;
                    case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked:
                        ico = eImageType.Blocked;
                        break;
                    case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped:
                        ico = eImageType.Skipped;
                        break;
                    default:
                        ico = eImageType.Pending;
                        break;
                }
            }
            
            return ico;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ActiveIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            eImageType ico = new eImageType();           
            bool status = (bool)value;
            switch (status)
            {
                case true:
                    ico = eImageType.Active;
                    break;
                case false:
                    ico = eImageType.InActive;
                    break;
            }
             return ico;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ReverseBooleanToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var b = (bool)value;
            if (b)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }

    public class RunOptionIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            eImageType ico = new eImageType();
            eRunOptions runoption = (eRunOptions)value;
            switch (runoption)
            {
                case eRunOptions.ContinueToRunall:
                    ico = eImageType.RunAll;
                    break;
                case eRunOptions.StopAllBusinessFlows:
                    ico = eImageType.StopAll;
                    break;
            }
            return ico;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RunSimulationModeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush b = null;
            bool RunInSimulationMode = (bool)value;
            switch (RunInSimulationMode)
            {
                case true:
                    b = Brushes.Blue;
                    break;
                case false:
                    b = Brushes.Gray;
                    break;
            }
            return b;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }   
}
