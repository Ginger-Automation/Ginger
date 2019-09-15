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

using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Amdocs.Ginger.Common.GeneralLib;
using GingerCore;
using Newtonsoft.Json;

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
        public bool ShowSocketMonitor { get; set; }

        [JsonProperty]
        public bool ShowTimings { get; set; }


        // Envs
        //public bool ShowNewEnvironmentPage { get { return GetFeature(nameof(ShowNewEnvironmentPage)).Selected; } set { UpdateFeature(nameof(ShowNewEnvironmentPage), value); } }

        // public bool SaveEnvironmentUsingSR2 { get { return GetFeature(nameof(SaveEnvironmentUsingSR2)).Selected; } set { UpdateFeature(nameof(SaveEnvironmentUsingSR2), value); } }

        //BFs        
        public bool BFExportToJava { get { return GetFeature(nameof(BFExportToJava)).Selected; } set { UpdateFeature(nameof(BFExportToJava), value); } }
        public bool BFPageActivitiesHookOnlyNewActivities { get { return GetFeature(nameof(BFPageActivitiesHookOnlyNewActivities)).Selected; } set { UpdateFeature(nameof(BFPageActivitiesHookOnlyNewActivities), value); } }



        // POM
        public bool ShowPOMInWindowExplorer { get { return GetFeature(nameof(ShowPOMInWindowExplorer)).Selected; } set { UpdateFeature(nameof(ShowPOMInWindowExplorer), value); } }


        // ALM
        public bool Rally { get { return GetFeature(nameof(Rally)).Selected; } set { UpdateFeature(nameof(Rally), value); } }

        //Gherkin
        public bool ImportGherkinFeatureWizrd { get { return GetFeature(nameof(ImportGherkinFeatureWizrd)).Selected; } set { UpdateFeature(nameof(ImportGherkinFeatureWizrd), value); } }

        // CDL
        public bool ShowCDL { get { return GetFeature(nameof(ShowCDL)).Selected; } set { UpdateFeature(nameof(ShowCDL), value); } }



        public BetaFeatures()
        {
            // Env
            // mFeatures.Add(new BetaFeature() { Group = "Environments", Description= "Show new environments Page in Reosurce tab",   ID = nameof(ShowNewEnvironmentPage), Warning = "Using Solution Repository", Selected = false });
            // Temp comment when ready 
            // mFeatures.Add(new BetaFeature() { Group = "Environments",Description = "Save Environment Using SR2",   ID = nameof(SaveEnvironmentUsingSR2), Warning = "zzz" });            

            //BFs

            mFeatures.Add(new BetaFeature() { Group = GingerDicser.GetTermResValue(eTermResKey.BusinessFlows), Description = "Export BF to Java menu item", ID = nameof(BFExportToJava) });
            mFeatures.Add(new BetaFeature() { Group = GingerDicser.GetTermResValue(eTermResKey.BusinessFlows), Description = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " " + GingerDicser.GetTermResValue(eTermResKey.Activities) + "page hook only new " + GingerDicser.GetTermResValue(eTermResKey.Activities) + "- speed", ID = nameof(BFPageActivitiesHookOnlyNewActivities) });

            // POM
            mFeatures.Add(new BetaFeature() { Group = "POM", Description = "Show POM in Window Explorer", ID = nameof(ShowPOMInWindowExplorer) });

            //ALM
            mFeatures.Add(new BetaFeature() { Group = "ALM", Description = "Show Rally", ID = nameof(Rally) });

            //Gherkin
            mFeatures.Add(new BetaFeature() { Group = "Gherkin", Description = "Import Gherkin feature wizard", ID = nameof(ImportGherkinFeatureWizrd) });

            //Repository
            // mFeatures.Add(new BetaFeature() { Group = "Repository", Description = "Use Solution Repository instead of LocalRepository", ID = nameof(Use Solution Repository instead of LocalRepository), Warning = "Will reload solution" });
            // mFeatures.Add(new BetaFeature() { Group = "Repository", Description = "Use New Repository Serializer", ID = nameof(UseNewRepositorySerializer), Warning = "Will reload solution" });

            //CDL            
            mFeatures.Add(new BetaFeature() { Group = "CDL", Description = "Show CDL - Change Definition Language", ID = nameof(ShowCDL) });

            //hook prop change
            foreach (BetaFeature f in mFeatures)
            {
                f.PropertyChanged += SelectionChanged;
            }
        }

        private void SelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
            OnPropertyChanged(nameof(IsUsingBetaFeatures));
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
                foreach (BetaFeature f in mFeatures)
                {
                    if (f.Selected)
                    {
                        return true;
                    }
                }
                return false;
            }
        }


        public IEnumerable Features { get { return mFeatures; } }

        static string UserBetaFeaturesConfigFilePath
        {
            get
            {
                return Path.Combine(General.LocalUserApplicationDataFolderPath, "Ginger Beta Feature Config.json");
            }
        }

        public static BetaFeatures LoadUserPref()
        {
            // always create new so we get latest beta features to select from
            BetaFeatures betaFeatures = new BetaFeatures();

            if (System.IO.File.Exists(UserBetaFeaturesConfigFilePath))
            {
                // Read user selection and merge with updated feature list
                string s = System.IO.File.ReadAllText(UserBetaFeaturesConfigFilePath);
                BetaFeatures bUser = Newtonsoft.Json.JsonConvert.DeserializeObject<BetaFeatures>(s);

                betaFeatures.ShowDebugConsole = bUser.ShowDebugConsole;
                betaFeatures.ShowTimings = bUser.ShowTimings;
                betaFeatures.ShowSocketMonitor = bUser.ShowSocketMonitor;

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
            System.IO.File.WriteAllText(UserBetaFeaturesConfigFilePath, s);
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
            Console.WriteLine("ShowSocketMonitor=" + ShowSocketMonitor);
            foreach (BetaFeature f in mFeatures)
            {
                Console.WriteLine(f.Description + " = " + f.Selected);
            }

            Console.WriteLine("-------------------------------------------------------------------------------");
        }
    }

}
