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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Gherkin;
using Gherkin.Ast;
using Ginger.TagsLib;
using GingerCore;
using GingerCore.Activities;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ginger.GherkinLib
{
    public class ScenariosGenerator
    {
        BusinessFlow mBizFlow;

        Dictionary<string, List<OptionalValue>> ValuesDict;        

        public void CreateScenarios(BusinessFlow BF)
        {
            string FileName = string.Empty;
            
            if (BF.ExternalID != null)
                    FileName = BF.ExternalID.Replace(@"~", App.UserProfile.Solution.Folder);

            if (!System.IO.File.Exists(FileName))
            {
                // General
                Reporter.ToUser(eUserMsgKeys.GherkinFileNotFound, FileName);
                return;
            }

            Parser parser = new Parser();
            GherkinDocument gherkinDocument = parser.Parse(FileName);
        
            mBizFlow = BF;
           
            ClearGeneretedActivites(mBizFlow);
            ClearOptimizedScenariosVariables(mBizFlow);

            //Add Tags to BF
            foreach (var t in gherkinDocument.Feature.Tags)
            {
                Guid TagGuid = GetOrCreateTagInSolution(t.Name);
                mBizFlow.Tags.Add(TagGuid);
            }

            foreach (Gherkin.Ast.ScenarioDefinition sc in gherkinDocument.Feature.Children)
            {
                IEnumerable<Examples> examples = null;
                // In case of Scenario Outline we need to generate new BF per each line in the table of examples
                if (sc.Keyword == "Scenario Outline")
                {
                    ScenarioOutline so = (ScenarioOutline)sc;
                    examples = so.Examples;
                }

                // Cretae new BF per each scenario

                if (examples == null)
                {
                    CreateScenario(sc);
                }
                else
                {
                    int i = 0;
                    ValuesDict = new Dictionary<string, List<OptionalValue>>();

                    //TODO: handle case of more than one example table - check what is Gherking expected todo: all combinations!!??
                    foreach (Examples x in examples)
                    {
                        foreach (Gherkin.Ast.TableRow tr in x.TableBody)
                        {
                            i++;
                            ActivitiesGroup AG = CreateScenario(sc, i);
                            // Now the we have the flow created with activities, we update the activities var replacing <param> with value from table
                            var activities = from z in BF.Activities where z.ActivitiesGroupID == AG.Name select z;

                            foreach (Activity a in activities)
                            {
                                while (true)
                                {
                                    string ColName = General.GetStringBetween(a.ActivityName, "<", ">");
                                    if (string.IsNullOrEmpty(ColName)) break;

                                    string val = GetExampleValue(x.TableHeader, tr, ColName);
                                    a.ActivityName = a.ActivityName.Replace("<" + ColName + ">", "\"" + val + "\"");                                   

                                    VariableBase v = a.Variables.Where(y => y.Name == ColName).FirstOrDefault();

                                    OptionalValue ov = new OptionalValue(val);
                                    if (ValuesDict.ContainsKey(ColName))
                                        ValuesDict[ColName].Add(ov);
                                    else
                                    {
                                        List<OptionalValue> newList = new List<OptionalValue>();
                                        newList.Add(ov);
                                        ValuesDict.Add(ColName, newList);
                                    }
                                    ((VariableSelectionList)v).OptionalValuesList.Add(ov);
                                    ((VariableSelectionList)v).SelectedValue = val;
                                }
                            }
                        }
                    }

                    foreach (Activity a in BF.Activities)
                    {
                        foreach (VariableBase vb in a.Variables)
                        {
                            if (vb is VariableSelectionList)
                            {
                                if (ValuesDict.ContainsKey(vb.Name))
                                {
                                    foreach (OptionalValue ov in ValuesDict[vb.Name])
                                    {
                                        OptionalValue ExistedOV = ((VariableSelectionList)vb).OptionalValuesList.Where(y => y.Value == ov.Value).FirstOrDefault();
                                        if (ExistedOV == null)
                                            ((VariableSelectionList)vb).OptionalValuesList.Add(ov);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(NotFoundItems))
                    Reporter.ToUser(eUserMsgKeys.GherkinColumnNotExist, NotFoundItems);
        }

        private void CreateBusinessFlowVar(string varName, string varDescription, string varValue)
        {
            VariableList v = new VariableList();
            v.FileName = varName;
            v.Description = varDescription;
            v.Formula += varValue + ",";

            mBizFlow.Variables.Add(v);
        }
        
        private string NotFoundItems = string.Empty;
        private List<string> NotFoundItemsList = new List<string>();

        private string GetExampleValue(TableRow tableHeader, TableRow tr, string ColName)
        {
            int i = 0;
            foreach (TableCell tc in tableHeader.Cells)
            {
                if (tc.Value == ColName)
                {
                    return tr.Cells.ElementAt(i).Value;
                }
                i++;
            }

            if (NotFoundItemsList.Contains(ColName))
                return string.Empty;

            if (string.IsNullOrEmpty(NotFoundItems))
                NotFoundItems = ColName;
            else
                NotFoundItems += ", " + ColName;

            NotFoundItemsList.Add(ColName);

            return string.Empty;
        }

        public void ClearOptimizedScenariosVariables(BusinessFlow BF)
        {
            foreach (Activity a in BF.Activities)
            {
                foreach (VariableBase vb in a.Variables)
                {
                    if (vb is VariableSelectionList)
                    {
                        ((VariableSelectionList)vb).OptionalValuesList.Clear();
                    }
                }
            }
        }

        public void ClearGeneretedActivites(BusinessFlow BF)
        {

            for (int indx = 0; indx < BF.ActivitiesGroups.Count; indx++)
            {
                if (BF.ActivitiesGroups[indx].ItemName != "Optimized Activities" && BF.ActivitiesGroups[indx].ItemName != "Optimized Activities - Not in Use")
                {
                    BF.ActivitiesGroups.RemoveAt(indx);
                    indx--;
                }
            }

            while (true)
            {
                //TODO: make const for "Optimized Activities"
                var a = (from x in BF.Activities where x.ActivitiesGroupID != "Optimized Activities" && x.ActivitiesGroupID != "Optimized Activities - Not in Use" select x).FirstOrDefault();
                if (a == null) break;
                
                BF.Activities.Remove(a);
            }

            foreach(Activity act in BF.Activities)
            {
                if (act.IsSharedRepositoryInstance == true)
                {
                    ObservableList<Activity> activities = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
                    // FIXME to use (true, "", true, act.Tags)
                    Activity a2 = (from x in activities where x.ActivityName == act.ActivityName select x).FirstOrDefault();
                    if(a2 !=null)
                    {
                        if (a2.Tags != null)
                        {
                            ObservableList<Guid> actTags = new ObservableList<Guid>();
                            foreach (Guid guid in act.Tags)
                                if (a2.Tags.Contains(guid))
                                    actTags.Add(guid);
                            act.Tags = actTags;
                        }
                        else
                            act.Tags.ClearAll();
                    }
                    else
                        act.Tags.ClearAll();
                }                    
                
            }
            BF.Tags.ClearAll();
        }

        private ActivitiesGroup CreateScenario(ScenarioDefinition sc, int? index = null)
        {
            // Create new activity per each step
            // Each scenario is in one ActivitiesGroup
            ActivitiesGroup AG = new ActivitiesGroup();
            AG.Name = sc.Name;
            if (index != null) AG.Name += " #" + index;  // Adding Scenario index so it will be uniqe
            mBizFlow.AddActivitiesGroup(AG);

            //Add Tags - on ActivitiesGroup and solution if needed

            IEnumerable<Tag> TagList = null;
            if (sc is ScenarioOutline)
                TagList = ((ScenarioOutline)sc).Tags;
            else if (sc is Scenario)
                TagList = ((Scenario)sc).Tags;                       

            foreach (var t in TagList)
            {
                Guid tg = GetOrCreateTagInSolution(t.Name);
                AG.Tags.Add(tg);
            }
            foreach(Guid guid in mBizFlow.Tags)
                AG.Tags.Add(guid);
            
            foreach (Gherkin.Ast.Step step in sc.Steps)
            {
                // Find the Acitivity from the tempalte BF with All activity, create a copy and add to BF
                string GN = GherkinGeneral.GetActivityGherkinName(step.Text);
                Activity a = SearchActivityByName(GN);
                if (a != null)
                {                    
                    Activity a1 = (Activity)a.CreateCopy(false);
                    a1.ActivityName = step.Text;                    
                    a1.Active = true;
                    a1.ParentGuid = a1.Guid;
                    Guid g = Guid.NewGuid();
                    a1.Guid = g;
                    foreach (var t in AG.Tags)
                    {
                        if (!a1.Tags.Contains(t))
                            a1.Tags.Add(t);
                        if(!a.Tags.Contains(t))
                            a.Tags.Add(t);
                    }
                    //Adding feature tags
                    foreach (Guid guid in mBizFlow.Tags)
                    {
                        if (!a1.Tags.Contains(guid))
                            a1.Tags.Add(guid);
                        if (!a.Tags.Contains(guid))
                            a.Tags.Add(guid);
                    }
                        

                    UpdateActivityVars(a1, a.ActivityName);
                    AG.AddActivityToGroup(a1);
                    mBizFlow.AddActivity(a1);
                }
                else
                {

                    //TODO: err activity not found...
                    Reporter.ToUser(eUserMsgKeys.GherkinActivityNotFound, GN);
                }
            }
            return AG;
        }

        private Guid GetOrCreateTagInSolution(string TagName)
        {
            if (TagName.StartsWith("@")) TagName = TagName.Substring(1);
            Guid TagGuid = (from x in App.UserProfile.Solution.Tags where x.Name == TagName select x.Guid).FirstOrDefault();
            if (TagGuid == Guid.Empty)
            {
                //TODO: notify the user that tags are added to solution and he needs to save it                    
                RepositoryItemTag RIT = new RepositoryItemTag() { Name = TagName };
                TagGuid = RIT.Guid;
                App.UserProfile.Solution.Tags.Add(RIT);
            }
            return TagGuid;
        }

        private Activity SearchActivityByName(string Name)
        {
            // First we search in Shared Repo if not found we try the BF
            Activity a = null;
            if (a == null)
            {
                //TODO: need to search only in the optimzied Acitivites group
                a = (from x in mBizFlow.Activities where x.ActivityName == Name select x).FirstOrDefault();
            }
            return a;
        }

        private void UpdateActivityVars(Activity a, string Name)
        {
            if (a.Variables.Count == 0) return;
            string s = Name;
            int i = 0;
            while (s.Contains('%'))
            {
                i++;
                //TODO: user const for %p everywhere
                string num = General.GetStringBetween(s, "%p", " ");
                // in case of EOL then there might not be space, so we search from %p until end of line
                if (num == "")
                {
                    int ii = s.IndexOf("%p");
                    num = s.Substring(ii + 2);
                }
                s = s.Replace("%p" + i, "");
                VariableString v = (VariableString)a.GetVariable("p" + i);
                v.InitialStringValue = GetParamByIndex(a.ActivityName, int.Parse(num));

            }
        }

        string GetParamByIndex(string s, int idx)
        {
            int i = 0;
            while (s.IndexOf("\"") > 0)
            {
                i++;
                string val = General.GetStringBetween(s, "\"", "\"");
                s = s.Replace('"' + val + '"', "");
                if (i == idx) return val;
            }
            return null;
        }
    }
}
