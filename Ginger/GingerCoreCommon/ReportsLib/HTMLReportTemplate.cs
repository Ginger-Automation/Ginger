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

using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common.Repository;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GingerCore;


namespace Ginger.Reports
{
    public class HTMLReportTemplate : RepositoryItemBase
    {
        

        public enum eReportStatus
        {
            Development = 0,
            Active = 1,
            Obsolete = 2
        }

        private string mDescription;
        private string mHTML;
        private string mName;
        private eReportStatus mStatus;

        [IsSerializedForLocalRepository]
        public string Name
        {
            get { return mName; }
            set
            {
                if (mName != value)
                {
                    mName = value;
                    OnPropertyChanged(Fields.Name);
                }
            }
        }

        [IsSerializedForLocalRepository]
        public string Description
        {
            get { return mDescription; }
            set
            {
                if (mDescription != value)
                {
                    mDescription = value;
                    OnPropertyChanged(Fields.Description);
                }
            }
        }

        [IsSerializedForLocalRepository]
        public eReportStatus Status
        {
            get { return mStatus; }
            set
            {
                if (mStatus != value)
                {
                    mStatus = value;
                    OnPropertyChanged(Fields.Status);
                }
            }
        }

        [IsSerializedForLocalRepository]
        public string HTML
        {
            get { return mHTML; }
            set
            {
                if (mHTML != value)
                {
                    mHTML = value;
                    OnPropertyChanged(Fields.HTML);
                }
            }
        }

        public override string GetNameForFileName()
        {
            return Name;
        }

        public static List<HTMLReportTemplate> GetInternalTemplates()
        {
            var list = new List<HTMLReportTemplate>();
            var RT1 = new HTMLReportTemplate();
            RT1.Name = "Summary HTML Report";
            RT1.Description = "aaa";
            RT1.HTML = GetInternalReportHTML("SummaryHTMLReport.html");
            list.Add(RT1);

            var RT2 = new HTMLReportTemplate();
            RT2.Name = "Skeleton for Customized HTML Report";
            RT2.Description = "aaa";
            RT2.HTML = GetInternalReportHTML("SkeletonHTMLReport.html");
            list.Add(RT2);
            return list;
        }

        private static string GetInternalReportHTML(string name)
        {
            Assembly ExecutingAssembly;
            ExecutingAssembly = Assembly.GetExecutingAssembly();
            var stream = ExecutingAssembly.GetManifestResourceStream("Ginger.Reports.HTMLTemplates." + name);
            var reader = new StreamReader(stream);
            var HTML = reader.ReadToEnd();
            return HTML;
        }

        public  static class Fields
        {
            public static string Name = "Name";
            public static string Description = "Description";
            public static string Status = "Status";
            public static string HTML = "HTML";
        }

        public override string ItemName
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }

        public override string GetItemType()
        {
            return nameof(HTMLReportTemplate);
        }


    }
}
