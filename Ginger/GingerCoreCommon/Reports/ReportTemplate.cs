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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Ginger.Run;
using GingerCore;
using GingerCore.Environments;

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;


namespace Ginger.Reports
{
    public class ReportTemplate : RepositoryItemBase, IReportTemplate
    {        

        public override string GetNameForFileName() { return Name; }

        public enum eReportStatus
        {
            Development = 0,
            Active = 1,
            Obsolete = 2
        }

        public static partial class Fields
        {            
            public static string Name = "Name";
            public static string Description = "Description";         
            public static string Status = "Status";
            public static string Xaml = "Xaml";            
        }

        private string mName;

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
       
        private string mDescription;
        [IsSerializedForLocalRepository]
        public string Description { get { return mDescription; } set { if (mDescription != value) { mDescription = value; OnPropertyChanged(Fields.Description); } } }
        
        eReportStatus mStatus;
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

        private string mXaml;
        [IsSerializedForLocalRepository]
        public string Xaml
        {
            get { return mXaml; }
            set
            {
                if (mXaml != value)
                {
                    mXaml = value;
                    OnPropertyChanged(Fields.Xaml);
                }
            }
        }

        public static List<ReportTemplate> GetInternalTemplates()
        {
            List<ReportTemplate> list = new List<ReportTemplate>();

            ReportTemplate RT1 = new ReportTemplate();
            RT1.Name = "Summary Report";
            RT1.Description = "aaa";
            RT1.Xaml = GetInternalReportXaml("SummaryReport.xaml");
            list.Add(RT1);

            ReportTemplate RT2 = new ReportTemplate();
            RT2.Name = "Totals Report";
            RT2.Description = "aaa";
            RT2.Xaml = GetInternalReportXaml("TotalsReport.xaml");
            list.Add(RT2);

            ReportTemplate RT3 = new ReportTemplate();
            RT3.Name = "Full Detailed Report";
            RT3.Description = "aaa";
            RT3.Xaml = GetInternalReportXaml("FullDetailedReport.xaml");
            list.Add(RT3);

            ReportTemplate RT4 = new ReportTemplate();
            RT4.Name = "Screenshots Report";
            RT4.Description = @"Showing Activity Details and all of it's actions screenshots without action level details";
            RT4.Xaml = GetInternalReportXaml("ScreenshotReport.xaml");
            list.Add(RT4);

            return list;
        }

        static string GetInternalReportXaml(string name)
        {
            System.Reflection.Assembly ExecutingAssembly;
            ExecutingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.Stream stream = ExecutingAssembly.GetManifestResourceStream("Ginger.Reports.InternalTemplates." + name);
            StreamReader reader = new StreamReader(stream);
            string Xaml = reader.ReadToEnd();
            return Xaml;
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
            return nameof(ReportTemplate);
        }

    }
}
