using System;
using Amdocs.Ginger.CoreNET.Run.SolutionCategory;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.Repository.SolutionCategories
{
    public class SolutionCategoryDefinition : RepositoryItemBase
    {        
        public static object Solution
        {
            get;set;
        }

        public SolutionCategoryDefinition()
        {

        }

        public SolutionCategoryDefinition(eSolutionCategories category)
        {
            this.Category = category;
        }

        [IsSerializedForLocalRepository]
        public eSolutionCategories Category
        {
            get;
            set;
        }

        public override string ItemName
        {
            get
            {
                return this.Category.ToString();
            }
            set
            {
                this.Category = (eSolutionCategories)Enum.Parse(typeof(eSolutionCategories), value.ToString());
            }
        }
        
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public ObservableList<SolutionCategoryValue> CategoryOptionalValues { get; set; }

        private Guid mSelectedValueID;
        [IsSerializedForLocalRepository]
        public Guid SelectedValueID
        {
            get { return mSelectedValueID; }
            set
            {
                if (mSelectedValueID != value)
                {
                    mSelectedValueID = value;
                    OnPropertyChanged(nameof(SelectedValueID));
                }
            }
        }
    }
}
