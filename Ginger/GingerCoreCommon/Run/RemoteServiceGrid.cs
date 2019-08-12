using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.Run
{
    public class RemoteServiceGrid : RepositoryItemBase
    {
        [IsSerializedForLocalRepository]
        public string Name { get; set; }

        [IsSerializedForLocalRepository]
        public string Host { get; set; }

        [IsSerializedForLocalRepository]
        public int HostPort { get; set; }

        [IsSerializedForLocalRepository]
        public bool Active { get; set; }


        public override string ItemName
        {
            get
            {
                return Name;
            }
            set
            {
                Name = value;
            }
        }

    }
}
