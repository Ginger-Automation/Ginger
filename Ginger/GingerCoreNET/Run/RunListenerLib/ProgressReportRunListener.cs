using Amdocs.Ginger.Common;
using Amdocs.Ginger.Run;
using GingerCore;
using GingerCore.Activities;

namespace Amdocs.Ginger.CoreNET.Run.RunListenerLib
{
    public class ProgressReportRunListener :  RunListenerBase
    {

        
        // private static void AddExecutionDetailsToLog(eExecutionPahse objExecutionPhase, string objType, string objName, object obj)
        //{
        /*  if (AppReporter.CurrentAppLogLevel == eAppReporterLoggingLevel.Debug)
          {
              string prefix = string.Empty;
              switch (objExecutionPhase)
              {
                  case eExecutionPahse.Start:
                      prefix = "--> Execution Started for the " + objType + ": '" + objName + "'";
                      break;
                  case eExecutionPahse.End:
                      prefix = "<-- Execution Ended for the " + objType + ": '" + objName + "'";
                      break;
              }

              //get the execution fields and their values
              if (obj != null)
              {
                  List<KeyValuePair<string, string>> fieldsAndValues = new List<KeyValuePair<string, string>>();
                  try
                  {
                      PropertyInfo[] props = obj.GetType(.GetProperties();
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
                                  string propFullName = ((FieldParamsNameCaption)prop.GetCustomAttribute(typeof(FieldParamsNameCaption)).NameCaption;
                                  string propValue = obj.GetType(.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance.GetValue(obj.ToString();
                                  fieldsAndValues.Add(new KeyValuePair<string, string>(propFullName, propValue));
                              }
                          }
                          catch (Exception)
                          {
                              //TODO: !!!!!!!!!!!!!!!!!! FIXME
                          }
                      }
                  }
                  catch (Exception)
                  {
                      //TODO: !!!!!!!!!!!!!!!!!! FIXME
                  }

                  //add to Console
                  string details = string.Empty;
                  foreach (KeyValuePair<string, string> det in fieldsAndValues)
                      details += det.Key + "= " + det.Value + System.Environment.NewLine;
                  Reporter.ToLog(eAppReporterLogLevel.INFO, prefix + System.Environment.NewLine + "Details:" + System.Environment.NewLine + details);
              }
              else
              {
                  Reporter.ToLog(eAppReporterLogLevel.INFO, prefix + System.Environment.NewLine);
              }
          }
         */
        //}
        public override void ActivityGroupStart(uint eventTime, ActivitiesGroup activityGroup)
        {
            Reporter.ToLog(eLogLevel.INFO, "ActivityGroup Started: " + activityGroup.Name);
        }

        public override void ActivityGroupEnd(uint eventTime, ActivitiesGroup activityGroup)
        {
            Reporter.ToLog(eLogLevel.INFO, "ActivityGroup End: " + activityGroup.Name);
        }

        public override void ActivityStart(uint eventTime, Activity activity)
        {            
            Reporter.ToLog(eLogLevel.INFO, GingerDicser.GetTermResValue(eTermResKey.Activity) + " Started: " + activity.ActivityName);
        }

        public override void ActivityEnd(uint eventTime, Activity activity)
        {
            Reporter.ToLog(eLogLevel.INFO, GingerDicser.GetTermResValue(eTermResKey.Activity) + " End: " + activity.ActivityName);
        }
    }
}
