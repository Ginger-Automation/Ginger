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

using System;
using SharpSvn;
using System.IO;

namespace GingerATS
{
    public class GingerATSSVNSourceControl
    {
        public GingerATSLog Logger = null;
        private SvnClient mClient;
        public SourceConnectionDetails mSourceConnectionDetails { get; set; }
        string mSolutionFullLocalPath=string.Empty;
        private SvnUpdateResult mOperationResult;
        public SvnUpdateResult OperationResult { get { return mOperationResult; } set { mOperationResult = value; } }
         
        public GingerATSSVNSourceControl(SourceConnectionDetails sourceConnectionDetails, string solutionFullLocalPath)
        {
            mSourceConnectionDetails = sourceConnectionDetails;
            mSolutionFullLocalPath = solutionFullLocalPath;
        }

        public void Init()
        {
            try
            {
                mClient = new SvnClient();

                //Configure the client profile folder
                string configPath = Path.Combine(Path.GetTempPath(), "sharpsvn");
                if (!Directory.Exists(configPath))
                {
                    Directory.CreateDirectory(configPath);
                }
                mClient.LoadConfiguration(configPath, true);

                mClient.Authentication.DefaultCredentials =
                    new System.Net.NetworkCredential(mSourceConnectionDetails.SourceUserName, mSourceConnectionDetails.SourceUserPass);
            }
            catch (Exception ex)
            {
                mClient = null;
                throw ex;
            }
        }

        public void GetProject()
        {
            if (mClient == null)
                Init();
            try
            {
                mClient.CheckOut(new Uri(mSourceConnectionDetails.SourceURL +
                                            mSourceConnectionDetails.GingerSolutionName + @"/SharedRepository"),
                                                mSolutionFullLocalPath + @"/SharedRepository", out mOperationResult);
            }
            catch (Exception ex)
            {
                Logger.AddLineToLog(eLogLineType.ERROR, "Failed to Clone the Repository to local, Error: " + ex.Message);
                mClient = null;
                throw ex;
            }
        }

        /// <summary>
        /// Get all files in path recursive 
        /// </summary>
        public void GetLatest()
        {
            if (mClient == null)
                Init();

            try
            {
                mClient.Update(Path.Combine(mSolutionFullLocalPath, "SharedRepository"), out mOperationResult);
            }
            catch (Exception ex)
            {
                Logger.AddLineToLog(eLogLineType.ERROR, "Failed to Update the Repository to local, Error: " + ex.Message);
                mClient = null;
                throw ex;
            }
        }
    }
}
