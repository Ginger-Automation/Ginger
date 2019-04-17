using amdocs.ginger.GingerCoreNET;
using Ginger.Run;
using GingerCore.Environments;
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
                    // GingerRunner.bus

                    // set BF Variables
                    foreach (SetBusinessFlowVariable sv in abf.Variables)
                    {
                        businessFlowRun.BusinessFlowCustomizedRunVariables.Add(new VariableString() { Name = sv.Name, Value = sv.Value });
                        // VariableBase v = (from x in businessFlow.Variables where x.Name == sv.Name select x).SingleOrDefault();
                        // TODO: if null !!!
                        // Based on variable type we check and set value !!!!!!!!!!!!! TODO: validations
                        // For now setting value of a string
                        //if (v.GetType() == typeof(VariableString))
                        //{
                        //    ((VariableString)v).Value = sv.Value;
                        //}
                        //else
                        //{
                        //    // temp !!!!!!!!!!!!
                        //    throw new Exception("Cannot set variable which is not a string: " + v.Name + " var type is: " + v.GetType().Name);
                        //}
                    }
                }
            }
            // do init runners

            // Set config
            runsetExecutor.RunSetConfig = runSetConfig;
        }

        public static string CreateRunSet(RunsetExecutor runsetExecutor)
        {
            return "xml 123";
        }
    }
}
