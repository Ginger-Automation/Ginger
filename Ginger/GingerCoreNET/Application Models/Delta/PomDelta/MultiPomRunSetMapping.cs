using Amdocs.Ginger.Repository;
using Ginger.Run;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerCoreNET.Application_Models
{
    public class MultiPomRunSetMapping : RepositoryItemBase
    {
        private bool mSelected = false;

        public bool Selected { get { return mSelected; } set { if (mSelected != value) { mSelected = value; OnPropertyChanged(nameof(Selected)); } } }
        public RunSetConfig runSetConfig { get; set; }

        public string runsetName { get; set; }

        public List<ApplicationPOMModel> applicationPOMModels { get; set; }

        public string commaSeparatedApplicationPOMModels { get; set; }

        public override string ItemName { get; set; }
    }
}
