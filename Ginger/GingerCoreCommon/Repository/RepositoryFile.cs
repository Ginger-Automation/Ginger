#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using System.IO;

namespace Amdocs.Ginger.Repository
{
    public class RepositoryFile
    {

        public enum eFileType
        {
            Unknown,  // Must be first
            BusinessFlow,
            Agent,
            Document,  // TODO: - rename to resources            
            Environment,
            RunSetConfig,
            HTMLReportConfiguration,
            Activity,
            Action,

            //TODO: all the rest 
        }


        public string FilePath { get; set; }

        private eFileType? mFileType = null;

        /// <summary>
        /// Based on file name return the Repository Item type
        /// </summary>
        /// 
        public eFileType? FileType
        {
            get
            {
                // we catch the result for speed
                if (mFileType == null)
                {
                    mFileType = GetFileType();                    
                }
                return mFileType;
            }
        }

        private eFileType? GetFileType()
        {            
            string s = Path.GetFileName(FilePath);
            if (s.Contains(".BusinessFlow.xml")) return eFileType.BusinessFlow;   // TODO: use const
            if (s.Contains(".Agent.xml")) return eFileType.Agent;   // TODO: use const
            if (s.Contains(".Environment.xml")) return eFileType.Environment;   // TODO: use const
            if (s.Contains(".HTMLReportConfiguration.xml")) return eFileType.HTMLReportConfiguration;   // TODO: use const
            if (s.Contains(".RunSetConfig.xml")) return eFileType.RunSetConfig;   // TODO: use const            
            if (s.Contains(".Activity.xml")) return eFileType.Activity;   // TODO: use const            
            if (s.Contains(".Action.xml")) return eFileType.Action;   // TODO: use const            
            // TODO: return all the rest         

            return eFileType.Unknown;
        }

        public string FileVersion { get; set; }
    }
}
