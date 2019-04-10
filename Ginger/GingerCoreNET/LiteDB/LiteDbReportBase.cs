using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;

namespace Amdocs.Ginger.CoreNET.LiteDB
{
    public class LiteDbReportBase
    {
        public int Seq { get; set; }
        public string GUID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string RunDescription { get; set; }
        public string Enviroment { get; set; }
        public DateTime StartTimeStamp { get; set; }
        public DateTime EndTimeStamp { get; set; }
        public string Elapsed { get; set; }
        public string RunStatus { get; set; }
        public string VariablesBeforeExec { get; set; }
        public string VariablesAfterExec { get; set; }
        public ObjectId _id { get; set; }
        public LiteDbReportBase()
        {
            _id = ObjectId.NewObjectId();
        }
    }
    public class LiteDbRunSet : LiteDbReportBase
    {
        public string GingerVersion { get; set; }
        public string MachineName { get; set; }
        public string ExecutedbyUser { get; set; }
        public List<LiteDbRunner> RunnerColl { get; set; }
        public LiteDbRunSet()
        {
            RunnerColl = new List<LiteDbRunner>();
        }
    }
    public class LiteDbRunner : LiteDbReportBase
    {
        public List<string> ApplicationAgentsMappingList { get; set; }
        public List<LiteDbBusinessFlow> BusinessFlowColl { get; set; }
        public LiteDbRunner()
        {
            BusinessFlowColl = new List<LiteDbBusinessFlow>();
        }
    }
    public class LiteDbBusinessFlow : LiteDbReportBase
    {
        public Guid InstanceGUID { get; set; }
        public List<LiteDbActivity> ActivitiesColl { get; set; }
        public List<LiteDbActivityGroup> ActivitiesGroupColl { get; set; }
        public LiteDbBusinessFlow()
        {
            ActivitiesGroupColl = new List<LiteDbActivityGroup>();
            ActivitiesColl = new List<LiteDbActivity>();
        }
    }

    public class LiteDbActivityGroup : LiteDbReportBase
    {
        public string ExecutedActivitiesGUID { get; set; }
        public string AutomationPrecentage { get; set; }
        public List<LiteDbActivity> ActivitiesColl { get; set; }
        public LiteDbActivityGroup()
        {
            ActivitiesColl = new List<LiteDbActivity>();
        }
    }
    public class LiteDbActivity : LiteDbReportBase
    {
        public string ActivityGroupName { get; set; }
        public List<LiteDbAction> actionsColl { get; set; }
        public LiteDbActivity()
        {
            actionsColl = new List<LiteDbAction>();
        }
    }
    public class LiteDbAction : LiteDbReportBase
    {
        public string CurrentRetryIteration { get; set; }
        public string Error { get; set; }
        public string ExInfo { get; set; }
        public string InputValues { get; set; }
        public string OutputValues { get; set; }
        public string ScreenShots { get; set; }
        public LiteDbAction()
        {

        }
    }
}
