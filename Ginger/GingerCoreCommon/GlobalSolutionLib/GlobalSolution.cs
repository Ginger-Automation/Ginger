using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Amdocs.Ginger.Common.GlobalSolutionLib
{
    public class GlobalSolution
    {
        public enum eImportFromType
        {
            LocalFolder,
            SourceControl,
            Package//Need to discuss
        }
        public enum eImportItemType
        {
            //[EnumValueDescription("Business Flows")]
            //BusinessFlows,
            //[EnumValueDescription("Shared Repository - Activities Group")]
            //SharedRepositoryActivitiesGroup,
            //[EnumValueDescription("Shared Repository - Activities")]
            //SharedRepositoryActivities,
            //[EnumValueDescription("Shared Repository - Actions")]
            //SharedRepositoryActions,
            //[EnumValueDescription("Shared Repository - Variables")]
            //SharedRepositoryVariables,
            //[EnumValueDescription("API Models")]
            //APIModels,
            //[EnumValueDescription("Target Applications")]
            //TargetApplications,
            //[EnumValueDescription("Runsets")]
            //Runsets,
            //[EnumValueDescription("Report Templates")]
            //ReportTemplates,
            //[EnumValueDescription("Agents")]
            //Agents,
            [EnumValueDescription("Documents")]
            Documents,
            [EnumValueDescription("Environments")]
            Environments,
            [EnumValueDescription("DataSources")]
            DataSources
        }
        public enum eItemDependancyType
        {
            Original,
            Dependant,
            None
        }
        public static IEnumerable<T> GetEnumValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public enum eImportSetting
        {
            CreateNew,
            ReplaceExsiting,
            KeepLocal
        }

    }
    //public class GlobalSolutionItem: INotifyPropertyChanged
    //{
    //    public GlobalSolutionItem(GlobalSolution.ImportItemType ItemType, string ItemExtraInfo, bool Selected, string ItemName, GlobalSolution.ItemDependancyType ItemDependancyType)
    //    {
    //        this.ItemType = ItemType;
    //        this.ItemExtraInfo= ItemExtraInfo;
    //        this.Selected = Selected;
    //        this.ItemName = ItemName;
    //        this.ItemDependancyType = ItemDependancyType;

    //    }

    //    public event PropertyChangedEventHandler PropertyChanged;

    //    private bool mSelected = true;
    //    public bool Selected { get { return mSelected; } set { if (mSelected != value) { mSelected = value; OnPropertyChanged(nameof(Selected)); } } }
    //    public GlobalSolution.eImportItemType ItemType { get; set; }
    //    public string ItemExtraInfo { get; set; }
    //    public string ItemName { get; set; }

    //    public GlobalSolution.eItemDependancyType ItemDependancyType { get; set; }
    //    public void OnPropertyChanged(string name)
    //    {
    //        PropertyChangedEventHandler handler = PropertyChanged;
    //        if (handler != null)
    //        {
    //            handler(this, new PropertyChangedEventArgs(name));
    //        }
    //    }
    //}
}
