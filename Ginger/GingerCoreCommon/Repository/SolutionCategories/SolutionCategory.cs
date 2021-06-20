using System;
using System.ComponentModel;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.CoreNET.Run.SolutionCategory
{
    public enum eSolutionCategories
    {
        [DescriptionAttribute("Product")]
        Product,
        [DescriptionAttribute("Test Type")]
        TestType,
        [DescriptionAttribute("Release")]
        Release,
        [DescriptionAttribute("Iteration")]
        Iteration,
        [DescriptionAttribute("User Category 1")]
        UserCategory1,
        [DescriptionAttribute("User Category 2")]
        UserCategory2,
        [DescriptionAttribute("User Category 3")]
        UserCategory3,
    }

    public class SolutionCategory: RepositoryItemBase
    {
        public SolutionCategory()
        {

        }

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

        public string CategoryName
        {
            get
            {
                try
                {
                    DescriptionAttribute[] attributes = (DescriptionAttribute[])typeof(eSolutionCategories).GetField(Category.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);                    
                    if (attributes.Length > 0)
                    {
                        return attributes[0].Description;
                    }
                    else
                    {
                        return Category.ToString();
                    }
                }
                catch
                {
                    return Category.ToString();
                }
            }
        }

        private string mDescription;
        [IsSerializedForLocalRepository]
        public string Description
        {
            get { return mDescription; }
            set
            {
                if (mDescription != value)
                {
                    mDescription = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        public override string ItemName
        {
            get
            {
                return this.CategoryName;
            }
            set
            {               
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
