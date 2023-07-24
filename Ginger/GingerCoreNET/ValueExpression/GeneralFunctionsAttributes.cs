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

namespace Amdocs.Ginger.CoreNET.ValueExpression
{
    public class ValueExpressionFunctionAttribute : Attribute
    {
        public override string ToString()
        {
            return "ValueExpressionFunction";
        }
    }
    public class ValueExpressionFunctionDescription : Attribute
    {
        public string DefaultValue { get; set; }

        public ValueExpressionFunctionDescription(string DefaultValue)
        {
            this.DefaultValue = DefaultValue;
        }
    }
    public class ValueExpressionFunctionExpression : Attribute
    {
        public string DefaultValue { get; set; }

        public ValueExpressionFunctionExpression(string DefaultValue)
        {
            this.DefaultValue = DefaultValue;
        }
    }

    public class ValueExpressionFunctionCategory : Attribute
    {
        public string DefaultValue { get; set; }

        public ValueExpressionFunctionCategory(string DefaultValue)
        {
            this.DefaultValue = DefaultValue;
        }
    }

    public class ValueExpressionFunctionSubCategory : Attribute
    {
        public string DefaultValue { get; set; }

        public ValueExpressionFunctionSubCategory(string DefaultValue)
        {
            this.DefaultValue = DefaultValue;
        }
    }
}
