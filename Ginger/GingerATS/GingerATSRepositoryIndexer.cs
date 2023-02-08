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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;

namespace GingerATS
{
    public class GingerATSRepositoryIndexer
    {
        string mSolutionFolderPath = string.Empty;
        string mRepositoryFolderPath = string.Empty;
        string mIndexerFilePath = string.Empty;

        public bool IsIndexerExist
        {
            get
            {
                return (File.Exists(mIndexerFilePath));
            }
        }

        public string IndexerPath
        {
            get
            {
                return (mIndexerFilePath);
            }
        }

        public GingerATSRepositoryIndexer(string solutionFolderPath)
        {
            mSolutionFolderPath = solutionFolderPath;
            mRepositoryFolderPath = Path.Combine(mSolutionFolderPath, "SharedRepository");
            mIndexerFilePath = Path.Combine(mSolutionFolderPath, "SharedRepository", "RepositoryIndexer.txt");
        }

        public void RefreshRepositoryIndexer()
        {
            //check if repo file exist
            if (IsIndexerExist == false)
            {
                CreateRepositoryIndexer();
            }
            else
            {
                UpdateRepositoryIndexer();
            }
        }

        public void CreateRepositoryIndexer()
        {
            List<string> indexerRecords = new List<string>();
            List<GingerRepositoryItem> repoItems;

            repoItems = GetAllRepositoryFiles();
            if (repoItems != null && repoItems.Count > 0)
            {
                foreach (GingerRepositoryItem repoItem in repoItems)
                {
                    SetRepositoryItemDetails(repoItem);
                    if (repoItem.XmlFileDetails != null && repoItem.XmlFileDetails != string.Empty)
                        indexerRecords.Add(repoItem.XmlFileDetails);
                }
            }

            //write to file
            WriteRepositoryIndexerData(indexerRecords);
        }


        public void UpdateRepositoryIndexer()
        {
            DateTime indexLastRan = File.GetLastWriteTime(mIndexerFilePath);
            List<string> UpdatedIndexerRecords = new List<string>();
            //Get the current file records which was not deleted/changed
            List<string> currentIndexerRecords = ReadRepositoryIndexerData();
            foreach (string line in currentIndexerRecords)
            {
                //check if file exists
                string[] indexItems = line.Split(';');
                if (File.Exists(indexItems[4]) && File.GetLastWriteTime(indexItems[4]) <= indexLastRan)
                {
                    UpdatedIndexerRecords.Add(line);
                }
            }

            //Add new/updated repo items
            List<GingerRepositoryItem> repoItems;
            repoItems = GetAllRepositoryFiles();
            if (repoItems != null && repoItems.Count > 0)
            {
                foreach (GingerRepositoryItem repoItem in repoItems)
                {
                    if (File.GetLastWriteTime(repoItem.FilePath) > indexLastRan)
                    {
                        SetRepositoryItemDetails(repoItem);
                        if (repoItem.XmlFileDetails != null && repoItem.XmlFileDetails != string.Empty)
                            UpdatedIndexerRecords.Add(repoItem.XmlFileDetails);
                    }
                }
            }

            WriteRepositoryIndexerData(UpdatedIndexerRecords);
        }

        private void WriteRepositoryIndexerData(List<string> dataToWrite)
        {
            bool succeedWritingFile = false;
            int tryingCounter = 1;
            Exception savedEx = null;
            while (succeedWritingFile == false && tryingCounter <= 3) //TODO: implement better way to syncronaize the calls to this function
            {
                try
                {
                    File.WriteAllLines(mIndexerFilePath, dataToWrite);
                    succeedWritingFile = true;
                }
                catch (Exception ex)
                {
                    Thread.Sleep(1000);
                    tryingCounter++;
                    savedEx = ex;
                }

                if (succeedWritingFile == false && tryingCounter > 3 && savedEx != null)
                    throw savedEx;
            }
        }

        public List<string> ReadRepositoryIndexerData()
        {
            List<string> IndexerLines = new List<string>();
            bool succeedReadingFile = false;
            int tryingCounter = 1;
            Exception savedEx = null;
            while (succeedReadingFile == false && tryingCounter <= 3) //TODO: implement better way to syncronaize the calls to this function
            {
                try
                {
                    System.IO.StreamReader file = new System.IO.StreamReader(mIndexerFilePath);
                    IndexerLines = (file.ReadToEnd()).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList<string>(); //fastest avg 702 ticks while ((indexLine = file.ReadLine()) != null) //this way is 100xs slower average 6124 ticks.
                    file.Close();
                    succeedReadingFile = true;
                }
                catch (Exception)
                {
                    Thread.Sleep(1000);
                    tryingCounter++;
                }
            }

            if (succeedReadingFile == false && tryingCounter > 3 && savedEx != null)
                throw savedEx;
            else
                return IndexerLines;
        }

