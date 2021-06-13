using System;
using Amdocs.Ginger.CoreNET.Run.SolutionCategory;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.Repository.SolutionCategories
{
    public class SolutionCategoryDefinition : RepositoryItemBase
    {
        public SolutionCategoryDefinition(eSolutionCategories category, SolutionCategoryValue value)
        {
            this.Category = category;
            this.SelectedValue = value;
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
                return string.Format("{0}='{1}'", this.Category.ToString(), this.SelectedValue.Value);
            }
            set
            {
                this.Category = (eSolutionCategories)Enum.Parse(typeof(eSolutionCategories), value.ToString());
            }
        }

        public ObservableList<SolutionCategoryValue> CategoryOptionalValues;

        private SolutionCategoryValue mSelectedValue;
        [IsSerializedForLocalRepository]
        public SolutionCategoryValue SelectedValue
        {
            get { return mSelectedValue; }
            set
            {
                if (mSelectedValue != value)
                {
                    mSelectedValue = value;
                    OnPropertyChanged(nameof(SelectedValue));
                }
            }
        }
    }
}
