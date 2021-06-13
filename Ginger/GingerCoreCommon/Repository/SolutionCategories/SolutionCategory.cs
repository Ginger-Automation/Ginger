using System;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.CoreNET.Run.SolutionCategory
{
    public enum eSolutionCategories
    {
        Product,
        TestType,
        Release,
        Iteration,
        UserCategory1,
        UserCategory2,
        UserCategory3,
    }

    public class SolutionCategory: RepositoryItemBase
    {
        public SolutionCategory(eSolutionCategories category)
        {
            this.Category = category;
        }

        private eSolutionCategories mCategory;
        [IsSerializedForLocalRepository]
        public eSolutionCategories Category
        {
            get { return mCategory; }
            set
            {
                if (mCategory != value)
                {
                    mCategory = value;
                    OnPropertyChanged(nameof(Category));
                }
            }
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

        [IsSerializedForLocalRepository]
        public ObservableList<SolutionCategoryValue> CategoryOptionalValues = new ObservableList<SolutionCategoryValue>();

        public string CategoryOptionalValuesString
        {
            get
            {
                string OptionalValuesString = string.Empty;
                foreach (SolutionCategoryValue optionalValue in CategoryOptionalValues)
                {
                        OptionalValuesString += optionalValue.Value + ",";
                }
                OptionalValuesString = OptionalValuesString.TrimEnd(',');
                return OptionalValuesString;
            }
        }

        public void PropertyChangedEventHandler()
        {
            OnPropertyChanged(nameof(CategoryOptionalValuesString));
        }
    }
}
