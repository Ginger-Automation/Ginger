using amdocs.ginger.GingerCoreNET;
using Ginger.Run;
using GingerCore.Environments;
using GingerCore.Platforms;
using GingerCore.Variables;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib
{
    public class DynamicRunSetManager
    {
        public static void Save(DynamicRunSet dynamicRunSet, string fileName)
        {
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(DynamicRunSet));
            System.IO.FileStream file = System.IO.File.Create(fileName);
            writer.Serialize(file, dynamicRunSet);
            file.Close();
        }

        public static DynamicRunSet Load(string fileName)
        {
            System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(DynamicRunSet));
            System.IO.StreamReader file = new System.IO.StreamReader(fileName);
            DynamicRunSet dynamicRunSet = (DynamicRunSet)reader.Deserialize(file);
            file.Close();
            return dynamicRunSet;
        }

        public static DynamicRunSet LoadContent(string content)
        {
            System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(DynamicRunSet));
            System.IO.StringReader stringReader = new System.IO.StringReader(content);
            DynamicRunSet dynamicRunSet = (DynamicRunSet)reader.Deserialize(stringReader);
            stringReader.Close();
            return dynamicRunSet;
        }

        public static string GetContent(DynamicRunSet dynamicRunSet)
        {
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(DynamicRunSet));
            System.IO.StringWriter stringWriter = new System.IO.StringWriter();
            writer.Serialize(stringWriter, dynamicRunSet);
            stringWriter.Close();
            return stringWriter.GetStringBuilder().ToString();
                
        }

        public static void LoadRunSet(RunsetExecutor runsetExecutor, DynamicRunSet dynamicRunSet)
        {
            RunSetConfig runSetConfig = new RunSetConfig();
            runSetConfig.Name = dynamicRunSet.Name;

            // Set env
            ProjEnvironment projEnvironment = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>() where x.Name == dynamicRunSet.Environemnt select x).SingleOrDefault();
            // TODO: if null !!!
            runsetExecutor.RunsetExecutionEnvironment = projEnvironment;

            // Add runners
            foreach (AddRunner r in dynamicRunSet.Runners)
            {
                GingerRunner gingerRunner = new GingerRunner();
                gingerRunner.Name = r.Name;
                runSetConfig.GingerRunners.Add(gingerRunner);
                // Add BFs
                foreach (AddBusinessFlow abf in r.BusinessFlows)
                {
                    // find the BF
                    // BusinessFlow businessFlow = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>() where x.Name == abf.Name select x).SingleOrDefault();
                    BusinessFlowRun businessFlowRun = new BusinessFlowRun();
                    businessFlowRun.BusinessFlowName = abf.Name;
                    // TODO: if null !!!
                    gingerRunner.BusinessFlowsRunList.Add(businessFlowRun);                    

                    // set BF Variables
                    foreach (SetBusinessFlowVariable sv in abf.Variables)
                    {
                        businessFlowRun.BusinessFlowCustomizedRunVariables.Add(new VariableString() { Name = sv.Name, Value = sv.Value });                        
                    }
                }
            }            

            // Set config
            runsetExecutor.RunSetConfig = runSetConfig;
        }

        public static string CreateRunSet(RunsetExecutor runsetExecutor)
        {
            DynamicRunSet dynamicRunSet = new DynamicRunSet();
            dynamicRunSet.Name = runsetExecutor.RunSetConfig.Name;
            dynamicRunSet.Environemnt = runsetExecutor.RunsetExecutionEnvironment.Name;
            foreach (GingerRunner gingerRunner in runsetExecutor.RunSetConfig.GingerRunners)
            {
                AddRunner addRunner = new AddRunner() { Name = gingerRunner.Name };
                foreach (ApplicationAgent applicationAgent in gingerRunner.ApplicationAgents)
                {
                    addRunner.Agents.Add(new SetAgent() { Agent = applicationAgent.AgentName, TargetApplication = applicationAgent.AppName });
                }
                
                dynamicRunSet.Runners.Add(addRunner);
                foreach(BusinessFlowRun businessFlowRun in gingerRunner.BusinessFlowsRunList)
                {
                    AddBusinessFlow addBusinessFlow = new AddBusinessFlow() { Name = businessFlowRun.BusinessFlowName };
                    // TODO: add vars override
                    addRunner.BusinessFlows.Add(addBusinessFlow);
                }                
            }
            string content = GetContent(dynamicRunSet);
            return content;
        }
    }
}
