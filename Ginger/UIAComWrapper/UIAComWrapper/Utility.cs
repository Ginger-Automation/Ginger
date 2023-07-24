#region License
/*
Copyright © 2014-2023 European Support Limited

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
using System.Collections;
using System.Globalization;
using System.Windows.Automation;
using Windows.Foundation;

namespace UIAComWrapperInternal
{
    class Utility
    {
        private static void CheckNonNull(object el1, object el2)
        {
            if (el1 == null)
            {
                throw new ArgumentNullException("el1");
            }
            if (el2 == null)
            {
                throw new ArgumentNullException("el2");
            }
        }

        internal static ControlTypeExtended ConvertToControlType(int id)
        {
            return ControlTypeExtended.LookupById(id);
        }

        internal static int ConvertToInt(bool b)
        {
            return (b) ? 1 : 0;
        }

        internal static Rect ConvertToRect(UIAutomationClient.tagRECT rc)
        {
            return new Rect(rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top);
        }

        internal static AutomationElement_Extend[] ConvertToElementArray(UIAutomationClient.IUIAutomationElementArray array)
        {
            AutomationElement_Extend[] elementArray;
            if (array != null)
            {
                elementArray = new AutomationElement_Extend[array.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    elementArray[i] = AutomationElement_Extend.Wrap(array.GetElement(i));
                }
            }
            else
            {
                elementArray = null;
            }
            return elementArray;
        }

        internal static bool ConvertException(System.Runtime.InteropServices.COMException e, out Exception uiaException)
        {
            bool handled = true;
            switch (e.ErrorCode)
            {
                case UiaCoreIds.UIA_E_ELEMENTNOTAVAILABLE:
                    uiaException = new ElementNotAvailableExceptionExtended(e);
                    break;

                case UiaCoreIds.UIA_E_ELEMENTNOTENABLED:
                    uiaException = new ElementNotEnabledException(e);
                    break;

                case UiaCoreIds.UIA_E_NOCLICKABLEPOINT:
                    uiaException = new NoClickablePointException(e);
                    break;

                case UiaCoreIds.UIA_E_PROXYASSEMBLYNOTLOADED:
                    uiaException = new ProxyAssemblyNotLoadedException(e);
                    break;

                default:
                    uiaException = null;
                    handled = false;
                    break;
            }
            return handled;
        }

        internal static Array CombineArrays(IEnumerable arrays, Type t)
        {
            int length = 0;
            foreach (Array array in arrays)
            {
                length += array.Length;
            }
            Array destinationArray = Array.CreateInstance(t, length);
            int destinationIndex = 0;
            foreach (Array array3 in arrays)
            {
                int num3 = array3.Length;
                Array.Copy(array3, 0, destinationArray, destinationIndex, num3);
                destinationIndex += num3;
            }
            return destinationArray;
        }

        internal static Array RemoveDuplicates(Array a, Type t)
        {
            if (a.Length == 0)
            {
                return a;
            }
            Array.Sort(a);
            int index = 0;
            for (int i = 1; i < a.Length; i++)
            {
                if (!a.GetValue(i).Equals(a.GetValue(index)))
                {
                    index++;
                    a.SetValue(a.GetValue(i), index);
                }
            }
            int length = index + 1;
            if (length == a.Length)
            {
                return a;
            }
            Array destinationArray = Array.CreateInstance(t, length);
            Array.Copy(a, 0, destinationArray, 0, length);
            return destinationArray;
        }

        internal static UIAutomationClient.tagPOINT PointManagedToNative(Point pt)
        {
            UIAutomationClient.tagPOINT nativePoint = new UIAutomationClient.tagPOINT();
            nativePoint.x = (int)pt.X;
            nativePoint.y = (int)pt.Y;
            return nativePoint;
        }

        internal static void ValidateArgument(bool cond, string reason)
        {
            if (!cond)
            {
                throw new ArgumentException(reason);
            }
        }

        internal static void ValidateArgumentNonNull(object obj, string argName)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(argName);
            }
        }

        internal static void ValidateCached(bool cached)
        {
            if (!cached)
            {
                throw new InvalidOperationException("Cache Request Needs Cache");
            }
        }

        internal static object WrapObjectAsPattern(AutomationElement_Extend el, object nativePattern, AutomationPatternExtended pattern, bool cached)
        {
            PatternTypeInfo info;
            if (!Schema.GetPatternInfo(pattern, out info))
            {
                throw new ArgumentException("Unsupported pattern");
            }
            if (info.ClientSideWrapper == null)
            {
                return null;
            }
            return info.ClientSideWrapper(el, nativePattern, cached);
        }

        internal static object WrapObjectAsProperty(AutomationPropertyExtended property, object obj)
        {
            PropertyTypeInfo info;

            // Handle the cases that we know.
            if (obj == AutomationElement_Extend.NotSupported)
            {
                // No-op
            }
            else if (obj is UIAutomationClient.IUIAutomationElement)
            {
                obj = AutomationElement_Extend.Wrap((UIAutomationClient.IUIAutomationElement)obj);
            }
            else if (obj is UIAutomationClient.IUIAutomationElementArray)
            {
                obj = Utility.ConvertToElementArray((UIAutomationClient.IUIAutomationElementArray)obj);
            }
            else if (Schema.GetPropertyTypeInfo(property, out info))
            {
                // Well known properties
                if ((obj != null) && (info.ObjectConverter != null))
                {
                    obj = info.ObjectConverter(obj);
                }
            }

            return obj;
        }

        // Unwrap an object from API representation to what the native client will expect
        internal static object UnwrapObject(object val)
        {
            if (val != null)
            {
                if (val is ControlTypeExtended)
                {
                    val = ((ControlTypeExtended)val).Id;
                }
                else if (val is Rect)
                {
                    Rect rect = (Rect)val;
                    val = new double[] { rect.Left, rect.Top, rect.Width, rect.Height };
                }
                else if (val is Point)
                {
                    Point point = (Point)val;
                    val = new double[] { point.X, point.Y };
                }
                else if (val is CultureInfo)
                {
                    val = ((CultureInfo)val).LCID;
                }
                else if (val is AutomationElement_Extend)
                {
                    val = ((AutomationElement_Extend)val).NativeElement;
                }
            }
            return val;
        }
    }
}
