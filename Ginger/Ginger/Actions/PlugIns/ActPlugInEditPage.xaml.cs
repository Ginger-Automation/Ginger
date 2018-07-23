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

using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions.PlugIns;
using GingerPlugIns.ActionsLib;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Ginger.Actions.PlugIns
{
    /// <summary>
    /// Interaction logic for ActPlugInEditPage.xaml
    /// </summary>
    public partial class ActPlugInEditPage : Page
    {
        ActPlugIn mAct;
        
        public ActPlugInEditPage(ActPlugIn act)
        {
            InitializeComponent();
            mAct = act;
            LoadEditPage();
        }

        //TODO: destructor is not working , need to fix geneal ActionEditPage
        ~ActPlugInEditPage()
        {
        }

        private void LoadEditPage()
        {
            try
            {
                // Edit Page can be .xaml or a classname of a page, or empty if no config needed
                PlugInWrapper PIW = App.LocalRepository.GetSolutionPlugIns().Where(x => x.ID == mAct.PlugInID).FirstOrDefault();
                string editpage = string.Empty;
                if (PIW != null)
                    editpage = PIW.GetEditPage(mAct.PlugInActionID);

                if (string.IsNullOrEmpty(editpage))
                {
                    EditFrame.Visibility = Visibility.Collapsed;
                    NoConfigLabel.Visibility = Visibility.Visible;
                    // There is no Edit page configured = action without params in
                    return;
                }

                NoConfigLabel.Visibility = Visibility.Collapsed;


                if (editpage.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
                {
                    if (!File.Exists(editpage))
                    {
                        EditFrame.Content = "Error: xaml file not found - " + editpage;
                        return;
                    }

                    FileStream s = new FileStream(editpage, FileMode.Open);
                    UIElement rootElement = (UIElement)XamlReader.Load(s);
                    s.Close();

                    EditFrame.Content = rootElement;

                    BindControlsToAction((Panel)rootElement);
                }
                else
                {
                    Type t = PIW.Assembly.GetType(editpage);
                    if (t == null)
                    {
                        EditFrame.Content = "Error: Action edit page not found - " + editpage;
                        return;
                    }

                    Page p = (Page)Activator.CreateInstance(t, mAct.GingerAction);

                    foreach (ActionParam AP in mAct.GingerAction.ParamsIn)
                    {
                        mAct.AddOrUpdateInputParamValue(AP.Name, AP.Value == null? string.Empty : AP.Value.ToString());
                        AP.PropertyChanged -= GingerAction_PropertyChanged;
                        AP.PropertyChanged += GingerAction_PropertyChanged;
                    }
                    EditFrame.Content = p;
                }
            }
            catch(Exception ex)
            {
                LoadErrorLbl.Visibility = Visibility.Visible;
                EditFrame.Visibility = Visibility.Collapsed;
                Reporter.ToLog(eLogLevel.ERROR, "Failed to load the plugin edit page for the action '" + mAct.Description + "'", ex);
            }
        }

        private void GingerAction_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (((ActionParam)sender).Value != null)
                mAct.AddOrUpdateInputParamValue(((ActionParam)sender).Name, ((ActionParam)sender).Value.ToString());
        }

        // We Bind the controls on the page to the action input params based ont he Xaml object Name - x:Name=
        //this is recursive function which drill down when needed
        private void BindControlsToAction(Panel container)
        {         
            foreach (UIElement e in container.Children)  
            {
                // recursive bind for example for StackPanle or other controls Panel
                if (e is Panel) BindControlsToAction((Panel)e);

                string name = (string)e.GetValue(NameProperty);
                // We bind control based on their name
                if (!string.IsNullOrEmpty(name))
                {                    
                    ActInputValue v = mAct.GetOrCreateInputParam(name);

                    //Bind a text box
                    App.ObjFieldBinding((Control)e, TextBox.TextProperty, v, "Value");

                    //TODO: check control type and bind per type
                }
            }
        }
    }
}
