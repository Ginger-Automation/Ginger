#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Amdocs.Ginger.Run;
using Ginger.Reports;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Run.RunListenerLib
{
       public class ExecutionProgressReporterListener :  RunListenerBase
    {
        public enum eExecutionPhase
        {
            Start,
            End
        }

        /// <summary>
        ///  udpate in BF start !!!!!!!!!!!!!!!!!!!!!!!
        /// </summary>
        BusinessFlow mBusinessFlow;
       
        public override void RunnerRunStart(uint eventTime, GingerRunner gingerRunner)
        {
            AddExecutionDetailsToLog(eExecutionPhase.Start, "Runner", gingerRunner.Name, null);
        }

        public override void RunnerRunEnd(uint eventTime, GingerRunner gingerRunner, string filename = null, int runnerCount = 0)
        {
            AddExecutionDetailsToLog(eExecutionPhase.End, "Runner", gingerRunner.Name, new GingerReport());
        }

        public override void BusinessFlowStart(uint eventTime, BusinessFlow businessFlow, bool ContinueRun = false)
        {
            mBusinessFlow = businessFlow;
            AddExecutionDetailsToLog(eExecutionPhase.Start, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), businessFlow.Name, new BusinessFlowReport(businessFlow));
        }

        public override void BusinessFlowEnd(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false)
        {
            AddExecutionDetailsToLog(eExecutionPhase.End, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), businessFlow.Name, new BusinessFlowReport(businessFlow));
        }

        public override void ActivityGroupStart(uint eventTime, ActivitiesGroup activityGroup)
        {
            AddExecutionDetailsToLog(eExecutionPhase.Start, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), activityGroup.Name, new ActivityGroupReport(activityGroup, mBusinessFlow));            
        }

        public override void ActivityGroupEnd(uint eventTime, ActivitiesGroup activityGroup)
        {
            AddExecutionDetailsToLog(eExecutionPhase.End, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), activityGroup.Name, new ActivityGroupReport(activityGroup, mBusinessFlow));
        }

        public override void ActivityStart(uint eventTime, Activity activity, bool continuerun = false)
        {                        
            AddExecutionDetailsToLog(eExecutionPhase.Start, GingerDicser.GetTermResValue(eTermResKey.Activity), activity.ActivityName, new ActivityReport(activity));
        }

        public override void ActivityEnd(uint eventTime, Activity activity, bool offlineMode= false)
        {
            AddExecutionDetailsToLog(eExecutionPhase.End, GingerDicser.GetTermResValue(eTermResKey.Activity), activity.ActivityName, new ActivityReport(activity));
        }

        public override void ActionStart(uint eventTime, Act action)
        {
            AddExecutionDetailsToLog(eExecutionPhase.Start, "Action", action.Description, new ActionReport(action));
        }

        public override void ActionEnd(uint eventTime, Act action, bool offlineMode=false)
        {
            AddExecutionDetailsToLog(eExecutionPhase.End, "Action", action.Description, new ActionReport(action));
        }

        public static void AddExecutionDetailsToLog(eExecutionPhase objExecutionPhase, string objType, string objName, object obj)
        {
            if (Reporter.AppLoggingLevel == eAppReporterLoggingLevel.Debug || Reporter.ReportAllAlsoToConsole == true)//needed for not derlling the objects if not needed to be reported
            {
                string prefix = string.Empty;

                StringBuilder stringBuilder = new StringBuilder();                
                switch (objExecutionPhase)
                {
                    case eExecutionPhase.Start:
                        stringBuilder.Append("--> ").Append(objType + " Execution Started");                        
                        break;
                    case eExecutionPhase.End:                        
                        stringBuilder.Append("<-- ").Append(objType + " Execution Ended");
                        break;
                }
                stringBuilder.Append(": '").Append(objName).Append("'").AppendLine();

                //get the execution fields and their values
                if (objExecutionPhase == eExecutionPhase.End && obj != null)
                {
                    stringBuilder.Append("Details:").AppendLine();
                    try
                    {
                        PropertyInfo[] props = obj.GetType().GetProperties();
                        foreach (PropertyInfo prop in props)
                        {
                            try
                            {
                                FieldParamsFieldType attr = ((FieldParamsFieldType)prop.GetCustomAttribute(typeof(FieldParamsFieldType)));
                                if (attr == null)
                                {
                                    continue;
                                }
                                FieldsType ftype = attr.FieldType;
                                if (ftype == FieldsType.Field)
                                {
                                    string propName = prop.Name;
                                    string propFullName = ((FieldParamsNameCaption)prop.GetCustomAttribute(typeof(FieldParamsNameCaption))).NameCaption;
                                    object propVal = prop.GetValue(obj);//obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance).GetValue(obj).ToString();
                                    string propValue;
                                    if (propVal != null)
                                    {
                                        propValue = propVal.ToString();
                                    }
                                    else
                                    {
                                        propValue = "";
                                    }

                                    stringBuilder.Append(propFullName).Append("= ").Append(propValue).AppendLine();
                                }
                            }
                            catch (Exception) { }                            
                        }
                    }
                    catch (Exception) { }                  
                }

                Reporter.ToLog(eLogLevel.DEBUG, stringBuilder.ToString());
            }
        }

    }
}
