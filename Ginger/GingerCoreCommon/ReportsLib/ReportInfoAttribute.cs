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

namespace Ginger.Reports
{
    public enum FieldsType
    {
        Field,
        Section
    };

    public class FieldParams : Attribute
    {
        public override string ToString()
        {
            return "Field had Parameters";
        }
    }

    public class FieldParamsNameCaption : Attribute
    {
        public string NameCaption { get; set; }

        public FieldParamsNameCaption(string NameCaption)
        {
            this.NameCaption = NameCaption;
        }
    }

    public class FieldParamsFieldType : Attribute
    {
        public FieldsType FieldType { get; set; }

        public FieldParamsFieldType(FieldsType FieldType)
        {
            this.FieldType = FieldType;
        }
    }

    public class FieldParamsIsNotMandatory : Attribute
    {
        public bool IsNotMandatory { get; set; }

        public FieldParamsIsNotMandatory(bool IsNotMandatory)
        {
            this.IsNotMandatory = IsNotMandatory;
        }
    }

    public class FieldParamsIsSelected : Attribute
    {
        public bool IsSelected { get; set; }

        public FieldParamsIsSelected(bool IsSelected)
        {
            this.IsSelected = IsSelected;
        }
    }

    public class UsingUTCTimeFormat : Attribute
    {
        public UsingUTCTimeFormat()
        {
        }
    }
}
