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


namespace GingerATS
{
    /// <summary>
    /// This class is used for grouping the details which required for establish connection to the Ginger source control
    /// </summary>
    public class SourceConnectionDetails
    {
        /// <summary>
        /// Source control URL
        /// </summary>
        public string SourceURL { get; set; }
        /// <summary>
        /// Source control User Name
        /// </summary>
        public string SourceUserName { get; set; }
        /// <summary>
        /// Source control User Password
        /// </summary>
        public string SourceUserPass { get; set; }
        /// <summary>
        /// The Ginger Solution name which Ginger supposed to connect to in the source control and pull the automation status
        /// </summary>
        public string GingerSolutionName { get; set; }
        /// <summary>
        /// Local path for storing the data from the source control while analyzing it
        /// </summary>
        public string GingerSolutionLocalPath { get; set; }

        /// <summary>
        /// This class is used for grouping the details which required for establish connection to the Ginger source control
        /// </summary>
        /// <param name="sourceURL">
        /// <param name="sourceUserName">Source control User Name</param>
        /// <param name="sourceUserPass">Source control User Password</param>
        /// <param name="gingerSolutionName">The Ginger Solution name which Ginger supposed to connect to in the source control 
        ///                                     and pull the automation status</param>
        /// <param name="gingerSolutionLocalPath">Local path for storing the data from the source control while analyzing it</param>
        public SourceConnectionDetails(string sourceURL, string sourceUserName, string sourceUserPass,
                                    string gingerSolutionName, string gingerSolutionLocalPath)
        {
            this.SourceURL = sourceURL;
            this.SourceUserName = sourceUserName;
            this.SourceUserPass = sourceUserPass;
            this.GingerSolutionName = gingerSolutionName;
            this.GingerSolutionLocalPath = gingerSolutionLocalPath;
        }
    }
}
