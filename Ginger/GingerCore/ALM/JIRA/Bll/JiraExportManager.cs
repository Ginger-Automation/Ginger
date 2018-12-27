using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerCore.ALM.JIRA.Bll
{
    public class JiraExportManager
    {
        private JiraRepository.JiraRepository jiraRepObj;
        public JiraExportManager(JiraRepository.JiraRepository jiraRep)
        {
            this.jiraRepObj = jiraRep;
        }

    }
}
