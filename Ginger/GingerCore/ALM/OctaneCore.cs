#region License
/*
Copyright © 2014-2020 European Support Limited

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

using ALM_Common.DataContracts;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Activities;
using GingerCore.Variables;
using OctaneSDK.Connector;
using OctaneSDK.Connector.Authentication;
using OctaneSDK.Connector.Credentials;
using OctaneSDK.Services;
using OctaneSDK.Services.RequestContext;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octane_Repository;
using ALM_Common.Data_Contracts;
using Octane_Repository.BLL;
using OctaneSDK.Entities.Base;
using OctaneSDK.Entities.WorkItems;
using OctaneSDK.Entities.Tests;
using OctaneSDK.Entities.Requirements;
using GingerCore.ALM.QC;
using OctaneSDK.Services.Queries;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Web;
using QCRestClient;
using QCTestSet = QCRestClient.QCTestSet;
using Couchbase.Utils;
using Amdocs.Ginger.Common.InterfacesLib;
using System.IO;
using System.IO.Compression;
using OctaneSDK.Entities.Releases;
using amdocs.ginger.GingerCoreNET;

namespace GingerCore.ALM
{
    public class OctaneCore : ALMCore
    {
        public override ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo { get; set; }
        public override ObservableList<Activity> GingerActivitiesRepo { get; set; }
        public ProjectArea ProjectArea { get; private set; }
        List<Release> releases;
        public RestConnector mOctaneRestConnector;
        public EntityService entityService;
        protected LwssoAuthenticationStrategy lwssoAuthenticationStrategy;
        protected WorkspaceContext workspaceContext;
        protected SharedSpaceContext sharedSpaceContext;
        protected OctaneRepository octaneRepository;
        private LoginDTO loginDto;
        private static Dictionary<string, string> ExploredApplicationModule = new Dictionary<string, string>();

        public OctaneCore()
        {
            octaneRepository = new OctaneRepository();
        }
        public override bool ConnectALMProject()
        {
            return this.ConnectALMServer();
        }

        public List<string> GetTestLabExplorer(string path)
        {

            List<string> testlabPathList = new List<string>();
            try
            {
                string[] separatePath = path.Split('\\');
                separatePath[0] = ExploredApplicationModule.ContainsKey("Application Modules") ? ExploredApplicationModule["Application Modules"] : GetRootFolderId();

                if (!ExploredApplicationModule.ContainsKey("Application Modules"))
                {
                    ExploredApplicationModule.Add("Application Modules", separatePath[0]);
                }

                for (int i = 1; i < separatePath.Length; i++)
                {
                    separatePath[i] = GetTestLabFolderId(separatePath[i], separatePath[i - 1]);
                }

                LogicalQueryPhrase parent = new LogicalQueryPhrase("id", separatePath[separatePath.Length - 1], ComparisonOperator.Equal);
                CrossQueryPhrase qdParent = new CrossQueryPhrase("parent", parent);
                List<IQueryPhrase> filter = new List<IQueryPhrase>() { qdParent };

                return Task.Run(() =>
                {
                    return octaneRepository.GetEntities<ApplicationModule>(GetLoginDTO(), filter);
                }).Result.Select(d => d.Name).ToList();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get Test Lab with REST API", ex);
            }
            return testlabPathList;
        }

        private string GetRootFolderId()
        {
            return Task.Run(() =>
            {
                LogicalQueryPhrase f1 = new LogicalQueryPhrase("name", "Application Modules", ComparisonOperator.Equal);
                LogicalQueryPhrase f2 = new LogicalQueryPhrase("path", "000000", ComparisonOperator.Equal);
                return octaneRepository.GetEntities<ApplicationModule>(GetLoginDTO(), new List<IQueryPhrase>() { f1, f2 });
            }).Result.FirstOrDefault().Id.ToString();
        }

        private string GetTestLabFolderId(string separateAti, string separateAtIMinusOne)
        {
            if (!ExploredApplicationModule.ContainsKey(separateAti))
            {
                LogicalQueryPhrase parent = new LogicalQueryPhrase("id", separateAtIMinusOne, ComparisonOperator.Equal);
                CrossQueryPhrase qdParent = new CrossQueryPhrase("parent", parent);
                LogicalQueryPhrase lp = new LogicalQueryPhrase("name", separateAti, ComparisonOperator.Equal);
                List<IQueryPhrase> filter = new List<IQueryPhrase>() { qdParent, lp };
                List<ApplicationModule> listnodes = Task.Run(() =>
                {
                    return octaneRepository.GetEntities<ApplicationModule>(GetLoginDTO(), filter);
                }).Result;

                ExploredApplicationModule.Add(listnodes.FirstOrDefault().Name, listnodes.FirstOrDefault().Id);
                return listnodes.FirstOrDefault().Id;
            }
            else
            {
                return ExploredApplicationModule[separateAti];
            }
        }

        private LoginDTO GetLoginDTO()
        {
            if (this.loginDto == null)
            {
                AlmResponseWithData<AlmDomainColl> domains = Task.Run(() =>
                {
                    return octaneRepository.GetLoginProjects(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL);
                }).Result;
                AlmDomain domain = domains.DataResult.Where(f => f.DomainName.Equals(ALMCore.DefaultAlmConfig.ALMDomain)).FirstOrDefault();
                ProjectArea project = domain.Projects.Where(p => p.ProjectName.Equals(ALMCore.DefaultAlmConfig.ALMProjectName)).FirstOrDefault();
                this.loginDto = new LoginDTO()
                {
                    User = ALMCore.DefaultAlmConfig.ALMUserName,
                    Password = ALMCore.DefaultAlmConfig.ALMPassword,
                    Server = ALMCore.DefaultAlmConfig.ALMServerURL,
                    SharedSpaceId = domain.DomainId,
                    WorkSpaceId = project.ProjectId
                };
            }
            return this.loginDto;
        }

        public List<QCTestSetSummary> GetTestSetExplorer(string PathNode)
        {
            List<QCTestSetSummary> testlabPathList = new List<QCTestSetSummary>();
            string[] separatePath = PathNode.Split('\\');
            try
            {
                separatePath[0] = ExploredApplicationModule.ContainsKey("Application Modules") ? ExploredApplicationModule["Application Modules"] : GetRootFolderId();

                if (!ExploredApplicationModule.ContainsKey("Application Modules"))
                {
                    ExploredApplicationModule.Add("Application Modules", separatePath[0]);
                }

                for (int i = 1; i < separatePath.Length; i++)
                {
                    separatePath[i] = GetTestLabFolderId(separatePath[i], separatePath[i - 1]);
                }

                List<TestSuite> testSets = Task.Run(() =>
                {
                    LogicalQueryPhrase test_Suite = new LogicalQueryPhrase("id", separatePath[separatePath.Length - 1], ComparisonOperator.Equal);
                    CrossQueryPhrase qd = new CrossQueryPhrase("product_areas", test_Suite);
                    return octaneRepository.GetEntities<TestSuite>(GetLoginDTO(), new List<IQueryPhrase>() { qd });
                }).Result;

                foreach (TestSuite testset in testSets)
                {
                    QCTestSetSummary QCTestSetTreeItem = new QCTestSetSummary();
                    QCTestSetTreeItem.TestSetID = testset.Id;
                    QCTestSetTreeItem.TestSetName = testset.Name;
                    testlabPathList.Add(QCTestSetTreeItem);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get Test Set with REST API", ex);
            }
            return testlabPathList;
        }

        public override bool ConnectALMServer()
        {
            try
            {
                if (octaneRepository == null)
                {
                    octaneRepository = new OctaneRepository();
                }
                Reporter.ToLog(eLogLevel.DEBUG, "Connecting to Octane server");
                return Task.Run(() =>
                    {
                        return octaneRepository.IsLoginValid(
                            new LoginDTO()
                            {
                                User = ALMCore.DefaultAlmConfig.ALMUserName,
                                Password = ALMCore.DefaultAlmConfig.ALMPassword,
                                Server = ALMCore.DefaultAlmConfig.ALMServerURL
                            });
                    }).Result;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Connecting to Octane server", ex);
                mOctaneRestConnector = null;
                return false;
            }
        }

        public override Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields, bool useREST = false)
        {
            Dictionary<Guid, string> defectsOpeningResults = new Dictionary<Guid, string>();
            foreach (KeyValuePair<Guid, Dictionary<string, string>> defectForOpening in defectsForOpening)
            {
                Dictionary<string, string> filedsToUpdate = new Dictionary<string, string>();

                foreach (var item in defectsFields.Where(a => a.Mandatory || a.ToUpdate))
                {
                    if (string.IsNullOrEmpty(item.SelectedValue)|| item.SelectedValue=="Unassigned")
                    {
                        item.SelectedValue= defectForOpening.Value.ContainsKey(item.ExternalID) && defectForOpening.Value[item.ExternalID]!= "Unassigned" ? defectForOpening.Value[item.ExternalID] : string.Empty;
                    }
                    filedsToUpdate.Add(item.ExternalID, item.SelectedValue);
                }

                //TODO: ToUpdate field is not set to true correctly on fields grid. 
                // So description is not captured. Setting it explicitly until grid finding is fixed
                filedsToUpdate.Add("severity", defectForOpening.Value.ContainsKey("severity") ? defectForOpening.Value["severity"] : string.Empty);
                filedsToUpdate.Add("description", defectForOpening.Value.ContainsKey("description") ? defectForOpening.Value["description"] : string.Empty);

                string newDefectID = Task.Run(() =>
                {
                    return octaneRepository.CreateDefect(GetLoginDTO(), filedsToUpdate);
                    
                }).Result;
                defectsOpeningResults.Add(defectForOpening.Key, newDefectID);


            }

            return defectsOpeningResults;
        }

        public override bool DisconnectALMProjectStayLoggedIn()
        {
            DisconnectALMServer();
            ConnectALMServer();
            return true;
        }

        public override void DisconnectALMServer()
        {
            var result = Task.Run(() =>
            {
                this.octaneRepository.DisconnectProject();
                return true;
            }).Result;
        }

        public override bool ExportExecutionDetailsToALM(BusinessFlow bizFlow, ref string result, bool exectutedFromAutomateTab = false, PublishToALMConfig publishToALMConfig = null)
        {
            result = string.Empty;
            ObservableList<ExternalItemFieldBase> runFields = new ObservableList<ExternalItemFieldBase>(WorkSpace.Instance.Solution.ExternalItemsFields);
            runFields = new ObservableList<ExternalItemFieldBase>(runFields.Where(f => f.ItemType.Equals("Run")).ToList());
            if (bizFlow.ExternalID == "0" || String.IsNullOrEmpty(bizFlow.ExternalID))
            {
                result = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ": " + bizFlow.Name + " is missing ExternalID, cannot locate QC TestSet without External ID";
                return false;
            }
            try
            {
                if ((publishToALMConfig.VariableForTCRunName == null) || (publishToALMConfig.VariableForTCRunName == string.Empty))
                {
                    publishToALMConfig.VariableForTCRunName = "GingerRun_" + DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss");
                }
                //get the BF matching test set
                QCTestSet testSet = this.GetTestSuiteById(bizFlow.ExternalID);//bf.externalID holds the TestSet TSTests collection id
                if (testSet != null)
                {
                    //get the Test set TC's
                    QCTestInstanceColl qcTSTests = this.GetTestsFromTSId(testSet.Id); //list of TSTest's on main TestSet in TestLab 

                    //get all BF Activities groups
                    ObservableList<ActivitiesGroup> activGroups = bizFlow.ActivitiesGroups;
                    if (activGroups.Count > 0)
                    {
                        RunSuite runSuite = CreateRunSuite(publishToALMConfig, bizFlow, testSet, runFields);
                        foreach (ActivitiesGroup activGroup in activGroups)
                        {
                            if ((publishToALMConfig.FilterStatus == FilterByStatus.OnlyPassed && activGroup.RunStatus == eActivitiesGroupRunStatus.Passed)
                            || (publishToALMConfig.FilterStatus == FilterByStatus.OnlyFailed && activGroup.RunStatus == eActivitiesGroupRunStatus.Failed)
                            || publishToALMConfig.FilterStatus == FilterByStatus.All)
                            {
                                QCTestInstance tsTest = null;
                                //go by TC ID = TC Instances ID
                                tsTest = qcTSTests.Find(x => x.TestId == activGroup.ExternalID && x.Id == activGroup.ExternalID2);

                                if (tsTest == null)
                                {
                                    //go by Linked TC ID + TC Instances ID
                                    tsTest = qcTSTests.Find(x => x.Id == activGroup.ExternalID && x.TestId == activGroup.ExternalID2);
                                }
                                if (tsTest == null)
                                {
                                    //go by TC ID 
                                    tsTest = qcTSTests.Find(x => x.TestId == activGroup.ExternalID);
                                }
                                if (tsTest != null)
                                {
                                    //get activities in group
                                    List<Activity> activities = (bizFlow.Activities.Where(x => x.ActivitiesGroupID == activGroup.Name)).Select(a => a).ToList();                                    

                                    //Commented below create test run as Above create test suite function creates test runs by default.
                                    //CrateTestRun(publishToALMConfig, activGroup, tsTest, runSuite.Id, runFields);

                                    // Attach ActivityGroup Report if needed
                                    if (publishToALMConfig.ToAttachActivitiesGroupReport)
                                    {
                                        if ((activGroup.TempReportFolder != null) && (activGroup.TempReportFolder != string.Empty) &&
                                            (System.IO.Directory.Exists(activGroup.TempReportFolder)))
                                        {
                                            //Creating the Zip file - start
                                            string targetZipPath = System.IO.Directory.GetParent(activGroup.TempReportFolder).ToString();
                                            string zipFileName = targetZipPath + "\\" + activGroup.Name.ToString() + "_GingerHTMLReport.zip";

                                            if (!System.IO.File.Exists(zipFileName))
                                            {
                                                ZipFile.CreateFromDirectory(activGroup.TempReportFolder, zipFileName);
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(zipFileName);
                                                ZipFile.CreateFromDirectory(activGroup.TempReportFolder, zipFileName);
                                            }
                                            System.IO.Directory.Delete(activGroup.TempReportFolder, true);
                                            //Creating the Zip file - finish

                                            if (!this.AddAttachment(testSet.Id, zipFileName))
                                            {
                                                result = "Failed to create attachment";
                                                return false;
                                            }

                                            System.IO.File.Delete(zipFileName);
                                        }
                                    }
                                }
                                else
                                {
                                    //No matching TC was found for the ActivitiesGroup in QC
                                    result = "Matching TC's were not found for all " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " in QC/ALM.";
                                }
                            }
                            if (result != string.Empty)
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        //No matching Test Set was found for the BF in QC
                        result = "No matching Test Set was found in QC/ALM.";
                    }

                    if (result == string.Empty)
                    {
                        result = "Export performed successfully.";
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                result = "Unexpected error occurred- " + ex.Message;
                if (ex.InnerException != null && ex.InnerException.InnerException != null)
                {
                    result += ex.InnerException.InnerException.Message;
                }
                Reporter.ToLog(eLogLevel.ERROR, "Failed to export execution details to QC/ALM", ex);
                return false;
            }

            return false;
        }

        private bool AddAttachment(string testSuiteId, string zipFileName)
        {
            try
            {                
                FileStream fs = new FileStream(zipFileName, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);
                byte[] fileData = br.ReadBytes((Int32)fs.Length);
                var tt = Task.Run(() => { return entityService.AttachToEntity(new WorkspaceContext(this.loginDto.SharedSpaceId, this.loginDto.WorkSpaceId), new TestSuite() { Id = new EntityId(testSuiteId) }, zipFileName.Split(Path.DirectorySeparatorChar).Last(), fileData, "text/zip", null); }).Result;
                fs.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private RunSuite CreateRunSuite(PublishToALMConfig publishToALMConfig, BusinessFlow bizFlow, QCTestSet testSet, ObservableList<ExternalItemFieldBase> runFields)
        {
            try
            {
                RunSuite runSuiteToExport = new RunSuite();
                runSuiteToExport.SetValue("subtype", "run_manual");
                runSuiteToExport.Name = testSet.Name;
                runSuiteToExport.SetValue("test", new BaseEntity()
                {
                    TypeName = "test_suite",
                    Id = testSet.Id,
                });
                runSuiteToExport.SetValue("native_status", new BaseEntity()
                {
                    TypeName = "list_node",
                    Id = "list_node.run_native_status." + bizFlow.RunStatus,
                });
                AddEntityFieldValues(runFields, runSuiteToExport, "test_suite");
                runSuiteToExport.SetValue("description", publishToALMConfig.VariableForTCRunName);
                return Task.Run(() => 
                { 
                    return this.octaneRepository.CreateEntity<RunSuite>(GetLoginDTO(), runSuiteToExport, null); 
                }).Result;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "In CreateRunSuite/OctaneCore.cs method ", ex);
                throw;
            }
        }

        private Run CrateTestRun(PublishToALMConfig publishToALMConfig, ActivitiesGroup activGroup, QCTestInstance tsTest, EntityId runSuiteId, ObservableList<ExternalItemFieldBase> runFields)
        {
            Run runToExport = new Run();

            runToExport.Name = publishToALMConfig.VariableForTCRunNameCalculated;
            runToExport.SetValue("subtype", "run_manual");
            runToExport.SetValue("parent_suite", new BaseEntity()
            {
                TypeName = "run_suite",
                Id = runSuiteId
            });
            runToExport.SetValue("subtype", "run_manual");
            AddEntityFieldValues(runFields, runToExport, "run_manual");
            //runToExport.SetValue("release", new BaseEntity()
            //{
            //    TypeName = "release",
            //    Id = GetReleases().FirstOrDefault().Id
            //});
            runToExport.SetValue("test", new BaseEntity()
            {
                TypeName = "test_manual",
                Id = tsTest.Id
            });
            runToExport.SetValue("native_status", new BaseEntity()
            {
                TypeName = "list_node",
                Id = "list_node.run_native_status." + activGroup.RunStatus,
            });

            return Task.Run(() => {
                return this.octaneRepository.CreateEntity<Run>(GetLoginDTO(), runToExport, null); }).Result;
        }

        public override Dictionary<string, string> GetALMDomainProjects(string ALMDomainName)
        {
            AlmResponseWithData<AlmDomainColl> domains = Task.Run(() =>
            {
                return octaneRepository.GetLoginProjects(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL);
            }).Result;
            return domains.DataResult.Where(f => f.DomainName.Equals(ALMDomainName)).FirstOrDefault().Projects.ToDictionary(project => project.ProjectName, project => project.ProjectName);
        }

        public override List<string> GetALMDomains()
        {
            AlmResponseWithData<AlmDomainColl> domains = Task.Run(() =>
            {
                return octaneRepository.GetLoginProjects(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL);
            }).Result;

            return domains.DataResult.Select(f => f.DomainName).ToList();
        }

        public override ObservableList<ExternalItemFieldBase> GetALMItemFields(BackgroundWorker bw, bool online, ResourceType resourceType = ResourceType.ALL)
        {
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();
            string resourse = string.Empty;
            LoginDTO _loginDto = GetLoginDTO();
            Dictionary<string, List<string>> listnodes = Task.Run(() =>
            {
                return octaneRepository.GetListNodes(_loginDto);
            }).Result;

            if (resourceType == ALM_Common.DataContracts.ResourceType.ALL)
            {
                resourse = Octane_Repository.BLL.Extensions.ConvertResourceType(ResourceType.TEST_CASE);
                ExtractFields(fields, resourse, "Test Case", _loginDto, listnodes);

                resourse = Octane_Repository.BLL.Extensions.ConvertResourceType(ResourceType.TEST_SET);
                ExtractFields(fields, resourse, "Test Suite", _loginDto, listnodes);

                resourse = Octane_Repository.BLL.Extensions.ConvertResourceType(ResourceType.REQUIREMENT);
                ExtractFields(fields, resourse, "Requirement", _loginDto, listnodes);

                resourse = Octane_Repository.BLL.Extensions.ConvertResourceType(ResourceType.TEST_RUN);
                ExtractFields(fields, resourse, "Run", _loginDto, listnodes);
            }
            else
            {

                resourse = Octane_Repository.BLL.Extensions.ConvertResourceType(resourceType);

                ExtractFields(fields, resourse, resourse, _loginDto, listnodes);
            }
            return fields;
        }

        private void ExtractFields(ObservableList<ExternalItemFieldBase> fields, string resourse, string resource2, LoginDTO loginDto, Dictionary<string, List<string>> listnodes)
        {
            fields.Append(AddFieldsValues(Task.Run(() =>
            {
                return octaneRepository.GetEntityFields(resourse, loginDto);
            }).Result, resource2, listnodes, Task.Run(() =>
            {
                return octaneRepository.GetPhases(loginDto, resourse);
            }).Result));
        }

        private ObservableList<ExternalItemFieldBase> AddFieldsValues(ListResult<FieldMetadata> entityFields, string entityType, Dictionary<string, List<string>> listnodes, Dictionary<string, List<string>> phases)
        {
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();
            List<Release> _releases = GetReleases();

            if ((entityFields != null) && (entityFields.total_count.Value > 0))
            {
                foreach (FieldMetadata field in entityFields.data)
                {
                    if (string.IsNullOrEmpty(field.Label) || !field.VisibleInUI || !field.IsEditable 
                        || field.Name.ToLower() == "parent" || field.GetValue("access_level").Equals("PRIVATE"))
                    {
                        continue;
                    }

                    ExternalItemFieldBase itemfield = new ExternalItemFieldBase();
                    itemfield.ID = field.Label;
                    itemfield.ExternalID = field.Name;
                    itemfield.Name = field.Label;
                    itemfield.Mandatory = field.IsRequired;
                    itemfield.SystemFieled = !field.IsUserField;

                    if (itemfield.Mandatory)
                    {
                        itemfield.ToUpdate = true;
                    }
                    itemfield.ItemType = entityType;
                    itemfield.Type = field.FieldType;
                    if (field.FieldType.ToLower() == "reference")
                    {
                        try
                        {
                            BaseEntity temp = (BaseEntity)field.GetValue("field_type_data");
                            itemfield.IsMultiple = temp.GetBooleanValue("multiple").Value;
                        }
                        catch (Exception ex) {
                            Reporter.ToLog(eLogLevel.DEBUG, "Not able to get Multiple value flag", ex);
                        }

                        if (field.Name.ToLower() == "release")
                        {
                            itemfield.PossibleValues = new ObservableList<string>(_releases.Select(g => g.Name).ToList());
                        }
                        else if (listnodes != null && listnodes.ContainsKey(field.Name) && listnodes[field.Name].Any())
                        {
                            itemfield.PossibleValues = new ObservableList<string>(listnodes[field.Name]);
                        }
                        else if (listnodes != null && listnodes.ContainsKey(entityType.ToLower() + "_" + field.Name) && listnodes[entityType.ToLower() + "_" + field.Name].Any())
                        {
                            itemfield.PossibleValues = new ObservableList<string>(listnodes[entityType.ToLower() + "_" + field.Name]);
                        }
                        else if (phases != null && phases.ContainsKey(field.Name) && phases[field.Name].Any())
                        {
                            itemfield.PossibleValues = new ObservableList<string>(phases[field.Name]);
                        }
                    }
                    
                    if (!(itemfield.PossibleValues != null && itemfield.PossibleValues.Count > 0) && itemfield.ExternalID != "closed_on")
                    {
                        itemfield.SelectedValue = "Unassigned";
                    }
                    fields.Add(itemfield);
                }
            }
            return fields;
        }

        public override bool IsServerConnected()
        {
            throw new NotImplementedException();
        }

        public QC.QCTestSet ImportTestSetData(QC.QCTestSet testSet)
        {
            QCTestInstanceColl testInstances = GetTestsFromTSId(testSet.TestSetID);

            foreach (QCTestInstance testInstance in testInstances)
            {
                testSet.Tests.Add(ImportTSTest(testInstance));
            }

            return testSet;
        }

        public QCTestInstanceColl GetTestsFromTSId(string testSetID)
        {
            try
            {
                List<TestSuite_Test_Link> TSLink;
                QCTestInstanceColl testCollection = new QCTestInstanceColl();
                CrossQueryPhrase qd = new CrossQueryPhrase("test_suite", new LogicalQueryPhrase("id", testSetID, ComparisonOperator.Equal));
                IList<IQueryPhrase> filter = new List<IQueryPhrase> { qd,new LogicalQueryPhrase("subtype", "test_suite_link_to_manual", ComparisonOperator.Equal) };
            
                TSLink = octaneRepository.GetEntities<TestSuite_Test_Link>(GetLoginDTO(), filter);
                foreach (TestSuite_Test_Link item in TSLink)
                {
                    testCollection.Add(new QCTestInstance() { Id = item.Test.Id, TestId = item.Test.Id, Name = item.Test.Name, CycleId = testSetID });
                }
                return testCollection;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get test instances of test set with REST API", ex);
                return null;
            }
        }

        public QCTestCase GetTestCases(string testcaseID)
        {
            LogicalQueryPhrase qd = new LogicalQueryPhrase("id", testcaseID, ComparisonOperator.Equal);
            IList<IQueryPhrase> filter = new List<IQueryPhrase> { qd };
            List<Test> test = octaneRepository.GetEntities<Test>(GetLoginDTO(), filter);
            if (test.Any())
            {
                Test testTemp = test.Where(f => true).FirstOrDefault();
                return new QCTestCase() { Id = testcaseID, TestId = testcaseID, Name = testTemp.Name };
            }
            return null;
        }

        public QCTestCaseStepsColl GetListTSTestSteps(QCTestCase testcase)
        {
            var lineBreaks = new[] { '\n' };
            QCTestCaseStepsColl stepsColl = new QCTestCaseStepsColl();
            string steps = Task.Run(() =>
            {
                return octaneRepository.GetTestCaseStep(GetLoginDTO(), testcase.Id);
            }).Result;
            int i = 1;
            if (!string.IsNullOrEmpty(steps))
            {
                foreach (string step in steps.Split(lineBreaks))
                {
                    stepsColl.Add(new QCTestCaseStep() { Id = testcase.Id + "_" + Convert.ToString(i), TestId = testcase.Id, Description = step, Name = step, StepOrder = Convert.ToString(i++) });
                }
            }
            return stepsColl;
        }

        private List<string> GetTCParameterList(string steps)
        {
            List<string> parameters = new List<string>();
            string hasParams = @"(<[a-zA-Z0-9,+/-_()~!@#$%^&*=]+>)";
            MatchCollection mc = Regex.Matches(steps, hasParams);
            if (mc != null && mc.Count > 0)
            {
                foreach (Match m in mc)
                {
                    parameters.Add(m.Value.Substring(1, m.Value.Length - 2));
                }
            }
            return parameters;
        }

        private void CheckForParameter(QC.QCTSTest newTSTest, string steps)
        {
            string hasParams = @"(<[a-zA-Z0-9,+/-_()~!@#$%^&*=]+>)";
            MatchCollection mc = Regex.Matches(steps, hasParams);
            if (mc != null && mc.Count > 0)
            {
                foreach (Match m in mc)
                {
                    QC.QCTSTestParameter newtsVar = new QC.QCTSTestParameter();
                    newtsVar.Name = m.Value.Substring(1, m.Value.Length - 2);
                    newTSTest.Parameters.Add(newtsVar);
                }
            }
        }

        public QC.QCTSTest ImportTSTest(QCTestInstance testInstance)
        {
            QC.QCTSTest newTSTest = new QC.QCTSTest();
            if (newTSTest.Runs == null)
            {
                newTSTest.Runs = new List<QCTSTestRun>();
            }
            QCTestCase testCase = GetTestCases(testInstance.Id);
            testCase.TestSetId = testInstance.CycleId;
            if (testInstance != null)
            {
                //Regular TC
                newTSTest.TestID = testInstance.Id;
                newTSTest.TestName = testInstance.Name;
            }

            //Get the TC design steps
            QCTestCaseStepsColl TSTestSteps = GetListTSTestSteps(testCase);
            foreach (QCTestCaseStep testcaseStep in TSTestSteps)
            {
                QC.QCTSTestStep newtsStep = new QC.QCTSTestStep();
                newtsStep.StepID = testcaseStep.Id.ToString();
                newtsStep.StepName = testcaseStep.Name;
                newtsStep.Description = testcaseStep.Description;
                newtsStep.Expected = "";
                newTSTest.Steps.Add(newtsStep);
                //Get the TC parameters
                CheckForParameter(newTSTest, newtsStep.Description);
            }

            //Get the TC execution history
            try
            {
                List<RunSuite> TSTestRuns = GetTestSuiteRun(testCase.TestSetId);

                foreach (RunSuite run in TSTestRuns)
                {
                    QC.QCTSTestRun newtsRun = new QC.QCTSTestRun();
                    newtsRun.RunID = run.Id;
                    newtsRun.RunName = run.Name;
                    newtsRun.Status = run.NativeStatus.Name;
                    newtsRun.ExecutionDate = (run.GetValue("started").ToString());
                    newtsRun.ExecutionTime = (run.GetValue("last_modified").ToString());
                    newtsRun.Tester = (run.DefaultRunBy.FullName).ToString();
                    newTSTest.Runs.Add(newtsRun);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to pull QC test case RUN info", ex);
                newTSTest.Runs = new List<QC.QCTSTestRun>();
            }
            return newTSTest;
        }

        public List<RunSuite> GetTestSuiteRun(string testSuiteId)
        {
            QCRunColl runColl = new QCRunColl();
            EntityListResult<RunSuite> testFolders = new EntityListResult<RunSuite>();
            LogicalQueryPhrase test_Suite = new LogicalQueryPhrase("id", testSuiteId, ComparisonOperator.Equal);
            CrossQueryPhrase qd = new CrossQueryPhrase("test", test_Suite);

            return octaneRepository.GetEntities<RunSuite>(GetLoginDTO(), new List<IQueryPhrase> { qd });
        }

        public dynamic GetTSRunStatus(dynamic TSItem)
        {
            List<RunSuite> testInstances = GetTestSuiteRun(TSItem.TestSetID);

            foreach (RunSuite testInstance in testInstances)
            {
                bool existing = false;
                foreach (string[] status in TSItem.TestSetStatuses)
                {
                    if (status[0] == testInstance.NativeStatus.Name)
                    {
                        existing = true;
                        status[1] = (Int32.Parse(status[1]) + 1).ToString();
                    }
                }
                if (!existing) { TSItem.TestSetStatuses.Add(new string[] { testInstance.NativeStatus.Name, "1" }); }
            }
            return TSItem;
        }

        public BusinessFlow ConvertQCTestSetToBF(QC.QCTestSet testSet)
        {
            try
            {
                if (testSet == null) { return null; }

                TestSuite tsLatest = Task.Run(() =>
                {
                    LogicalQueryPhrase test_Suite = new LogicalQueryPhrase("id", testSet.TestSetID, ComparisonOperator.Equal);
                    return octaneRepository.GetEntities<TestSuite>(GetLoginDTO(), new List<IQueryPhrase>() { test_Suite });
                }).Result.FirstOrDefault();
                //Create Business Flow
                BusinessFlow busFlow = new BusinessFlow();
                busFlow.Name = testSet.TestSetName;
                busFlow.ExternalID = testSet.TestSetID;
                busFlow.Status = BusinessFlow.eBusinessFlowStatus.Development;
                busFlow.Description = StripHTML(tsLatest.GetStringValue("description"));
                busFlow.Activities = new ObservableList<Activity>();
                busFlow.Variables = new ObservableList<VariableBase>();
                Dictionary<string, string> busVariables = new Dictionary<string, string>();//will store linked variables

                //Create Activities Group + Activities for each TC
                foreach (QC.QCTSTest tc in testSet.Tests)
                {
                    //check if the TC is already exist in repository
                    ActivitiesGroup tcActivsGroup;
                    ActivitiesGroup repoActivsGroup = null;
                    if (tc.LinkedTestID != null && tc.LinkedTestID != string.Empty)
                    {
                        repoActivsGroup = GingerActivitiesGroupsRepo.Where(x => x.ExternalID == tc.LinkedTestID).FirstOrDefault();
                    }
                    if (repoActivsGroup == null)
                    {
                        repoActivsGroup = GingerActivitiesGroupsRepo.Where(x => x.ExternalID == tc.TestID).FirstOrDefault();
                    }
                    if (repoActivsGroup != null)
                    {
                        List<Activity> repoNotExistsStepActivity = GingerActivitiesRepo.Where(z => repoActivsGroup.ActivitiesIdentifiers.Select(y => y.ActivityExternalID).ToList().Contains(z.ExternalID))
                                                                                       .Where(x => !tc.Steps.Select(y => y.StepID).ToList().Contains(x.ExternalID)).ToList();

                        tcActivsGroup = (ActivitiesGroup)repoActivsGroup.CreateInstance();

                        var ActivitySIdentifiersToRemove = tcActivsGroup.ActivitiesIdentifiers.Where(x => repoNotExistsStepActivity.Select(z => z.ExternalID).ToList().Contains(x.ActivityExternalID));
                        for (int indx = 0; indx < tcActivsGroup.ActivitiesIdentifiers.Count; indx++)
                        {
                            if ((indx < tcActivsGroup.ActivitiesIdentifiers.Count) && (ActivitySIdentifiersToRemove.Contains(tcActivsGroup.ActivitiesIdentifiers[indx])))
                            {
                                tcActivsGroup.ActivitiesIdentifiers.Remove(tcActivsGroup.ActivitiesIdentifiers[indx]);
                                indx--;
                            }
                        }

                        tcActivsGroup.ExternalID2 = tc.TestID;
                        busFlow.AddActivitiesGroup(tcActivsGroup);
                        busFlow.ImportActivitiesGroupActivitiesFromRepository(tcActivsGroup, GingerActivitiesRepo, true, true);
                        busFlow.AttachActivitiesGroupsAndActivities();
                    }
                    else //TC not exist in Ginger repository so create new one
                    {
                        tcActivsGroup = new ActivitiesGroup();
                        tcActivsGroup.Name = tc.TestName;
                        if (tc.LinkedTestID == null || tc.LinkedTestID == string.Empty)
                        {
                            tcActivsGroup.ExternalID = tc.TestID;
                            tcActivsGroup.ExternalID2 = tc.TestID;
                        }
                        else
                        {
                            tcActivsGroup.ExternalID = tc.LinkedTestID;
                            tcActivsGroup.ExternalID2 = tc.TestID; //original TC ID will be used for uploading the execution details back to QC
                            tcActivsGroup.Description = tc.Description;
                        }
                        busFlow.AddActivitiesGroup(tcActivsGroup);
                    }

                    //Add the TC steps as Activities if not already on the Activities group
                    foreach (QC.QCTSTestStep step in tc.Steps)
                    {
                        Activity stepActivity;
                        bool toAddStepActivity = false;

                        //check if mapped activity exist in repository
                        Activity repoStepActivity = (Activity)GingerActivitiesRepo.Where(x => x.ExternalID == step.StepID).FirstOrDefault();
                        if (repoStepActivity != null)
                        {
                            //check if it is part of the Activities Group
                            ActivityIdentifiers groupStepActivityIdent = (ActivityIdentifiers)tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.StepID).FirstOrDefault();
                            if (groupStepActivityIdent != null)
                            {
                                //already in Activities Group so get link to it
                                stepActivity = (Activity)busFlow.Activities.Where(x => x.Guid == groupStepActivityIdent.ActivityGuid).FirstOrDefault();
                                // in any case update description/expected/name - even if "step" was taken from repository
                                stepActivity.Description = step.Description;
                                stepActivity.Expected = step.Expected;
                                stepActivity.ActivityName = tc.TestName + ">" + step.StepName;
                            }
                            else//not in ActivitiesGroup so get instance from repo
                            {
                                stepActivity = (Activity)repoStepActivity.CreateInstance();
                                toAddStepActivity = true;
                            }
                        }
                        else//Step not exist in Ginger repository so create new one
                        {
                            stepActivity = new Activity();
                            stepActivity.ActivityName = tc.TestName + ">" + step.StepName;
                            stepActivity.ExternalID = step.StepID;
                            stepActivity.Description = step.Description;
                            stepActivity.Expected = step.Expected;

                            toAddStepActivity = true;
                        }

                        if (toAddStepActivity)
                        {
                            //not in group- need to add it
                            busFlow.AddActivity(stepActivity, tcActivsGroup);
                        }

                        //pull TC-Step parameters and add them to the Activity level
                        List<string> stepParamsList;
                        stepParamsList = GetTCParameterList(step.Description);
                        foreach (string param in stepParamsList)
                        {
                            //get the param value
                            string paramSelectedValue = string.Empty;
                            bool? isflowControlParam = null;
                            QC.QCTSTestParameter tcParameter = tc.Parameters.Where(x => x.Name.ToUpper() == param.ToUpper()).FirstOrDefault();

                            //get the param value
                            if (tcParameter != null && tcParameter.Value != null && tcParameter.Value != string.Empty)
                            {
                                paramSelectedValue = tcParameter.Value;
                            }
                            else
                            {
                                isflowControlParam = null;//empty value
                                paramSelectedValue = "<Empty>";
                            }

                            //check if parameter is part of a link
                            string linkedVariable = null;
                            if (paramSelectedValue.StartsWith("#$#"))
                            {
                                var valueParts = paramSelectedValue.Split(new[] { "#$#" }, StringSplitOptions.None);
                                if (valueParts.Count() == 3)
                                {
                                    linkedVariable = valueParts[1];
                                    paramSelectedValue = "$$_" + valueParts[2];//so it still will be considered as non-flow control

                                    if (busVariables.Keys.Contains(linkedVariable))
                                    {
                                        busVariables.Add(linkedVariable, valueParts[2]);
                                    }
                                }
                            }

                            //determine if the param is Flow Control Param or not based on it value and agreed sign "$$_"
                            if (paramSelectedValue.StartsWith("$$_"))
                            {
                                isflowControlParam = false;
                                if (paramSelectedValue.StartsWith("$$_"))
                                {
                                    paramSelectedValue = paramSelectedValue.Substring(3);//get value without "$$_"
                                }
                            }
                            else if (paramSelectedValue != "<Empty>")
                            {
                                isflowControlParam = true;
                            }

                            //check if already exist param with that name
                            VariableBase stepActivityVar = stepActivity.Variables.Where(x => x.Name.ToUpper() == param.ToUpper()).FirstOrDefault();
                            if (stepActivityVar == null)
                            {
                                //#Param not exist so add it
                                if (isflowControlParam != null && isflowControlParam.Value)
                                {
                                    //add it as selection list param                               
                                    stepActivityVar = new VariableSelectionList();
                                    stepActivityVar.Name = param;
                                    stepActivity.AddVariable(stepActivityVar);
                                    stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because new flow control param was added
                                }
                                else
                                {
                                    //add as String param
                                    stepActivityVar = new VariableString();
                                    stepActivityVar.Name = param;
                                    ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                                    stepActivity.AddVariable(stepActivityVar);
                                }
                            }
                            else
                            {
                                //#param exist
                                if (isflowControlParam != null && isflowControlParam.Value)
                                {
                                    if (!(stepActivityVar is VariableSelectionList))
                                    {
                                        //flow control param must be Selection List so transform it
                                        stepActivity.Variables.Remove(stepActivityVar);
                                        stepActivityVar = new VariableSelectionList();
                                        stepActivityVar.Name = param;
                                        stepActivity.AddVariable(stepActivityVar);
                                        stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because flow control param was added
                                    }
                                }
                                else if (isflowControlParam != null && !isflowControlParam.Value)
                                {
                                    if (stepActivityVar is VariableSelectionList)
                                    {
                                        //change it to be string variable
                                        stepActivity.Variables.Remove(stepActivityVar);
                                        stepActivityVar = new VariableString();
                                        stepActivityVar.Name = param;
                                        ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                                        stepActivity.AddVariable(stepActivityVar);
                                        stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because flow control param was removed
                                    }
                                }
                            }

                            //add the variable selected value                          
                            if (stepActivityVar is VariableSelectionList)
                            {
                                OptionalValue stepActivityVarOptionalVar = ((VariableSelectionList)stepActivityVar).OptionalValuesList.Where(x => x.Value == paramSelectedValue).FirstOrDefault();
                                if (stepActivityVarOptionalVar == null)
                                {
                                    //no such variable value option so add it
                                    stepActivityVarOptionalVar = new OptionalValue(paramSelectedValue);
                                    ((VariableSelectionList)stepActivityVar).OptionalValuesList.Add(stepActivityVarOptionalVar);
                                    if (isflowControlParam.Value)
                                    {
                                        stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because new param value was added
                                    }
                                }
                                //set the selected value
                                ((VariableSelectionList)stepActivityVar).SelectedValue = stepActivityVarOptionalVar.Value;
                            }
                            else
                            {
                                //try just to set the value
                                try
                                {
                                    stepActivityVar.Value = paramSelectedValue;
                                    if (stepActivityVar is VariableString)
                                    {
                                        ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                                    }
                                }
                                catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
                            }

                            //add linked variable if needed
                            if (string.IsNullOrEmpty(linkedVariable))
                            {
                                stepActivityVar.LinkedVariableName = linkedVariable;
                            }
                            else
                            {
                                stepActivityVar.LinkedVariableName = string.Empty;//clear old links
                            }
                        }
                    }

                    //order the Activities Group activities according to the order of the matching steps in the TC
                    try
                    {
                        int startGroupActsIndxInBf = busFlow.Activities.IndexOf(tcActivsGroup.ActivitiesIdentifiers[0].IdentifiedActivity);
                        foreach (QC.QCTSTestStep step in tc.Steps)
                        {
                            int stepIndx = tc.Steps.IndexOf(step) + 1;
                            ActivityIdentifiers actIdent = (ActivityIdentifiers)tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.StepID).FirstOrDefault();
                            if (actIdent == null || actIdent.IdentifiedActivity == null) { break; }
                            Activity act = (Activity)actIdent.IdentifiedActivity;
                            int groupActIndx = tcActivsGroup.ActivitiesIdentifiers.IndexOf(actIdent);
                            int bfActIndx = busFlow.Activities.IndexOf(act);

                            //set it in the correct place in the group
                            int numOfSeenSteps = 0;
                            int groupIndx = -1;
                            foreach (ActivityIdentifiers ident in tcActivsGroup.ActivitiesIdentifiers)
                            {
                                groupIndx++;
                                if (string.IsNullOrEmpty(ident.ActivityExternalID) ||
                                        tc.Steps.Where(x => x.StepID == ident.ActivityExternalID).FirstOrDefault() == null)
                                {
                                    continue;//activity which not originally came from the TC
                                }
                                numOfSeenSteps++;

                                if (numOfSeenSteps >= stepIndx) { break; }
                            }
                            ActivityIdentifiers identOnPlace = (ActivityIdentifiers)tcActivsGroup.ActivitiesIdentifiers[groupIndx];
                            if (identOnPlace.ActivityGuid != act.Guid)
                            {
                                //replace places in group
                                tcActivsGroup.ActivitiesIdentifiers.Move(groupActIndx, groupIndx);
                                //replace places in business flow
                                busFlow.Activities.Move(bfActIndx, startGroupActsIndxInBf + groupIndx);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                        //failed to re order the activities to match the tc steps order, not worth breaking the import because of this
                    }
                }

                //Add the BF variables (linked variables)
                if (busVariables.Keys.Count > 0)
                {
                    foreach (KeyValuePair<string, string> var in busVariables)
                    {
                        //add as String param
                        VariableString busVar = new VariableString();
                        busVar.Name = var.Key;
                        busVar.InitialStringValue = var.Value;
                        busFlow.AddVariable(busVar);
                    }
                }

                return busFlow;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to import QC test set and convert it into " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), ex);
                return null;
            }
        }

        public bool ExportBusinessFlow(BusinessFlow businessFlow, QCTestSet mappedTestSet, string fatherId, ObservableList<ExternalItemFieldBase> testSetFields, ObservableList<ExternalItemFieldBase> testInstanceFields, ref string result)
        {
            int testSetId = 0;
            try
            {
                if (mappedTestSet == null) //##create new Test Set in QC
                {
                    testSetId = CreateNewTestSet(businessFlow, fatherId, testSetFields);
                }
                else //##update existing test set
                {
                    testSetId = UpdateExistingTestSet(businessFlow, mappedTestSet, fatherId, testSetFields);
                }

                businessFlow.ExternalID = testSetId.ToString();
                return true;
            }
            catch (Exception ex)
            {
                result = "Unexpected error occurred- " + ex.Message;
                Reporter.ToLog(eLogLevel.ERROR, "Failed to export the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " to QC/ALM", ex);
                return false;
            }
        }
        private int CreateNewTestSet(BusinessFlow businessFlow, string fatherId, ObservableList<ExternalItemFieldBase> testSetFields)
        {            
            TestSuite testSuite = new TestSuite();
            testSuite.Name = businessFlow.Name;
            testSuite.SetValue("description", businessFlow.Description);
            testSuite.SetValue("product_areas", new EntityList<BaseEntity>()
            {
                data = new List<BaseEntity>() { new BaseEntity("product_area") { Id = fatherId, TypeName = "product_area" } }
            });
            AddEntityFieldValues(testSetFields, testSuite, "test_suite");
            TestSuite created = Task.Run(() =>
            {
                return this.octaneRepository.CreateEntity<TestSuite>(GetLoginDTO(), testSuite);
            }).Result;

            int testSuiteId = Convert.ToInt32(created.Id.ToString());

            LinkTestCasesToTestSuite(testSuiteId, businessFlow.ActivitiesGroups.Select(f => int.Parse(f.ExternalID)).ToList());

            return testSuiteId;
        }
        private int UpdateExistingTestSet(BusinessFlow businessFlow, QCTestSet existingTS, string fatherId, ObservableList<ExternalItemFieldBase> testSetFields)
        {
            TestSuite testSuite = new TestSuite();
            testSuite.Id = existingTS.Id;
            testSuite.Name = businessFlow.Name;
            testSuite.SetValue("description", businessFlow.Description);
            testSuite.SetValue("product_areas", new EntityList<BaseEntity>()
            {
                data = new List<BaseEntity>() { new BaseEntity("product_area") { Id = fatherId, TypeName = "product_area" } }
            });
            AddEntityFieldValues(testSetFields, testSuite, "test_suite");
            TestSuite created = Task.Run(() =>
            {
                return this.octaneRepository.UpdateEntity<TestSuite>(GetLoginDTO(), testSuite);
            }).Result;

            int testSuiteId = Convert.ToInt32(created.Id.ToString());

            DeleteLinkTestCasesToTestSuite(testSuiteId);
            LinkTestCasesToTestSuite(testSuiteId, businessFlow.ActivitiesGroups.Select(f => int.Parse(f.ExternalID)).ToList());

            return testSuiteId;
        }


        private void LinkTestCasesToTestSuite(int testSuiteId, List<int> entityTcIdMapping)
        {
            foreach (int tcId in entityTcIdMapping)
            {
                TestSuiteLinkToTests suiteLinkToTests = new TestSuiteLinkToTests();

                suiteLinkToTests.SetValue("subtype", "test_suite_link_to_manual");

                suiteLinkToTests.SetValue("test_suite", new BaseEntity("test") { Id = testSuiteId.ToString(), TypeName = "test" });

                suiteLinkToTests.SetValue("test", new BaseEntity("test") { Id = tcId.ToString(), TypeName = "test" });

                var created = Task.Run(() =>
                {
                    return octaneRepository.CreateEntity<TestSuiteLinkToTests>(GetLoginDTO(), suiteLinkToTests);
                }).Result;
            }
        }


        private void DeleteLinkTestCasesToTestSuite(int testSuiteId)
        {
            CrossQueryPhrase qd = new CrossQueryPhrase("test_suite", new LogicalQueryPhrase("id", testSuiteId, ComparisonOperator.Equal));
            Task.Run(() =>
            {
                this.octaneRepository.DeleteEntity<TestSuiteLinkToTests>(GetLoginDTO(), new List<IQueryPhrase>() { qd });
            });
        }
        public string GetLastTestPlanIdFromPath(string path)
        {
            string[] separatePath = path.Split('\\');

            separatePath[0] = ExploredApplicationModule.ContainsKey("Application Modules") ? ExploredApplicationModule["Application Modules"] : GetRootFolderId();

            if (!ExploredApplicationModule.ContainsKey("Application Modules"))
            {
                ExploredApplicationModule.Add("Application Modules", separatePath[0]);
            }

            for (int i = 1; i < separatePath.Length; i++)
            {
                separatePath[i] = GetTestLabFolderId(separatePath[i], separatePath[i - 1]);
            }

            return separatePath.Last();
        }

        public string CreateApplicationModule(string appModuleNameTobeCreated, string desc, string paraentId)
        {
            ApplicationModule applicationModule = new ApplicationModule();
            applicationModule.Name = appModuleNameTobeCreated;
            applicationModule.SetValue("description", desc);

            applicationModule.SetValue("parent", new BaseEntity("application_module")
            {
                Id = paraentId,
                TypeName = "application_module"
            });

            ApplicationModule module = Task.Run(() =>
            {
                return this.octaneRepository.CreateEntity<ApplicationModule>(GetLoginDTO(), applicationModule, new List<string>() { "path", "id" });
            }).Result;

            return module.Id.ToString();
        }

        public bool ExportActivitiesGroupToALM(ActivitiesGroup activitiesGroup, QCTestCase mappedTest, string fatherId, ObservableList<ExternalItemFieldBase> testCaseFields, ObservableList<ExternalItemFieldBase> designStepsFields, ObservableList<ExternalItemFieldBase> designStepsParamsFields, ref string result)
        {
            try
            {
                string step = string.Empty;
                activitiesGroup.ActivitiesIdentifiers.ToList().ForEach(p =>
                {
                    if (!string.IsNullOrEmpty(step))
                    {
                        step += "- ";
                    }
                    step += p.ActivityName + " ";

                    if (p.IdentifiedActivity.Variables.Any())
                    {
                        p.IdentifiedActivity.Variables.ToList().ForEach(f =>
                        {
                            step += " <" + f.Name + "> ";
                        });
                    }
                    step += '\n';
                });

                if (mappedTest == null) //#Create new test case
                {
                    CreateNewTestCase(activitiesGroup, fatherId, testCaseFields, step);
                }
                else //##update existing test case
                {
                    //TODO: Maheshk: Update existing testcase
                    if (!string.IsNullOrEmpty(activitiesGroup.ExternalID))
                    {
                        UpdateTestCase(activitiesGroup, fatherId, testCaseFields, step);
                    }
                    else
                    {
                        CreateNewTestCase(activitiesGroup, fatherId, testCaseFields, step);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                result = "Unexpected error occurred- " + ex.Message;
                Reporter.ToLog(eLogLevel.ERROR, "Failed to export the " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " to QC/ALM", ex);
                return false;
            }
        }

        private string CreateNewTestCase(ActivitiesGroup activitiesGroup, string fatherId, ObservableList<ExternalItemFieldBase> testCaseFields, string testScript)
        {
            //set item fields
            TestManual test = new TestManual();
            test.Name = activitiesGroup.Name;
            test.SetValue("description", activitiesGroup.Description);
            test.SetValue("product_areas", new EntityList<BaseEntity>()
            {
                data = new List<BaseEntity>()
                {
                    new BaseEntity() {Id = fatherId, TypeName = "product_area"}
                }
            });

            AddEntityFieldValues(testCaseFields, test, "test_manual");

            test = Task.Run(() => { return this.octaneRepository.CreateEntity(GetLoginDTO(), test, null); }).Result;

            activitiesGroup.ExternalID = test.Id.ToString();
            activitiesGroup.ExternalID2 = test.Id.ToString();
            CreateTestStep(test.Id, testScript);

            return test.Id.ToString();
        }

        private void AddEntityFieldValues(ObservableList<ExternalItemFieldBase> fields, BaseEntity test, string entityType)
        {
            foreach (ExternalItemFieldBase field in fields)
            {
                try
                {
                    if ((field.ToUpdate || field.Mandatory) && !test.Contains(field.ExternalID))
                    {
                        if (!string.IsNullOrEmpty(field.SelectedValue)&& field.SelectedValue != "Unassigned")
                        {
                            if (field.Type.ToLower() != "reference")
                            {
                                test.SetValue(field.ExternalID, field.SelectedValue);
                            }
                            else
                            {
                                if (field.ExternalID == "phase")
                                {
                                    test.SetValue(field.ExternalID, new BaseEntity()
                                    {
                                        TypeName = "phase",
                                        Id = "phase." + entityType + "." + field.SelectedValue.ToLower()
                                    });
                                }
                                else if (field.ExternalID == "release")
                                {
                                    List<Release> releases = GetReleases();
                                    test.SetValue(field.ExternalID, new BaseEntity()
                                    {
                                        TypeName = "release",
                                        Id = releases.Where(r => r.Name.Equals(field.SelectedValue)).FirstOrDefault().Id
                                    });
                                }
                                else
                                {
                                    if (field.IsMultiple)
                                    {
                                        test.SetValue(field.ExternalID, new EntityList<BaseEntity>()
                                        {
                                            data = new List<BaseEntity>() {
                                                new BaseEntity()
                                                {
                                                    TypeName = "list_node",
                                                    Id = "list_node."+field.ExternalID +"."+field.SelectedValue.ToLower()
                                                }
                                            }
                                        });
                                    }
                                    else
                                    {
                                        test.SetValue(field.ExternalID, new BaseEntity()
                                        {
                                            TypeName = "list_node",
                                            Id = "list_node." + field.ExternalID + "." + field.SelectedValue
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "In AddEntityFieldValues function", ex);
                }
            }
        }

        private List<Release> GetReleases()
        {
            if (this.releases == null)
            {
                this.releases = Task.Run(() =>
                  {
                      return octaneRepository.GetEntities<Release>(GetLoginDTO(), null);
                  }).Result;
            }
            return this.releases;
        }

        private string UpdateTestCase(ActivitiesGroup activitiesGroup, string fatherId, ObservableList<ExternalItemFieldBase> testCaseFields, string testScript)
        {
            TestManual test = new TestManual();
            test.Id = activitiesGroup.ExternalID;
            test.Name = activitiesGroup.Name;
            test.SetValue("description", activitiesGroup.Description);

            test.SetValue("product_areas", new EntityList<BaseEntity>()
            {
                data = new List<BaseEntity>()
                {
                    new BaseEntity() {Id = fatherId, TypeName = "product_area"}
                }
            }); 

            AddEntityFieldValues(testCaseFields, test, "test_manual");

            test = Task.Run(() => { return this.octaneRepository.UpdateEntity(GetLoginDTO(), test, null); }).Result;

            activitiesGroup.ExternalID = test.Id.ToString();
            activitiesGroup.ExternalID2 = test.Id.ToString();
            CreateTestStep(test.Id, testScript);

            return test.Id.ToString();
        }

        private string CreateTestStep(string tcId, string script)
        {
            TestScript testScript = new TestScript();
            testScript.Id = new EntityId(tcId);
            testScript.SetValue("script", script);
            Task.Run(() => { return this.octaneRepository.AddStepsToTC<TestScript>(GetLoginDTO(), testScript, null); });
            return "";
        }

        public QCTestSet GetTestSuiteById(string tsId)
        {
            List<TestSuite> testsuite = Task.Run(() =>
            {
                LogicalQueryPhrase test_Suite = new LogicalQueryPhrase("id", tsId, ComparisonOperator.Equal);
                return octaneRepository.GetEntities<TestSuite>(GetLoginDTO(), new List<IQueryPhrase>() { test_Suite });
            }).Result;

            if (testsuite.Any())
            {
                EntityList<BaseEntity> father = (EntityList<BaseEntity>)testsuite[0].GetValue("product_areas");
                if (father != null && father.data.Any())
                {
                    return new QCTestSet() { Id = testsuite[0].Id, Name = testsuite[0].Name, ParentId = father.data[0].Id };
                }
                return new QCTestSet() { Id = testsuite[0].Id, Name = testsuite[0].Name, ParentId = GetRootFolderId() };
            }
            return null;
        }

        public string ConvertResourceType(ResourceType resourceType)
        {
            return resourceType.ToString();
        }

        private static string StripHTML(string HTMLText, bool toDecodeHTML = true)
        {
            try
            {
                if (!string.IsNullOrEmpty(HTMLText))
                {
                    HTMLText = HTMLText.Replace("<br />", Environment.NewLine);
                    Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
                    var stripped = reg.Replace(HTMLText, "");
                    if (toDecodeHTML)
                    {
                        stripped = HttpUtility.HtmlDecode(stripped);
                    }
                    stripped = stripped.Trim();
                    stripped = stripped.TrimStart('\n', '\r');
                    stripped = stripped.TrimEnd('\n', '\r');

                    return stripped; 
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while stripping the HTML from QC TC Step Description/Expected", ex);
                return HTMLText;
            }
        }
    }
}
