using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET
{
    public class POMObjectRecordingHelper
    {
        public ApplicationPOMModel ApplicationPOM { get; set; }
        public string PageTitle { get; set; }
        public string PageURL { get; set; }

        public POMObjectRecordingHelper()
        {

        }

        public POMObjectRecordingHelper(ApplicationPOMModel model, string pageTitle, string pageURL)
        {
            ApplicationPOM = model;
            PageTitle = pageTitle;
            PageURL = pageURL;
        }
    }
}
