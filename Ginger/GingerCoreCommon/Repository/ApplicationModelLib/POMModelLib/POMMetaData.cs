using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using static Amdocs.Ginger.Repository.CustomRelativeXpathTemplate;

namespace Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib
{
    public class POMMetaData : RepositoryItemBase
    {
        public enum MetaDataType
        {
            Form,
            Tags
        }
        private MetaDataType mType;
        [IsSerializedForLocalRepository]
        public MetaDataType Type
        {
            get
            {
                return mType;
            }
            set
            {
                mType = value;
                OnPropertyChanged(nameof(Type));
            }
        }

        [IsSerializedForLocalRepository]
        public string Name { get; set; }


        [IsSerializedForLocalRepository]        
        public ObservableList<ElementMetaData> ElementsMetaData { get; set; } = new ObservableList<ElementMetaData>();

        public override string ItemName
        {
            get { return this.Name; }
            set
            {
                //do nothing 
            }
        }
    }

    public class ElementMetaData : RepositoryItemBase
    {
        [IsSerializedForLocalRepository]
        public Guid ElementGuid { get; set; }

        [IsSerializedForLocalRepository]
        public int OredrId { get; set; }

        public override string ItemName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
