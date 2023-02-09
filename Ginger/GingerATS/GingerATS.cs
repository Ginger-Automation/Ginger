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

namespace GingerATS
{
    /// <summary>
    /// Reflects the process end status
    /// </summary>
    public enum ResultStatus
    {
        /// <summary>
        /// The process ended successfully
        /// </summary>
        Success,
        /// <summary>
        /// Error occurred during the process
        /// </summary>
        Fail,
        /// <summary>
        /// Failed to connect to source control- process aborted
        /// </summary>
        SVNConnectionError,
        /// <summary>
        /// Failed to get/update the repository data from SVN
        /// </summary>
        SVNDataPullError,
        /// <summary>
        /// Failed to create/update the repository indexer
        /// </summary>
        RefreshRepositoryIndexerError,
    }

    public class GingerATS
    {
        /// <summary>
        /// The function supposed to be used for checking the automation status of QC Test Cases/s in a specific Ginger Solution
        /// </summary>
        /// <param name="logsFolderPath">Path of ATS logs folder to create/update the GingerATSLog file with function run information</param>
        /// <param name="sourceConnectionDetails">Connection details for the Ginger Source Control Solution</param>
        /// <param name="testCases">List of QC Test Cases to check their automation status</param>        
        public static ResultStatus GetAutomationStatus(string logsFolderPath, SourceConnectionDetails sourceConnectionDetails,
                                                                                ref Dictionary<long, TestCaseAutoStatus> testCases)
        {
            GingerATSLog Logger = null;
            string solutionFullLocalPath = string.Empty;
            List<string> IndexerRecords = null;
            try
            {
                //Logger creation
                Logger = new GingerATSLog(logsFolderPath);
                Logger.AddLineToLog(eLogLineType.INFO, "#######################  UPDATED GingerATS version for new XMLs format ########################");
                Logger.AddLineToLog(eLogLineType.INFO, "####################### GetAutomationStatus Function Called for Solution: '" + sourceConnectionDetails.GingerSolutionName + "'  ########################");

                //Update local repository with solution data from source control
                Logger.AddLineToLog(eLogLineType.INFO, "#####>>>>> Get/Update Solution Shared Repository Data from SVN:");
                if (sourceConnectionDetails != null)
                {
                    Logger.AddLineToLog(eLogLineType.INFO, "Input Details:" + Environment.NewLine +
                                                            "SourceURL= " + sourceConnectionDetails.SourceURL + Environment.NewLine +
                                                            "SourceUserName= " + sourceConnectionDetails.SourceUserName + Environment.NewLine +
                                                            "SourceUserPass= " + sourceConnectionDetails.SourceUserPass + Environment.NewLine +
                                                            "GingerSolutionName= " + sourceConnectionDetails.GingerSolutionName + Environment.NewLine +
                                                            "GingerSolutionLocalPath= " + sourceConnectionDetails.GingerSolutionLocalPath);

                    solutionFullLocalPath = Path.Combine(sourceConnectionDetails.GingerSolutionLocalPath, sourceConnectionDetails.GingerSolutionName);
                    try
                    {
                        GingerATSSVNSourceControl sourceControl = new GingerATSSVNSourceControl(sourceConnectionDetails, solutionFullLocalPath);
                        sourceControl.Logger = Logger;

                        //Check if require to get the full solution data or only update it
                        if (!Directory.Exists(solutionFullLocalPath))
                        {
                            //get full solution
                            Logger.AddLineToLog(eLogLineType.INFO, "Getting all Solution shared repository data from SVN into: '" + solutionFullLocalPath + "'");
                            sourceControl.GetProject();
                        }
                        else
                        {
                            //update solution
                            Logger.AddLineToLog(eLogLineType.INFO, "Updating the Solution shared repository data from SVN into: '" + solutionFullLocalPath + "'");
                            sourceControl.GetLatest();
                        }

                        if (sourceControl.OperationResult != null && sourceControl.OperationResult.Revision != -1)
                        {
                            Logger.AddLineToLog(eLogLineType.INFO, "SVN Result: Local copy was updated to Revision number: " + sourceControl.OperationResult.Revision.ToString());
                        }
                        else
                            Logger.AddLineToLog(eLogLineType.WARNING, "SVN Result: No result value returned, probably error occurred.");

                    }
                    catch (Exception ex)
                    {
                        Logger.AddLineToLog(eLogLineType.ERROR, "Error occurred while trying to get/update the data from SVN source", ex);
                        Logger.WriteToLogFile();
                        return ResultStatus.SVNDataPullError;
                    }
                }
                else
                {
                    Logger.AddLineToLog(eLogLineType.ERROR, "The provided SVN connection details object is NULL");
                    Logger.WriteToLogFile();
                    return ResultStatus.SVNConnectionError;
                }

                //Create or Update the Solution Repository Indexer
                Logger.AddLineToLog(eLogLineType.INFO, "#####>>>>> Create/Update the Solution Repository Indexer:");
                GingerATSRepositoryIndexer RepositoryIndexer = null;
                try
                {
                    RepositoryIndexer = new GingerATSRepositoryIndexer(solutionFullLocalPath);
                    if (!RepositoryIndexer.IsIndexerExist)
                    {
                        Logger.AddLineToLog(eLogLineType.INFO, "Creating the Repository Indexer in path: '" + RepositoryIndexer.IndexerPath + "'");
                        RepositoryIndexer.CreateRepositoryIndexer();
                    }
                    else
                    {
                        Logger.AddLineToLog(eLogLineType.INFO, "Updating the Repository Indexer which in path: '" + RepositoryIndexer.IndexerPath + "'");
                        RepositoryIndexer.UpdateRepositoryIndexer();
                    }

                    IndexerRecords = RepositoryIndexer.ReadRepositoryIndexerData();
                }
                catch (Exception ex)
                {
                    Logger.AddLineToLog(eLogLineType.ERROR, "Error occurred while creating/refreshing the Solution Repository Indexer", ex);
                    Logger.WriteToLogFile();
                    return ResultStatus.RefreshRepositoryIndexerError;
                }

                //Check the automation status of each TC in the provided list by ATS   
                Logger.AddLineToLog(eLogLineType.INFO, "#####>>>>> Check the automation status of each TC in the provided list by ATS:");
                if (testCases != null)
                {
                    int tcsCounter = 0;
                    foreach (TestCaseAutoStatus tc in testCases.Values)
                    {
                        tcsCounter++;
                        if (tc == null) continue;
                        Logger.AddLineToLog(eLogLineType.INFO, "Checking the automation status of TC ID: " + tc.ID + " (" + tcsCounter + "/" + testCases.Count + ")");
                        //Search for the TC in the Activities Group reposiotry
                        GingerRepositoryItem tc_ActivitiesGroup = null;
                        try
                        {
                            tc_ActivitiesGroup = RepositoryIndexer.GetGingerRepositoryItem(eGingerRepoItemType.ActivitiesGroup, tc.ID.ToString(), IndexerRecords);
                        }
                        catch (Exception ex)
                        {
                            Logger.AddLineToLog(eLogLineType.ERROR, "Error Occurred while trying to get the repository details for TC (Activities Group) ID: " + tc.ID, ex);
                        }
                        if (tc_ActivitiesGroup != null)
                        {
                            //Activities Group xml was found
                            tc.KnownByGinger = true;

                            //Check for each TC step if it was automated for the specific parameter value
                            if (tc.Steps != null)
                            {
                                foreach (StepAutoStatus step in tc.Steps.Values)
                                {
                                    //Search for the TC Step (Activity) in the Activity's reposiotry
                                    GingerRepositoryItem step_Activity = null;
                                    try
                                    {
                                        step_Activity = RepositoryIndexer.GetGingerRepositoryItem(eGingerRepoItemType.Activity, step.ID.ToString(), IndexerRecords);
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.AddLineToLog(eLogLineType.ERROR, "Error Occurred while trying to get the repository details for TC_Step (Activity) ID: " + step.ID, ex);
                                    }
                                    if (step_Activity != null)
                                    {
                                        //Check if the step/activity was automated
                                        string activityAutomationStatus =
                                            GingerATSXmlReader.GetNodeAttributeValue(step_Activity.FilePath,
                                                                                     GingerRepositoryItem.GetRepoItemMainXmlNodeName(step_Activity.Type),
                                                                                     GingerRepositoryItem.ActivityAutomationStatusAttributeName);
                                        if (activityAutomationStatus != null && activityAutomationStatus == GingerRepositoryItem.ActivityAutomationStatusAutomatedValue)
                                        {
                                            //Step/activity was automated
                                            step.AutomatedByGinger = true;

                                            //Check for each step param if the selected value is supported and automated
                                            if (step.Parameters != null)
                                            {
                                                //pull the variables section from XML- workaround because sometimes (when the Activity - Actions includes Variables Dependencies) the XmlReader is fails to get to the Variables node from some reason                                                
                                                string step_Activity_VariablesXml = GingerATSXmlReader.GetXmlFileNodeContent(step_Activity.FilePath, GingerRepositoryItem.ActivityVariablesAttributeName);

                                                foreach (StepParamAutoStatus param in step.Parameters.Values)
                                                {
                                                    if (param.SelectedValue == null || param.SelectedValue == string.Empty) //was added to support empty value as an option
                                                        param.SelectedValue = "<Empty>";

                                                    //check if the variable is known to Ginger
                                                    List<NodeAttributeValidation> variableExistsAttributes = new List<NodeAttributeValidation>();
                                                    variableExistsAttributes.Add(new NodeAttributeValidation(GingerRepositoryItem.ActivityVariableNameAttribute, param.Name));
                                                    if (GingerATSXmlReader.ValidateNodeAttributsValue(step_Activity_VariablesXml,
                                                                                                       GingerRepositoryItem.ActivityVariablesAttributeName, variableExistsAttributes, true))
                                                    {
                                                        //Variable Exist
                                                        //check if it from typeSelection List
                                                        List<NodeAttributeValidation> variableIsSelectionListAttributes = new List<NodeAttributeValidation>();
                                                        variableIsSelectionListAttributes.Add(new NodeAttributeValidation(GingerRepositoryItem.VariableSelectionListNameAttribute, param.Name));
                                                        if (GingerATSXmlReader.ValidateNodeAttributsValue(step_Activity_VariablesXml,
                                                                                                           GingerRepositoryItem.VariableSelectionListNodeName, variableIsSelectionListAttributes, true))
                                                        {
                                                            //Selection List
                                                            //check if the parameter value is supported
                                                            List<NodeAttributeValidation> variableSelectionListValueAttributes = new List<NodeAttributeValidation>();
                                                            variableSelectionListValueAttributes.Add(new NodeAttributeValidation(GingerRepositoryItem.VariableSelectionListNameAttribute, param.Name));
                                                            variableSelectionListValueAttributes.Add(new NodeAttributeValidation(GingerRepositoryItem.VariableSelectionListOptionalValuesAttribute,
                                                                                                                    param.SelectedValue, NodeAttributeValidation.eNodeAttributeValidationType.NewLineSeperatedListValue, true));
                                                            if (GingerATSXmlReader.ValidateNodeAttributsValue(step_Activity_VariablesXml,
                                                                                                               GingerRepositoryItem.VariableSelectionListNodeName, variableSelectionListValueAttributes, true))
                                                            {
                                                                //supported
                                                                param.AutomatedByGinger = true;
                                                            }
                                                            else
                                                            {
                                                                //not supported
                                                                param.AutomatedByGinger = false;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            //Not Selection List
                                                            param.AutomatedByGinger = true;//supported no matter what is the value
                                                        }
                                                    }
                                                    else
                                                    {
                                                        //Variable Not Exist
                                                        param.AutomatedByGinger = false;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //Step/activity was not automated
                                            step.AutomatedByGinger = false;
                                        }
                                    }
                                    else
                                    {
                                        //Tc step was not found in the Activity's reposiotry- so it was not automated   
                                        step.AutomatedByGinger = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            //tc was not found in the Activities Group reposiotry- it was not automated   
                            tc.KnownByGinger = false;
                        }
                    }
                }
                else
                {
                    Logger.AddLineToLog(eLogLineType.ERROR, "The provided Test Cases list object is NULL");
                    Logger.WriteToLogFile();
                    return ResultStatus.Fail;
                }

                Logger.AddLineToLog(eLogLineType.INFO, "####################### Checking the Automation status ended successfully.");
                Logger.WriteToLogFile();
                return ResultStatus.Success;
            }
            catch (Exception ex)
            {
                Logger.AddLineToLog(eLogLineType.ERROR, "Error occurred while checking the automation status of provided Test Cases", ex);
                Logger.WriteToLogFile();
                return ResultStatus.Fail;
            }
        }
    }
}