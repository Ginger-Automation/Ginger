using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Amdocs.Ginger.Common.GlobalSolutionLib
{
    public class GlobalSolution
    {
        public enum ImportFromType
        {
            LocalFolder,
            SourceControl,
            Package//Need to discuss
        }
        public enum ImportItemType
        {
            [EnumValueDescription("Business Flows")]
            BusinessFlows,
            [EnumValueDescription("Shared Repository - Activities Group")]
            SharedRepositoryActivitiesGroup,
            [EnumValueDescription("Shared Repository - Activities")]
            SharedRepositoryActivities,
            [EnumValueDescription("Shared Repository - Actions")]
            SharedRepositoryActions,
            [EnumValueDescription("Shared Repository - Variables")]
            SharedRepositoryVariables,
            [EnumValueDescription("API Models")]
            APIModels,
            [EnumValueDescription("Model Global Parameters")]
            ModelGlobalParameters,
            [EnumValueDescription("Target Applications")]
            TargetApplications,
            [EnumValueDescription("Global Variables")]
            GlobalVariables,
            [EnumValueDescription("Runsets")]
            Runsets,
            [EnumValueDescription("Report Templates")]
            ReportTemplates,
            [EnumValueDescription("Agents")]
            Agents,
            [EnumValueDescription("Documents")]
            Documents,
            [EnumValueDescription("Environments")]
            Environments,
            [EnumValueDescription("DataSources")]
            DataSources
        }

        public static IEnumerable<T> GetEnumValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
    public class GlobalSolutionItem: INotifyPropertyChanged
    {
        public GlobalSolutionItem(GlobalSolution.ImportItemType ItemType, string ItemExtraInfo, bool Selected, string ItemFullPath, string ItemName)
        {
            this.ItemType = ItemType;
            this.ItemExtraInfo= ItemExtraInfo;
            this.Selected = Selected;
            this.ItemFullPath = ItemFullPath;
            this.ItemName = ItemName;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool mSelected = true;
        public bool Selected { get { return mSelected; } set { if (mSelected != value) { mSelected = value; OnPropertyChanged(nameof(Selected)); } } }
        public GlobalSolution.ImportItemType ItemType { get; set; }
        public string ItemExtraInfo { get; set; }
        public string ItemFullPath { get; set; }
        public string ItemName { get; set; }

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
