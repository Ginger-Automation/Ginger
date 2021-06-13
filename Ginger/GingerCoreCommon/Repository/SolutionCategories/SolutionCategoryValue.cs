using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.CoreNET.Run.SolutionCategory
{
    public class SolutionCategoryValue : RepositoryItemBase
    {
        public SolutionCategoryValue (string value)
        {
            this.Value = value;
        }

        private string mValue;
        [IsSerializedForLocalRepository]
        public string Value
        {
            get { return mValue; }
            set
            {
                if (mValue != value)
                {
                    mValue = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public override string ItemName
        {
            get
            {
                return this.Value;
            }
            set
            {
                this.Value = value;
            }
        }
    }
}
