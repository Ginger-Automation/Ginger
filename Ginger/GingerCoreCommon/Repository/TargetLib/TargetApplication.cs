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
using System.Collections.Generic;
using System.Linq;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Repository;

namespace GingerCore.Platforms
{
    public class TargetApplication : TargetBase
    {
        public override string Name { get { return AppName; } }


        
        public Guid TargetGuid
        {
            get
            {
                return this.ParentGuid;
            }
            set
            {
                if (this.ParentGuid != value)
                {
                    this.ParentGuid = value;
                    OnPropertyChanged(nameof(this.ParentGuid));
                }
            }
        }


        string mAppName;
        //TODO: how about use GUID or add it for in case        
        [IsSerializedForLocalRepository]
        public string AppName
        {
            get
            {
                return mAppName;
            }
            set
            {
                if (mAppName != value)
                {
                    mAppName = value;
                    OnPropertyChanged(nameof(AppName));
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public TargetApplication() { }

        public TargetApplication(RIBXmlReader reader) : base(reader) { }

        protected override IEnumerable<PropertyParser<RepositoryItemBase,string>> AttributeParsers()
        {
            return _attributeParsers;
            //return base.AttributeParsers().Concat(new List<PropertyParser<string>>()
            //{
            //    new(nameof(AppName), value => AppName = value)
            //});
        }

        protected static new readonly IEnumerable<PropertyParser<RepositoryItemBase,string>> _attributeParsers =
            TargetBase._attributeParsers.Concat(new List<PropertyParser<RepositoryItemBase,string>>()
            {
                new(nameof(AppName), (rib,value) => ((TargetApplication)rib).AppName = value)
            });

        protected override void ParseAttribute(string attributeName, string attributeValue)
        {
            base.ParseAttribute(attributeName, attributeValue);
            if (string.Equals(attributeName, nameof(AppName)))
                AppName = attributeValue;
        }

        protected override void DeserializeProperty(RIBXmlReader reader)
        {
            base.DeserializeProperty(reader);

            if (reader.IsName(nameof(AppName)))
                AppName = reader.Value;
        }
    }
}
