using Amdocs.Ginger.Common.Actions;
using Amdocs.Ginger.CoreNET.GeneralLib;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using System;
using System.Collections;
using System.Text;

namespace Ginger.Run
{
    public class GingerRunnerLogger
    {
        string fileName; 

        public GingerRunnerLogger(string loggerFile)
        {
            this.fileName = loggerFile;
        }

        public void LogAction(Act logAction)
        {
            if (!logAction.EnableActionLogConfig) return;

            StringBuilder strBuilder = new StringBuilder();
            ActionLogConfig actionLogConfig = logAction.ActionLogConfig;
            FormatTextTable formatTextTable = new FormatTextTable();

            // log timestamp
            strBuilder.AppendLine(GetCurrentTimeStampHeader());

            // create a new log file if not exists and append the contents
            strBuilder.AppendLine("[Action] " + logAction.ActionDescription);
            strBuilder.AppendLine("[Text] " + actionLogConfig.ActionLogText);

            // log all the input values
            if (actionLogConfig.LogInputVariables)
            {
                strBuilder.AppendLine("[Input Values]");
                formatTextTable = new FormatTextTable();

                ArrayList colHeaders = new ArrayList() { "Parameter", "Value" };
                formatTextTable.AddRowHeader(colHeaders);

                foreach (ActInputValue actInputValue in logAction.InputValues)
                {
                    ArrayList colValues = new ArrayList() { actInputValue.ItemName, actInputValue.Value };
                    formatTextTable.AddRowValues(colValues);
                }
                strBuilder.AppendLine(formatTextTable.FormatLogTable());
            }

            // log all the output variables
            if (actionLogConfig.LogOutputVariables)
            {
                strBuilder.AppendLine("[Return Values]");
                formatTextTable = new FormatTextTable();

                ArrayList colHeaders = new ArrayList() { "Parameter", "Expected", "Actual" };
                formatTextTable.AddRowHeader(colHeaders);

                foreach (ActReturnValue actReturnValue in logAction.ReturnValues)
                {
                    ArrayList colValues = new ArrayList() { actReturnValue.ItemName, actReturnValue.Expected, actReturnValue.Actual };
                    formatTextTable.AddRowValues(colValues);
                }
                strBuilder.AppendLine(formatTextTable.FormatLogTable());
            }

            // action status
            if (actionLogConfig.LogRunStatus)
            {
                strBuilder.AppendLine("[Run Status] " + logAction.Status);
            }

            // action elapsed time
            if (actionLogConfig.LogElapsedTime)
            {
                strBuilder.AppendLine("[Elapsed Time (In Secs)] " + logAction.ElapsedSecs);
            }

            // action error
            if (actionLogConfig.LogError)
            {
                strBuilder.AppendLine("[Error] " + logAction.Error);
            }

            // flush value expression
            // flush flow control

            // flush to log file
            FlushToLogFile(strBuilder.ToString());
        }

        private string GetCurrentTimeStampHeader()
        {
            StringBuilder sbr = new StringBuilder();
            sbr.AppendLine("-------------------------------");
            sbr.AppendLine(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());
            sbr.AppendLine("-------------------------------");
            return sbr.ToString();
        }

        private void FlushToLogFile(string strContents)
        {
            System.IO.File.AppendAllText(fileName, strContents);
        }

    }
}
