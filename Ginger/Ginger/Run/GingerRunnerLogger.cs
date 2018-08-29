using Amdocs.Ginger.Common.Actions;
using Amdocs.Ginger.Repository;
using GingerCoreNET.GeneralLib;
using GingerCore.Actions;
using System.Text;
using Amdocs.Ginger.CoreNET.GeneralLib;

namespace Ginger.Run
{
    public class GingerRunnerLogger
    {
        Logger logger;

        public GingerRunnerLogger(string fileName)
        {
            logger = new Logger(fileName, false);
            logger.LogTime();
            logger.Log("Log started");
        }

        public void LogAction(Act act)
        {
            if (!act.EnableActionLogConfig) return;

            StringBuilder strBuilder = new StringBuilder();
            ActionLogConfig actionLogConfig = act.ActionLogConfig;
            FormatTextTable formatTextTable = new FormatTextTable();

            // create a new log file if not exists and append the contents
            strBuilder.AppendLine("[Action] " + act.ActionDescription);
            strBuilder.AppendLine("[Text] " + actionLogConfig.LogText);

            // log all the input values
            if (actionLogConfig.LogInputVariables)
            {
                strBuilder.AppendLine("[Input Values]");
                formatTextTable.AddColumnHeaders("Parameter", "Value");
                foreach (ActInputValue actInputValue in act.InputValues)
                {
                    formatTextTable.AddRowValues(actInputValue.ItemName, actInputValue.Value);
                }
                strBuilder.AppendLine(formatTextTable.CreateTable());
                formatTextTable.clear();
            }            

            // log all the output variables
            if (actionLogConfig.LogOutputVariables)
            {
                strBuilder.AppendLine("[Return Values]");
                formatTextTable.AddRowHeader("Parameter", "Expected", "Actual");
                foreach (ActReturnValue actReturnValue in act.ReturnValues)
                {
                    formatTextTable.AddRow(actReturnValue.ItemName, actReturnValue.Expected, actReturnValue.Actual);
                }
                strBuilder.AppendLine(formatTextTable.CreateTable());
                formatTextTable.clear();
            }

            // action status
            if (actionLogConfig.LogRunStatus)
            {
                strBuilder.AppendLine("[Run Status] " + act.Status);
            }

            // action elapsed time
            if (actionLogConfig.LogElapsedTime)
            {
                strBuilder.AppendLine("[Elapsed Time (In Secs)] " + act.ElapsedSecs);
            }

            // action error
            if (actionLogConfig.LogError)
            {
                strBuilder.AppendLine("[Error] " + act.Error);
            }

            // flush value expression
            // flush flow control

            // flush to logger
            logger.LogTimeStamp();
            logger.Log(strBuilder.ToString());

        }


    }
}
