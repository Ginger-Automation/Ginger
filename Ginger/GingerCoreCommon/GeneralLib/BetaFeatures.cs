﻿#region License
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

using Amdocs.Ginger.Common.GeneralLib;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Common
{
    
    [JsonObject(MemberSerialization.OptIn)]    
    public class BetaFeatures : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [JsonProperty]
        ObservableList<BetaFeature> mFeatures = new ObservableList<BetaFeature>();

        // General flags

        [JsonProperty]
        public bool ShowDebugConsole { get; set; }
        [JsonProperty]
        public bool ShowTimings { get; set; }


        // Envs
        //public bool ShowNewEnvironmentPage { get { return GetFeature(nameof(ShowNewEnvironmentPage)).Selected; } set { UpdateFeature(nameof(ShowNewEnvironmentPage), value); } }

        // public bool SaveEnvironmentUsingSR2 { get { return GetFeature(nameof(SaveEnvironmentUsingSR2)).Selected; } set { UpdateFeature(nameof(SaveEnvironmentUsingSR2), value); } }

        //BFs
        public bool BFUseSolutionRepositry { get { return GetFeature(nameof(BFUseSolutionRepositry)).Selected; } set { UpdateFeature(nameof(BFUseSolutionRepositry), value); } }                
        public bool BFExportToJava { get { return GetFeature(nameof(BFExportToJava)).Selected; } set { UpdateFeature(nameof(BFExportToJava), value); } }
        public bool BFPageActivitiesHookOnlyNewActivities { get { return GetFeature(nameof(BFPageActivitiesHookOnlyNewActivities)).Selected; } set { UpdateFeature(nameof(BFPageActivitiesHookOnlyNewActivities), value); } }

        // POM
        public bool ShowPOMInWindowExplorer { get { return GetFeature(nameof(ShowPOMInWindowExplorer)).Selected; } set { UpdateFeature(nameof(ShowPOMInWindowExplorer), value); } }
        public bool ShowPOMInResourcesTab{ get { return GetFeature(nameof(ShowPOMInResourcesTab)).Selected; } set { UpdateFeature(nameof(ShowPOMInResourcesTab), value); } }


        // ALM
        public bool Rally { get { return GetFeature(nameof(Rally)).Selected; } set { UpdateFeature(nameof(Rally), value); } }
        public bool RestAPI { get { return GetFeature(nameof(RestAPI)).Selected; } set { UpdateFeature(nameof(RestAPI), value); } }

        //Gherkin
        public bool ImportGherkinFeatureWizrd { get { return GetFeature(nameof(ImportGherkinFeatureWizrd)).Selected; } set { UpdateFeature(nameof(ImportGherkinFeatureWizrd), value); } }


        // Repository
        public bool UseNewRepositorySerializer { get { return GetFeature(nameof(UseNewRepositorySerializer)).Selected; } set { UpdateFeature(nameof(UseNewRepositorySerializer), value); } }


        // CDL
        public bool ShowCDL { get { return GetFeature(nameof(ShowCDL)).Selected; } set { UpdateFeature(nameof(ShowCDL), value); } }


        // CDL
        public bool ShowNewautomate { get { return GetFeature(nameof(ShowNewautomate)).Selected; } set { UpdateFeature(nameof(ShowNewautomate), value); } }

        

        public BetaFeatures()
        {
            // Env
            // mFeatures.Add(new BetaFeature() { Group = "Environments", Description= "Show new environments Page in Reosurce tab",   ID = nameof(ShowNewEnvironmentPage), Warning = "Using Solution Repository", Selected = false });
            // Temp comment when ready 
            // mFeatures.Add(new BetaFeature() { Group = "Environments",Description = "Save Environment Using SR2",   ID = nameof(SaveEnvironmentUsingSR2), Warning = "zzz" });            
            
            //BFs
            mFeatures.Add(new BetaFeature() { Group = "Business Flows", Description = "BFs using Solution Repository", ID = nameof(BFUseSolutionRepositry), Warning = "Will reload solution" });
            mFeatures.Add(new BetaFeature() { Group = "Business Flows", Description = "Export BF to Java menu item", ID = nameof(BFExportToJava)});
            mFeatures.Add(new BetaFeature() { Group = "Business Flows", Description = "BF Activities page hook only new activities - speed", ID = nameof(BFPageActivitiesHookOnlyNewActivities) });

            // POM
            mFeatures.Add(new BetaFeature() { Group = "POM", Description = "Show POM in Window Explorer", ID = nameof(ShowPOMInWindowExplorer)});
            mFeatures.Add(new BetaFeature() { Group = "POM", Description = "Show POM in Resources Tab", ID = nameof(ShowPOMInResourcesTab)});

            //ALM
            mFeatures.Add(new BetaFeature() { Group = "ALM", Description = "Show Rally", ID = nameof(Rally) });
            mFeatures.Add(new BetaFeature() { Group = "ALM", Description = "Show REST API", ID = nameof(RestAPI) });

            //Gherkin
            mFeatures.Add(new BetaFeature() { Group = "Gherkin", Description = "Import Gherkin feature wizard", ID = nameof(ImportGherkinFeatureWizrd)});

            //Repository
            // mFeatures.Add(new BetaFeature() { Group = "Repository", Description = "Use Solution Repository instead of LocalRepository", ID = nameof(Use Solution Repository instead of LocalRepository), Warning = "Will reload solution" });
            mFeatures.Add(new BetaFeature() { Group = "Repository", Description = "Use New Repository Serializer", ID = nameof(UseNewRepositorySerializer), Warning = "Will reload solution" });

            //CDL            
            mFeatures.Add(new BetaFeature() { Group = "CDL", Description = "Show CDL - Change Definition Language", ID = nameof(ShowCDL) });

            //New Automate
            mFeatures.Add(new BetaFeature() { Group = "Automnate", Description = "Show new automate Ribbon", ID = nameof(ShowNewautomate) });


            //hook prop change
            foreach (BetaFeature f in mFeatures)
            {
                f.PropertyChanged += selectionChanged;
            }
        }

        private void selectionChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
            OnPropertyChanged(nameof(UsingStatus));
        }

        

        BetaFeature GetFeature(string name)
        {            
            BetaFeature f = (from x in mFeatures where x.ID == name select x).SingleOrDefault();            
            return f;
        }

        void UpdateFeature(string name, bool value)
        {
            BetaFeature f = (from x in mFeatures where x.ID == name select x).SingleOrDefault();                        
            f.Selected = value;
            OnPropertyChanged(nameof(name));            
        }


        public bool IsUsingBetaFeatures
        {
            get
            {
                foreach(BetaFeature f in mFeatures)
                {
                    if (f.Selected)
                    {
                        return true;
                    }
                }                
                return false;                
            }
        }

        public string UsingStatus
        {
            get
            {
                if (IsUsingBetaFeatures)
                {
                    return "with Beta Features...";
                }
                else
                {
                    return "";
                }
            }
        }

        

        public IEnumerable Features { get { return mFeatures; } }

        static string UserPrefFileName()
        {
            string s = Path.Combine(Environment.CurrentDirectory, "Ginger Beta Feature Config.json");
            return s;
        }

        public static BetaFeatures LoadUserPref()
        {
            // always create new so we get latest beta features to select from
            BetaFeatures betaFeatures = new BetaFeatures();
            
            if (System.IO.File.Exists(UserPrefFileName()))
            {
                // Read user selection and merge with updated feature list
                string s = System.IO.File.ReadAllText(UserPrefFileName());
                BetaFeatures bUser = Newtonsoft.Json.JsonConvert.DeserializeObject<BetaFeatures>(s);

                betaFeatures.ShowDebugConsole = bUser.ShowDebugConsole;
                betaFeatures.ShowTimings = bUser.ShowTimings;

                foreach (BetaFeature f in bUser.mFeatures)
                {
                    BetaFeature bf = (from x in betaFeatures.mFeatures where x.ID == f.ID select x).SingleOrDefault();
                    if (bf != null)  // if null we just ignore - probably feature doesn't exist any more as it it is in release
                    {
                        bf.Selected = f.Selected;
                    }
                }
                
            }            
            return betaFeatures;
        }

        public void SaveUserPref()
        {
            string s = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            System.IO.File.WriteAllText(UserPrefFileName(), s);
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public void DisplayStatus()
        {
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine("Beta Features Status:");

            Console.WriteLine("ShowDebugConsole=" + ShowDebugConsole);
            Console.WriteLine("ShowTimings=" + ShowTimings);
            foreach (BetaFeature f in mFeatures)
            {
                Console.WriteLine(f.Description + " = " + f.Selected);
            }
            
            Console.WriteLine("-------------------------------------------------------------------------------");
        }
    }

}
