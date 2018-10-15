#region License
/*
Copyright © 2014-2018 European Support Limited

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

using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GingerCore.Activities;
using System.Text.RegularExpressions;
using System.Web;
using GingerCore.Variables;
using System.Xml;
using RQM_Repository.Data_Contracts;
using GingerCore.ALM.RQM;
using RQM_Repository;
using ALM_Common.DataContracts;
using ALM_Common.Abstractions;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using Amdocs.Ginger.Repository;

namespace GingerCore.ALM.Rally
{
    /// <summary>
    /// </summary>
    public enum eRallyItemType { TestPlan, TestCase, TestScript }

    /// <summary>
    /// </summary>
    public static class ImportFromRally
    {
        public static ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo { get; set; }
        public static ObservableList<Activity> GingerActivitiesRepo { get; set; }

        public static int totalValues = 0;
        public static string populatedValue = string.Empty;

        public static BusinessFlow ConvertRallyTestPlanToBF(RallyTestPlan testPlan)
        {
            try
            {
                if (testPlan == null) return null;

                //Creat Business Flow
                BusinessFlow busFlow = new BusinessFlow();
                busFlow.Name = testPlan.Name;
                busFlow.ExternalID = "RallyID=" + testPlan.RallyID;
                busFlow.Status = BusinessFlow.eBusinessFlowStatus.Development;
                busFlow.Activities = new ObservableList<Activity>();
                busFlow.Variables = new ObservableList<VariableBase>();

                //Create Activities Group + Activities for each TC
                foreach (RallyTestCase tc in testPlan.TestCases)
                {
                    //check if the TC is already exist in repository
                    ActivitiesGroup tcActivsGroup;
                    ActivitiesGroup repoActivsGroup = null;
                    if (repoActivsGroup == null)
                        repoActivsGroup = GingerActivitiesGroupsRepo.Where(x => x.ExternalID != null ? x.ExternalID.Split('|').First().Split('=').Last() == tc.RallyID : false).FirstOrDefault();
                    if (repoActivsGroup != null)
                    {
                        tcActivsGroup = (ActivitiesGroup)repoActivsGroup.CreateInstance();
                        busFlow.AddActivitiesGroup(tcActivsGroup);
                        busFlow.ImportActivitiesGroupActivitiesFromRepository(tcActivsGroup, GingerActivitiesRepo, true, true);
                        busFlow.AttachActivitiesGroupsAndActivities();
                    }
                    else // TC not exist in Ginger repository so create new one
                    {
                        tcActivsGroup = new ActivitiesGroup();
                        tcActivsGroup.Name = tc.Name;
                        tcActivsGroup.Description = tc.Description;                        
                        tcActivsGroup.ExternalID = "RallyID=" + tc.RallyID + "|AtsID=" + tc.BTSID;
                        busFlow.AddActivitiesGroup(tcActivsGroup);
                    }

                    foreach (RallyTestStep step in tc.TestSteps)
                    {
                        Activity stepActivity;
                        bool toAddStepActivity = false;

                        // check if mapped activity exist in repository
                        Activity repoStepActivity = GingerActivitiesRepo.Where(x => x.ExternalID != null ? x.ExternalID.Split('|').First().Split('=').Last() == step.RallyIndex : false).FirstOrDefault();
                        if (repoStepActivity != null)
                        {
                            //check if it is part of the Activities Group
                            ActivityIdentifiers groupStepActivityIdent = tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.RallyIndex).FirstOrDefault();
                            if (groupStepActivityIdent != null)
                            {
                                //already in Activities Group so get link to it
                                stepActivity = busFlow.Activities.Where(x => x.Guid == groupStepActivityIdent.ActivityGuid).FirstOrDefault();
                            }
                            else // not in ActivitiesGroup so get instance from repo
                            {
                                stepActivity = (Activity)repoStepActivity.CreateInstance();
                                toAddStepActivity = true;
                            }
                        }
                        else //Step not exist in Ginger repository so create new one
                        {
                            string strBtsID = string.Empty;
                            stepActivity = new Activity();
                            stepActivity.ActivityName = tc.Name + ">" + step.Name;
                            stepActivity.ExternalID = "RallyID=" + step.RallyIndex + "|AtsID=" + strBtsID;
                            stepActivity.Description = StripHTML(step.Description);
                            stepActivity.Expected = StripHTML(step.ExpectedResult);

                            toAddStepActivity = true;
                        }

                        if (toAddStepActivity)
                        {
                            // not in group- need to add it
                            busFlow.AddActivity(stepActivity);
                            tcActivsGroup.AddActivityToGroup(stepActivity);
                        }

                        //pull TC-Step parameters and add them to the Activity level
                        foreach (RallyTestParameter param in tc.Parameters)   // Params taken from TestScriptLevel only!!!! Also exists parapameters at TestCase, to check if them should be taken!!!
                        {                                                               
                            bool? isflowControlParam = null;

                            //detrmine if the param is Flow Control Param or not based on it value and agreed sign "$$_"
                            if (param.Value.ToString().StartsWith("$$_"))
                            {
                                isflowControlParam = false;
                                if (param.Value.ToString().StartsWith("$$_"))
                                    param.Value = param.Value.ToString().Substring(3); //get value without "$$_"
                            }
                            else if (param.Value.ToString() != "<Empty>")
                                isflowControlParam = true;

                            //check if already exist param with that name
                            VariableBase stepActivityVar = stepActivity.Variables.Where(x => x.Name.ToUpper() == param.Name.ToUpper()).FirstOrDefault();
                            if (stepActivityVar == null)
                            {
                                //#Param not exist so add it
                                if (isflowControlParam == true)
                                {
                                    //add it as selection list param                               
                                    stepActivityVar = new VariableSelectionList();
                                    stepActivityVar.Name = param.Name;
                                    stepActivity.AddVariable(stepActivityVar);
                                    stepActivity.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset status because new flow control param was added
                                }
                                else
                                {
                                    //add as String param
                                    stepActivityVar = new VariableString();
                                    stepActivityVar.Name = param.Name;
                                    ((VariableString)stepActivityVar).InitialStringValue = param.Value;
                                    stepActivity.AddVariable(stepActivityVar);
                                }
                            }
                            else
                            {
                                //#param exist
                                if (isflowControlParam == true)
                                {
                                    if (!(stepActivityVar is VariableSelectionList))
                                    {
                                        //flow control param must be Selection List so transform it
                                        stepActivity.Variables.Remove(stepActivityVar);
                                        stepActivityVar = new VariableSelectionList();
                                        stepActivityVar.Name = param.Name;
                                        stepActivity.AddVariable(stepActivityVar);
                                        stepActivity.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset status because flow control param was added
                                    }
                                }
                                else if (isflowControlParam == false)
                                {
                                    if (stepActivityVar is VariableSelectionList)
                                    {
                                        //change it to be string variable
                                        stepActivity.Variables.Remove(stepActivityVar);
                                        stepActivityVar = new VariableString();
                                        stepActivityVar.Name = param.Name;
                                        ((VariableString)stepActivityVar).InitialStringValue = param.Value;
                                        stepActivity.AddVariable(stepActivityVar);
                                        stepActivity.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset status because flow control param was removed
                                    }
                                }
                            }

                            //add the variable selected value                          
                            if (stepActivityVar is VariableSelectionList)
                            {
                                OptionalValue stepActivityVarOptionalVar = ((VariableSelectionList)stepActivityVar).OptionalValuesList.Where(x => x.Value == param.Value).FirstOrDefault();
                                if (stepActivityVarOptionalVar == null)
                                {
                                    //no such variable value option so add it
                                    stepActivityVarOptionalVar = new OptionalValue(param.Value);
                                    ((VariableSelectionList)stepActivityVar).OptionalValuesList.Add(stepActivityVarOptionalVar);
                                    ((VariableSelectionList)stepActivityVar).SyncOptionalValuesListAndString();
                                    if (isflowControlParam == true)
                                        stepActivity.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset status because new param value was added
                                }
                                //set the selected value
                                ((VariableSelectionList)stepActivityVar).SelectedValue = stepActivityVarOptionalVar.Value;
                            }
                            else
                            {
                                //try just to set the value
                                try
                                {
                                    stepActivityVar.Value = param.Value;
                                    if (stepActivityVar is VariableString)
                                        ((VariableString)stepActivityVar).InitialStringValue = param.Value;
                                }
                                catch (Exception ex) { Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}"); }
                            }
                        }
                    }

                }

                return busFlow;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to import Rally test set and convert it into " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), ex);
                return null;
            }
        }

        private static void GetStepParameters(string stepText, ref List<string> stepParamsList)
        {
            try
            {
                MatchCollection stepParams = Regex.Matches(stepText, @"\<<<([^>]*)\>>>");

                foreach (var param in stepParams)
                {
                    string strParam = param.ToString().TrimStart(new char[] { '<' });
                    strParam = strParam.TrimEnd(new char[] { '>' });
                    stepParamsList.Add(strParam);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error occurred while pulling the parameters names from Rally TC Step Description/Expected", ex);
            }
        }

        public static string StripHTML(string HTMLText, bool toDecodeHTML = true)
        {
            try
            {
                HTMLText = HTMLText.Replace("<br />", Environment.NewLine);
                Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
                var stripped = reg.Replace(HTMLText, "");
                if (toDecodeHTML)
                    stripped = HttpUtility.HtmlDecode(stripped);

                stripped = stripped.TrimStart(new char[] { '\r', '\n' });
                stripped = stripped.TrimEnd(new char[] { '\r', '\n' });

                return stripped;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error occured while stripping the HTML from Rally TC Step Description/Expected", ex);
                return HTMLText;
            }
        }

        public static ObservableList<ExternalItemFieldBase> RefreshItemFields(eRQMItemType itemType, ObservableList<ExternalItemFieldBase> exitingFields, BackgroundWorker bw)
        {
            ObservableList<ExternalItemFieldBase> updatedFields = GetItemFields(itemType, bw);
            ObservableList<ExternalItemFieldBase> refreshedFields = new ObservableList<ExternalItemFieldBase>();


            foreach (ExternalItemFieldBase field in updatedFields)
            {
                if (exitingFields != null)
                {
                    ExternalItemFieldBase existingField = exitingFields.Where(x => x.ItemType == itemType.ToString() && x.ID == field.ID).FirstOrDefault();
                    if (existingField != null)
                    {
                        if (field.Mandatory == false)
                            field.ToUpdate = existingField.ToUpdate;

                        if (string.IsNullOrEmpty(existingField.SelectedValue) == false)
                        {
                            if (field.PossibleValues.Count > 0)
                            {
                                if (field.PossibleValues.Contains(existingField.SelectedValue))
                                    field.SelectedValue = existingField.SelectedValue;
                            }
                            else
                            {
                                field.SelectedValue = existingField.SelectedValue;
                            }
                        }
                    }
                }

                refreshedFields.Add(field);
            }

            //TODO: sort the fields by ItemType before returning
            ObservableList<ExternalItemFieldBase> sortedFields = refreshedFields;
            refreshedFields = new ObservableList<ExternalItemFieldBase>(from i in sortedFields orderby i.ItemType select i);
            return refreshedFields;
        }

        public static ObservableList<ExternalItemFieldBase> GetItemFields(eRQMItemType itemType, BackgroundWorker bw)
        {
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();
            //TODO : receive as parameters:
            RqmRepository rqmRep = new RqmRepository();
            List<IProjectDefinitions> rqmProjectsDataList;
            //string rqmSserverUrl = loginData.Server.ToString() + "/";
            string rqmSserverUrl = ALMCore.AlmConfig.ALMServerURL + "/";
            LoginDTO loginData = new LoginDTO() { User = ALMCore.AlmConfig.ALMUserName, Password = ALMCore.AlmConfig.ALMPassword, Server = ALMCore.AlmConfig.ALMServerURL };
            IProjectData rqmProjectsData = rqmRep.GetVisibleProjects(loginData);
            rqmProjectsDataList = rqmProjectsData.IProjectDefinitions;
            IProjectDefinitions currentProj = rqmProjectsDataList.FirstOrDefault();
            string rqmDomain = currentProj.Prefix;
            string rqmProject = currentProj.ProjectName;

            //------------------------------- Improved solution

            string baseUri_ = string.Empty;
            string selfLink_ = string.Empty;
            int maxPageNumber_ = 0;
            int totalCategoryTypeCount = 0;

            string categoryValue = string.Empty;  // --> itemfield.PossibleValues.Add(ccNode.Name);
            string categoryTypeID = string.Empty; //--> itemfield.ID

            //TODO: Populate list fields with CategoryTypes
            populatedValue = "Starting fields retrieve process... ";
            bw.ReportProgress(totalValues, populatedValue);
            RqmResponseData categoryType = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(rqmSserverUrl + rqmDomain + "/service/com.ibm.rqm.integration.service.IIntegrationService/resources/" + rqmProject + "/categoryType"));
            XmlDocument categoryTypeList = new XmlDocument();
            if (!string.IsNullOrEmpty(categoryType.responseText))
            {
                categoryTypeList.LoadXml(categoryType.responseText);
            }            

            //TODO: Get 'next' and 'last links
            XmlNodeList linkList_ = categoryTypeList.GetElementsByTagName("link");
            if (linkList_.Count > 0)
            {
                XmlNode selfPage = linkList_.Item(1);
                XmlNode lastPage_ = linkList_.Item(3);

                if (selfPage.Attributes["rel"].Value.ToString() == "self") //verify self link is present
                {
                    selfLink_ = selfPage.Attributes["href"].Value.ToString();
                    //baseUri_ = selfLink_.Substring(0, selfLink_.Length - 1);
                    baseUri_ = selfLink_;
                }

                if (lastPage_.Attributes["rel"].Value.ToString() == "last") //verify there is more than one page
                {
                    if (selfPage.Attributes["rel"].Value.ToString() == "self") //verify self link is present
                    {
                        selfLink_ = selfPage.Attributes["href"].Value.ToString();
                        baseUri_ = selfLink_.Substring(0, selfLink_.Length - 1);
                    }

                    string tempString_ = lastPage_.Attributes["href"].Value.ToString();
                    maxPageNumber_ = System.Convert.ToInt32(tempString_.Substring(tempString_.LastIndexOf('=') + 1));
                }
                string newUri_ = string.Empty;
                List<string> categoryTypeUriPages = new List<string>();
                //List<ExternalItemFieldBase> tempFieldList = new List<ExternalItemFieldBase>();
                ConcurrentBag<ExternalItemFieldBase> catTypeRsult = new ConcurrentBag<ExternalItemFieldBase>();

                for (int k = 0; k <= maxPageNumber_; k++)
                {
                    if (maxPageNumber_ > 0)
                    {
                        newUri_ = baseUri_ + k.ToString();
                        categoryTypeUriPages.Add(newUri_);
                    }
                    else
                    {
                        newUri_ = baseUri_;
                        categoryTypeUriPages.Add(newUri_);
                    }
                }

                //Parallel computing solution
                List<XmlNode> entryList = new List<XmlNode>();
                if (categoryTypeUriPages.Count > 1)
                {
                    Parallel.ForEach(categoryTypeUriPages.AsParallel(), categoryTypeUri =>
                    {
                        newUri_ = categoryTypeUri;
                        categoryType = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(newUri_));
                        if (!string.IsNullOrEmpty(categoryType.responseText))
                        {
                            categoryTypeList.LoadXml(categoryType.responseText);
                        }

                        //TODO: Get all ID links under entry:
                        XmlNodeList categoryTypeEntry_ = categoryTypeList.GetElementsByTagName("entry");

                        foreach (XmlNode entryNode in categoryTypeEntry_)
                        {
                            entryList.Add(entryNode);
                        }

                        ParallelLoopResult innerResult = Parallel.ForEach(entryList.AsParallel(), singleEntry =>
                        {
                            XmlNodeList innerNodes = singleEntry.ChildNodes;
                            XmlNode linkNode = innerNodes.Item(4);
                            ExternalItemFieldBase itemfield = new ExternalItemFieldBase();

                            string getIDlink = string.Empty;
                            getIDlink = linkNode.Attributes["href"].Value.ToString(); // retrived CategoryType link
                            
                            
                            RqmResponseData categoryTypeDetail = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(getIDlink));
                            //System.Diagnostics.Debug.WriteLine("entered loop 2");

                            XmlDocument categoryTypeListing = new XmlDocument();
                            if (!string.IsNullOrEmpty(categoryTypeDetail.responseText))
                            {
                                categoryTypeListing.LoadXml(categoryTypeDetail.responseText);
                            }


                            string categoryTypeName = string.Empty; // -->itemfield.Name
                            string categoryTypeItemType = string.Empty; //-->itemfield.ItemType
                            string categoryTypeMandatory = string.Empty; // --> itemfield.Mandatory & initial value for : --> itemfield.ToUpdate

                            string typeIdentifier = categoryTypeListing.GetElementsByTagName("ns3:identifier").Item(0).InnerText;
                            categoryTypeID = typeIdentifier.Substring(typeIdentifier.LastIndexOf(':') + 1); 
                            categoryTypeName = categoryTypeListing.GetElementsByTagName("ns3:title").Item(0).InnerText;
                            categoryTypeItemType = categoryTypeListing.GetElementsByTagName("ns2:scope").Item(0).InnerText;
                            categoryTypeMandatory = categoryTypeListing.GetElementsByTagName("ns2:required").Item(0).InnerText;


                            itemfield.ItemType = categoryTypeItemType;
                            itemfield.ID = categoryTypeID;
                            itemfield.Name = categoryTypeName;
                            if (itemfield.SelectedValue == null)
                            {
                                itemfield.SelectedValue = "NA";
                            }

                            if (categoryTypeMandatory == "true")
                            {
                                itemfield.ToUpdate = true;
                                itemfield.Mandatory = true;
                            }
                            else
                            {
                                itemfield.ToUpdate = false;
                                itemfield.Mandatory = false;
                            }

                            catTypeRsult.Add(itemfield);
                            populatedValue = "Populating field :" + categoryTypeName + " \r\nNumber of fields populated :" + catTypeRsult.Count;
                            bw.ReportProgress(catTypeRsult.Count, populatedValue);
                        });
                    });
                }
                else
                {
                    populatedValue = string.Empty;
                    newUri_ = baseUri_;
                    categoryType = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(newUri_));

                    if (!string.IsNullOrEmpty(categoryType.responseText))
                    {
                        categoryTypeList.LoadXml(categoryType.responseText);
                    }

                    //TODO: Get all ID links under entry:
                    XmlNodeList categoryTypeEntry_ = categoryTypeList.GetElementsByTagName("entry");

                    foreach (XmlNode entryNode in categoryTypeEntry_)
                    {
                        entryList.Add(entryNode);
                    }

                    ParallelLoopResult innerResult = Parallel.ForEach(entryList.AsParallel(), singleEntry =>
                    {
                        XmlNodeList innerNodes = singleEntry.ChildNodes;
                        XmlNode linkNode = innerNodes.Item(4);
                        ExternalItemFieldBase itemfield = new ExternalItemFieldBase();

                        string getIDlink = string.Empty;
                        getIDlink = linkNode.Attributes["href"].Value.ToString(); // retrived CategoryType link

                        RqmResponseData categoryTypeDetail = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(getIDlink));

                        XmlDocument categoryTypeListing = new XmlDocument();

                        if (!string.IsNullOrEmpty(categoryTypeDetail.responseText))
                        {
                            categoryTypeListing.LoadXml(categoryTypeDetail.responseText);
                        }
                        
                        string categoryTypeName = string.Empty; // -->itemfield.Name
                        string categoryTypeItemType = string.Empty; //-->itemfield.ItemType
                        string categoryTypeMandatory = string.Empty; // --> itemfield.Mandatory & initial value for : --> itemfield.ToUpdate

                        string typeIdentifier = categoryTypeListing.GetElementsByTagName("ns3:identifier").Item(0).InnerText;
                        categoryTypeID = typeIdentifier.Substring(typeIdentifier.LastIndexOf(':') + 1); 
                        categoryTypeName = categoryTypeListing.GetElementsByTagName("ns3:title").Item(0).InnerText;
                        categoryTypeItemType = categoryTypeListing.GetElementsByTagName("ns2:scope").Item(0).InnerText;
                        categoryTypeMandatory = categoryTypeListing.GetElementsByTagName("ns2:required").Item(0).InnerText;

                        itemfield.ItemType = categoryTypeItemType;
                        itemfield.ID = categoryTypeID;
                        itemfield.Name = categoryTypeName;
                        if (itemfield.SelectedValue == null)
                        {
                            itemfield.SelectedValue = "NA";
                        }

                        if (categoryTypeMandatory == "true")
                        {
                            itemfield.ToUpdate = true;
                            itemfield.Mandatory = true;
                        }
                        else
                        {
                            itemfield.ToUpdate = false;
                            itemfield.Mandatory = false;
                        }

                        catTypeRsult.Add(itemfield);
                        populatedValue = "Populating field :" + categoryTypeName +" \r\n Number of fields populated :" + catTypeRsult.Count;
                        bw.ReportProgress(catTypeRsult.Count, populatedValue);
                    });
                }
                
                foreach (ExternalItemFieldBase field in catTypeRsult)
                {
                    fields.Add(field);
                    totalCategoryTypeCount++;
                    System.Diagnostics.Debug.WriteLine("Number of retrieved fields:" + totalCategoryTypeCount);
                }

                //TODO: Add Values to CategoryTypes Parallel
                populatedValue = "Starting values retrieve process... ";
                bw.ReportProgress(totalValues, populatedValue);

                RqmResponseData category = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(rqmSserverUrl + rqmDomain + "/service/com.ibm.rqm.integration.service.IIntegrationService/resources/" + rqmProject + "/category"));
                XmlDocument CategoryList = new XmlDocument();
                CategoryList.LoadXml(category.responseText);
                totalValues = 0;
                populatedValue = string.Empty;

                //TODO: Get 'next' and 'last links
                XmlNodeList linkList = CategoryList.GetElementsByTagName("link");
                XmlNode selfPageNode = linkList.Item(1);
                XmlNode lastPageNode = linkList.Item(3);

                string selfLink = selfPageNode.Attributes["href"].Value.ToString();
                string baseUri = selfLink.Substring(0, selfLink.Length - 1);

                string tempString = lastPageNode.Attributes["href"].Value.ToString();
                int maxPageNumber = System.Convert.ToInt32(tempString.Substring(tempString.LastIndexOf('=') + 1));
                string newUri = string.Empty;
                List<string> categoryUriPages = new List<string>();

                for (int i = 0; i <= maxPageNumber; i++)
                //for (int i = 0; i <= 25; i++) //scale testing 
                {
                    if (maxPageNumber > 0)
                    {
                        newUri = baseUri + i.ToString();
                        categoryUriPages.Add(newUri);
                    }
                    else
                    {
                        newUri = baseUri;
                        categoryUriPages.Add(newUri);

                    }
                }

                List<ExternalItemFieldBase> valueFields = new List<ExternalItemFieldBase>();
                if (categoryUriPages.Count > 0)
                {
                    int iDCount = 0;

                    Parallel.ForEach(categoryUriPages.AsParallel(), singleUri =>
                    {
                        newUri = singleUri;

                        RqmResponseData category_ = new RqmResponseData();
                        category_ = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(newUri));

                        if (category_.ErrorCode == 401)
                        {
                            RQM.RQMConnect.Instance.RQMRep.ConnectToServer(loginData.Server.ToString(), loginData.User.ToString(), loginData.Password.ToString());
                            RQM.RQMConnect.Instance.RQMRep.ConnetProject(rqmDomain, rqmProject, loginData.User.ToString(), loginData.Password.ToString());
                            category_ = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(newUri));
                        }

                        XmlDocument CategoryList_ = new XmlDocument();
                        if (!string.IsNullOrEmpty(category_.responseText))
                        {
                            CategoryList_.LoadXml(category_.responseText);
                        }

                        XmlNodeList categoryIDs = CategoryList_.GetElementsByTagName("id");
                         iDCount += categoryIDs.Count;

                        if (categoryIDs.Count > 0)
                        {
                            List<string> idLinkList = new List<string>();
                            for (int n = 1; n < categoryIDs.Count; n++)
                                idLinkList.Add(categoryIDs.Item(n).InnerText);

                            Parallel.ForEach(idLinkList.AsParallel(), getIDlink =>
                            {
                                  ExternalItemFieldBase valuesItemfield = new ExternalItemFieldBase();

                                  if (!string.IsNullOrEmpty(getIDlink))
                                  {
                                      RqmResponseData categoryValueDetails = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(getIDlink)); // retrieve category page
                                      XmlDocument categoryValueXML = new XmlDocument();

                                      if (categoryValueDetails.ErrorCode == 401)
                                      {
                                          RQM.RQMConnect.Instance.RQMRep.ConnectToServer(loginData.Server.ToString(), loginData.User.ToString(), loginData.Password.ToString());
                                          RQM.RQMConnect.Instance.RQMRep.ConnetProject(rqmDomain, rqmProject, loginData.User.ToString(), loginData.Password.ToString());
                                          categoryValueDetails = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(getIDlink));
                                      }

                                      if (!string.IsNullOrEmpty(categoryValueDetails.responseText))
                                      {
                                          categoryValueXML.LoadXml(categoryValueDetails.responseText);
                                      }

                                      //categoryValueXML.LoadXml(categoryValueDetails.responseText.ToString());
                                      XmlNode categoryTypeNode;
                                      string catTypeLink = string.Empty;
                                    
                                    if (!string.IsNullOrEmpty(categoryValueXML.InnerText.ToString()))
                                    {
                                        categoryTypeNode = categoryValueXML.GetElementsByTagName("ns2:categoryType").Item(0); //need to consider changes in tag i.e. ns3/ns4...
                                        catTypeLink = categoryTypeNode.Attributes["href"].Value.ToString();

                                        categoryTypeID = catTypeLink.Substring(catTypeLink.LastIndexOf(':') + 1); 
                                        categoryValue = categoryValueXML.GetElementsByTagName("ns3:title").Item(0).InnerText;  

                                        valuesItemfield.ID = categoryTypeID;
                                        valuesItemfield.PossibleValues.Add(categoryValue);
                                        totalValues++;

                                        valueFields.Add(valuesItemfield);


                                        System.Diagnostics.Debug.WriteLine("Total number of populated values is :" + totalValues + "/" + iDCount * (categoryUriPages.Count + 1)); //TODO pass this to a string to print in the UI
                                        populatedValue = "Populating value:" + categoryValue + " \r\n Total Values:" + totalValues;
                                        bw.ReportProgress(totalValues, populatedValue);

                                    }
                                }
                             });

                            System.Diagnostics.Debug.WriteLine("inner parallel -->finished");
                        }
                    });
                    System.Diagnostics.Debug.WriteLine("outer parallel -->finished");
                }

                //TODO: insert loop to add value to field
                if (fields.Count > 0) //category list has at least 1 entry
                {
                    for (int j = 0; j < fields.Count; j++) //run through list
                    {
                        foreach (ExternalItemFieldBase values in valueFields)
                        {
                            if ((fields[j].ID.ToString() == values.ID.ToString()))
                            {
                                ExternalItemFieldBase addValueField = fields[j];
                                foreach (string possibleValue in values.PossibleValues)
                                {
                                    addValueField.PossibleValues.Add(possibleValue);
                                }
                                fields[j] = addValueField;
                                fields[j].SelectedValue = fields[j].PossibleValues[0];
                            }
                        }
                    }
                }
            }
            return fields;
        }

        public static XmlNodeList Readxmlfile(string fieldType, string solutionFolder)
        {
            XmlNodeList xlist = null;
            XmlDocument doc = new XmlDocument();
            if (System.IO.File.Exists(System.IO.Path.Combine(solutionFolder, @"Documents\ALM\RQM_Configs\FieldMapping.xml")))
            {
                doc.Load(System.IO.Path.Combine(solutionFolder, @"Documents\ALM\RQM_Configs\FieldMapping.xml"));

                if (fieldType == "TestPlan")
                {
                    xlist = doc.SelectNodes("//TestPlan/*");

                }
                if (fieldType == "TestCase")
                {
                    xlist = doc.SelectNodes("//TestCase/*");
                }
                if (fieldType == "TestScript")
                {
                    xlist = doc.SelectNodes("//TestScript/*");
                }

            }
            else
            {
                //TODO : build FieldMapping.xml
            }
            return xlist;
        }
    }
}
