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

using System.Diagnostics;
using UIAComWrapperInternal;

namespace System.Windows.Automation
{
    public abstract class ConditionExtended
    {
        
        public static readonly ConditionExtended FalseCondition = BoolCondition.Wrap(false);
        public static readonly ConditionExtended TrueCondition = BoolCondition.Wrap(true);

        internal abstract UIAutomationClient.IUIAutomationCondition NativeCondition { get; }

        internal static ConditionExtended Wrap(UIAutomationClient.IUIAutomationCondition obj)
        {
            if (obj is UIAutomationClient.IUIAutomationBoolCondition)
                return new BoolCondition((UIAutomationClient.IUIAutomationBoolCondition)obj);
            else if (obj is UIAutomationClient.IUIAutomationAndCondition)
                return new AndConditionExtended((UIAutomationClient.IUIAutomationAndCondition)obj);
            else if (obj is UIAutomationClient.IUIAutomationOrCondition)
                return new OrCondition((UIAutomationClient.IUIAutomationOrCondition)obj);
            else if (obj is UIAutomationClient.IUIAutomationNotCondition)
                return new NotCondition((UIAutomationClient.IUIAutomationNotCondition)obj);
            else if (obj is UIAutomationClient.IUIAutomationPropertyCondition)
                return new PropertyConditionExtended((UIAutomationClient.IUIAutomationPropertyCondition)obj);
            else
                throw new ArgumentException("obj");
        }

        internal static UIAutomationClient.IUIAutomationCondition ConditionManagedToNative(
            ConditionExtended ConditionExtended)
        {
            return (ConditionExtended == null) ? null : ConditionExtended.NativeCondition;
        }

        internal static UIAutomationClient.IUIAutomationCondition[] ConditionArrayManagedToNative(
            ConditionExtended[] conditions)
        {
            UIAutomationClient.IUIAutomationCondition[] unwrappedConditions =
                new UIAutomationClient.IUIAutomationCondition[conditions.Length];
            for (int i = 0; i < conditions.Length; ++i)
            {
                unwrappedConditions[i] = ConditionManagedToNative(conditions[i]);
            }
            return unwrappedConditions;
        }

        internal static ConditionExtended[] ConditionArrayNativeToManaged(
            Array conditions)
        {
            ConditionExtended[] wrappedConditions = new ConditionExtended[conditions.Length];
            for (int i = 0; i < conditions.Length; ++i)
            {
                wrappedConditions[i] = Wrap((UIAutomationClient.IUIAutomationCondition)conditions.GetValue(i));
            }
            return wrappedConditions;
        }

        
        private class BoolCondition : ConditionExtended
        {
            
            internal UIAutomationClient.IUIAutomationBoolCondition _obj;

            internal BoolCondition(UIAutomationClient.IUIAutomationBoolCondition obj)
            {
                Debug.Assert(obj != null);
                this._obj = obj;
            }

            internal override UIAutomationClient.IUIAutomationCondition NativeCondition
            {
                get { return this._obj; }
            }

            internal static BoolCondition Wrap(bool b)
            {
                UIAutomationClient.IUIAutomationBoolCondition obj = (UIAutomationClient.IUIAutomationBoolCondition)((b) ?
                    AutomationExtended.Factory.CreateTrueCondition() :
                    AutomationExtended.Factory.CreateFalseCondition());
                return new BoolCondition(obj);
            }
        }
    }

    public class NotCondition : ConditionExtended
    {
        
        internal UIAutomationClient.IUIAutomationNotCondition _obj;

        
        internal NotCondition(UIAutomationClient.IUIAutomationNotCondition obj)
        {
            Debug.Assert(obj != null);
            this._obj = obj;
        }

        public NotCondition(ConditionExtended ConditionExtended)
        {
            this._obj = (UIAutomationClient.IUIAutomationNotCondition)
                AutomationExtended.Factory.CreateNotCondition(
                ConditionManagedToNative(ConditionExtended));
        }

        internal override UIAutomationClient.IUIAutomationCondition NativeCondition
        {
            get { return this._obj; }
        }

        
        public ConditionExtended ConditionExtended
        {
            get
            {
                return Wrap(this._obj.GetChild());
            }
        }
    }
    
    public class AndConditionExtended : ConditionExtended
    {
        
        internal UIAutomationClient.IUIAutomationAndCondition _obj;

        
        internal AndConditionExtended(UIAutomationClient.IUIAutomationAndCondition obj)
        {
            Debug.Assert(obj != null);
            this._obj = obj;
        }

        public AndConditionExtended(params ConditionExtended[] conditions)
        {
            this._obj = (UIAutomationClient.IUIAutomationAndCondition)
                AutomationExtended.Factory.CreateAndConditionFromArray(
                ConditionArrayManagedToNative(conditions));
        }

        internal override UIAutomationClient.IUIAutomationCondition NativeCondition
        {
            get { return this._obj; }
        }

        public ConditionExtended[] GetConditions()
        {
            return ConditionArrayNativeToManaged(this._obj.GetChildren());
        }
    }

    public class OrCondition : ConditionExtended
    {
        
        internal UIAutomationClient.IUIAutomationOrCondition _obj;

        
        internal OrCondition(UIAutomationClient.IUIAutomationOrCondition obj)
        {
            Debug.Assert(obj != null);
            this._obj = obj;
        }

        public OrCondition(params ConditionExtended[] conditions)
        {
            this._obj = (UIAutomationClient.IUIAutomationOrCondition)
                AutomationExtended.Factory.CreateOrConditionFromArray(
                ConditionArrayManagedToNative(conditions));
        }

        internal override UIAutomationClient.IUIAutomationCondition NativeCondition
        {
            get { return this._obj; }
        }

        public ConditionExtended[] GetConditions()
        {
            return ConditionArrayNativeToManaged(this._obj.GetChildren());
        }
    }

    [Flags]
    public enum PropertyConditionFlags
    {
        None,
        IgnoreCase
    }

    public class PropertyConditionExtended : ConditionExtended
    {
        
        internal UIAutomationClient.IUIAutomationPropertyCondition _obj;

        
        internal PropertyConditionExtended(UIAutomationClient.IUIAutomationPropertyCondition obj)
        {
            Debug.Assert(obj != null);
            this._obj = obj;
        }

        public PropertyConditionExtended(AutomationPropertyExtended property, object value)
        {
            this.Init(property, value, PropertyConditionFlags.None);
        }

        public PropertyConditionExtended(AutomationPropertyExtended property, object value, PropertyConditionFlags flags)
        {
            this.Init(property, value, flags);
        }

        private void Init(AutomationPropertyExtended property, object val, PropertyConditionFlags flags)
        {
            Utility.ValidateArgumentNonNull(property, "property");

            this._obj = (UIAutomationClient.IUIAutomationPropertyCondition)
                AutomationExtended.Factory.CreatePropertyConditionEx(
                property.Id,
                Utility.UnwrapObject(val), 
                (UIAutomationClient.PropertyConditionFlags)flags);
        }

        internal override UIAutomationClient.IUIAutomationCondition NativeCondition
        {
            get { return this._obj; }
        }

        
        public PropertyConditionFlags Flags
        {
            get
            {
                return (PropertyConditionFlags)this._obj.PropertyConditionFlags;
            }
        }

        public AutomationPropertyExtended Property
        {
            get
            {
                return AutomationPropertyExtended.LookupById(this._obj.propertyId);
            }
        }

        public object Value
        {
            get
            {
                return this._obj.PropertyValue;
            }
        }
    }
}
