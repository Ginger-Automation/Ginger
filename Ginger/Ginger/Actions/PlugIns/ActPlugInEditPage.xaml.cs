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
using Amdocs.Ginger.Common.Actions;
using Amdocs.Ginger.Repository;
using Ginger.UserControlsLib.ActionInputValueUserControlLib;
using GingerCore;
using GingerCore.Actions.PlugIns;
using GingerPlugIns.ActionsLib;
using System;
using System.Collections.Generic;
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
            AutoCreateEditPage();
        }

        //TODO: destructor is not working , need to fix geneal ActionEditPage
        ~ActPlugInEditPage()
        {
        }

        private void LoadEditPage()
        {
            InputGrid.ItemsSource = mAct.InputValues;
            
            //try
            //{
            //    // Edit Page can be .xaml or a classname of a page, or empty if no config needed                
            //    //WorkSpace.Instance.PluginsManager.GetPluginAction(mAct)
            //    // Plugin PM.GetStandAloneActions()
            //    //string editpage = string.Empty;
            //    if (PIW != null)
            //        editpage = PIW.GetEditPage(mAct.PlugInActionID);

            //    if (string.IsNullOrEmpty(editpage))
            //    {
            //        EditFrame.Visibility = Visibility.Collapsed;
            //        NoConfigLabel.Visibility = Visibility.Visible;
            //        // There is no Edit page configured = action without params in
            //        return;
            //    }

            //    NoConfigLabel.Visibility = Visibility.Collapsed;


            //    if (editpage.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
            //    {
            //        if (!File.Exists(editpage))
            //        {
            //            EditFrame.Content = "Error: xaml file not found - " + editpage;
            //            return;
            //        }

            //        FileStream s = new FileStream(editpage, FileMode.Open);
            //        UIElement rootElement = (UIElement)XamlReader.Load(s);
            //        s.Close();

            //        EditFrame.Content = rootElement;

            //        BindControlsToAction((Panel)rootElement);
            //    }
            //    else
            //    {
            //        Type t = PIW.Assembly.GetType(editpage);
            //        if (t == null)
            //        {
            //            EditFrame.Content = "Error: Action edit page not found - " + editpage;
            //            return;
            //        }

            //        Page p = (Page)Activator.CreateInstance(t, mAct.GingerAction);

            //        foreach (ActionParam AP in mAct.GingerAction.ParamsIn)
            //        {
            //            mAct.AddOrUpdateInputParamValue(AP.Name, AP.Value == null ? string.Empty : AP.Value.ToString());
            //            AP.PropertyChanged -= GingerAction_PropertyChanged;
            //            AP.PropertyChanged += GingerAction_PropertyChanged;
            //        }
            //        EditFrame.Content = p;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LoadErrorLbl.Visibility = Visibility.Visible;
            //    EditFrame.Visibility = Visibility.Collapsed;
            //    Reporter.ToLog(eLogLevel.ERROR, "Failed to load the plugin edit page for the action '" + mAct.Description + "'", ex);
            //}
        }

        private void GingerAction_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (((ActionParam)sender).Value != null)
                mAct.AddOrUpdateInputParamValue(((ActionParam)sender).Name, ((ActionParam)sender).Value.ToString());
        }

        // We Bind the controls on the page to the action input params based on the Xaml object Name - x:Name=
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


        private void AutoCreateEditPage()
        {

            string pluginId = mAct.GetInputParamValue("PluginId");  //TODO: use const
            string serviceId = mAct.GetInputParamValue("ServiceId");  //TODO: use const
            string actionId = mAct.GetInputParamValue("GingerActionId");  //TODO: use const
            PluginIdLabel.Content = pluginId;
            ServiceIdLabel.Content = serviceId;
            ActionIdLabel.Content = actionId;

            int rows = mAct.InputValues.Count;
            for (int i = 0; i < rows; i++)
            {
                ActionConfigGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(35) });
            }

            ActionConfigGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(30, GridUnitType.Star) });
            ActionConfigGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(70, GridUnitType.Star) });

            int rnum = 0;

            List <ActionInputValueInfo> list = WorkSpace.Instance.PlugInsManager.GetActionEditInfo(pluginId, serviceId, actionId);


            foreach (ActInputValue param in mAct.InputValues)
            {                
                if (param.Param == "PluginId" ||  param.Param == "ServiceId" || param.Param == "GingerActionId" || param.Param == "GA")  // TODO: use const
                {
                    continue;
                }

                // update the type based on the info json of the plugin
                param.ParamType = (from x in list where x.Param == param.Param select x.ParamType).SingleOrDefault();

                Label l = new Label() { Content = param.Param };
                ActionConfigGrid.Children.Add(l);
                l.Style = App.GetStyle("@InputFieldLabelStyle");
                Grid.SetRow(l, rnum);

                

                //TODO: based on the param type create textbox, check box, combo, etc...

                ActionInputValueUserControl actionInputValueUserControl = new ActionInputValueUserControl();
                actionInputValueUserControl.BindControl(param);
                actionInputValueUserControl.Margin = new Thickness(5);
                ActionConfigGrid.Children.Add(actionInputValueUserControl);
                Grid.SetRow(actionInputValueUserControl, rnum);
                Grid.SetColumn(actionInputValueUserControl, 1);
                rnum++;
            }
        }
    }
}