        private List<GingerRepositoryItem> GetAllRepositoryFiles()
        {
            List<GingerRepositoryItem> repoFilesList = new List<GingerRepositoryItem>();

            //Get the ActivitiesGroups items
            string activitiesGroupsFolderPath = mRepositoryFolderPath + @"\" +
                                                    GingerRepositoryItem.GetRepoItemMainFolderName(eGingerRepoItemType.ActivitiesGroup) + @"\";
            if (Directory.Exists(activitiesGroupsFolderPath))
            {
                string[] files = Directory.GetFiles(activitiesGroupsFolderPath, "*", SearchOption.AllDirectories);
                if (files != null && files.Length > 0)
                    foreach (string filePath in files)
                        if (!filePath.ToUpper().Contains("PREVVERSIONS") && !filePath.ToUpper().Contains(".SVN") && Path.GetExtension(filePath).ToUpper() == ".XML")
                        {
                            GingerRepositoryItem activitiesGroupItem = new GingerRepositoryItem(eGingerRepoItemType.ActivitiesGroup);
                            activitiesGroupItem.FilePath = filePath;
                            repoFilesList.Add(activitiesGroupItem);
                        }
            }

            //Get the Activities items
            string activitiesFolderPath = mRepositoryFolderPath + @"\" +
                                                    GingerRepositoryItem.GetRepoItemMainFolderName(eGingerRepoItemType.Activity) + @"\";
            if (Directory.Exists(activitiesFolderPath))
            {
                string[] files = Directory.GetFiles(activitiesFolderPath, "*", SearchOption.AllDirectories);
                if (files != null && files.Length > 0)
                    foreach (string filePath in files)
                        if (!filePath.ToUpper().Contains("PREVVERSIONS") && !filePath.ToUpper().Contains(".SVN") && Path.GetExtension(filePath).ToUpper() == ".XML")
                        {
                            GingerRepositoryItem activityItem = new GingerRepositoryItem(eGingerRepoItemType.Activity);
                            activityItem.FilePath = filePath;
                            repoFilesList.Add(activityItem);
                        }
            }

            //TODO: Get the Actions items if needed for Ginger-ATS integration

            //TODO: Get the Variables items if needed for Ginger-ATS integration

            return repoFilesList;
        }

        private void SetRepositoryItemDetails(GingerRepositoryItem repoItem)
        {
            //get the XML reader
            XmlReader xmlReader = GingerATSXmlReader.GetXMLReaderFromFile(repoItem.FilePath);
            if (xmlReader != null)
            {
                //get the details from the xml
                if (GingerATSXmlReader.MoveXmlReaderToSpecificNode(xmlReader, GingerRepositoryItem.GetRepoItemMainXmlNodeName(repoItem.Type)))
                {
                    repoItem.Name = xmlReader.GetAttribute(GingerRepositoryItem.GetRepoItemNameFieldLabel(repoItem.Type));
                    repoItem.GUID = xmlReader.GetAttribute(eGeneralGingerRepoAttributes.Guid.ToString());
                    repoItem.ExternalID = xmlReader.GetAttribute(eGeneralGingerRepoAttributes.ExternalID.ToString());
                    if (repoItem.ExternalID == null) repoItem.ExternalID = "Null";
                    repoItem.LastUpdated = xmlReader.GetAttribute(eGeneralGingerRepoAttributes.LastUpdate.ToString());

                    repoItem.XmlFileDetails = GingerRepositoryItem.GetRepoItemTypeLabelInIndexer(repoItem.Type) + ";" +
                                                repoItem.Name + ";" +
                                                repoItem.GUID + ";" +
                                                repoItem.ExternalID + ";" +
                                                repoItem.FilePath + ";" +
                                                repoItem.LastUpdated;
                }
            }

            xmlReader.Close();
        }

        public GingerRepositoryItem GetGingerRepositoryItem(eGingerRepoItemType itemType, string itemExternalID, List<string> indexerRecords = null)
        {
            GingerRepositoryItem repoItem = null;
            if (indexerRecords == null)
                indexerRecords = ReadRepositoryIndexerData();
            foreach (string indexerRecord in indexerRecords)
            {
                string[] indexerRecordDetails = indexerRecord.Split(';');
                if (indexerRecordDetails != null && indexerRecordDetails.Length == 6)
                {
                    string aTSExternalID = String.Empty;
                    if ((indexerRecordDetails.Length > 3) && (indexerRecordDetails[3] != null) && (indexerRecordDetails[3].ToString() != string.Empty))
                    {
                        if ((indexerRecordDetails[3].Contains('|')) && (indexerRecordDetails[3].Contains('=')))
                        {
                            aTSExternalID = indexerRecordDetails[3].Split('|').Last().Split('=').Last();
                        }
                        else
                        {
                            aTSExternalID = indexerRecordDetails[3];
                        }
                    }
                    if ((indexerRecordDetails[0] == GingerRepositoryItem.GetRepoItemTypeLabelInIndexer(itemType)) &&
                        (aTSExternalID == itemExternalID))
                    {
                        repoItem = new GingerRepositoryItem(itemType);
                        repoItem.Name = indexerRecordDetails[1];
                        repoItem.GUID = indexerRecordDetails[2];
                        repoItem.ExternalID = aTSExternalID;
                        repoItem.FilePath = indexerRecordDetails[4];
                        repoItem.LastUpdated = indexerRecordDetails[5];
                        break;
                    }
                }
            }

            return repoItem;
        }
    }
}
