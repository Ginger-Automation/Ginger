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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Actions;
using Amdocs.Ginger.Repository;
using Ginger.UserControlsLib.ActionInputValueUserControlLib;
using GingerCore.Actions.PlugIns;
using System.Collections.Generic;

using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

            InputGrid.ItemsSource = mAct.InputValues;//TODO: Hide it

            SetActionInputsControls();
        }

        //TODO: destructor is not working , need to fix general ActionEditPage
        ~ActPlugInEditPage()
        {
        }

        private void SetActionInputsControls()
        {
            List<ActionInputValueInfo> actionInputsDetails = WorkSpace.Instance.PlugInsManager.GetActionEditInfo(mAct.PluginId, mAct.ServiceId, mAct.ActionId);

            foreach (ActInputValue param in mAct.InputValues)
            {
                ActionInputValueInfo actionInputValueInfo = (from x in actionInputsDetails where x.Param == param.Param select x).SingleOrDefault();
                // update the type based on the info json of the plugin
                param.ParamType = actionInputValueInfo.ParamType;
                
                // Add ActionInputValueUserControl for the param value to edit
                ActionInputValueUserControl actionInputValueUserControl = new ActionInputValueUserControl(Context.GetAsContext(mAct.Context), param, actionInputValueInfo.ParamAttrs);
                DockPanel.SetDock(actionInputValueUserControl, Dock.Top);
                actionInputValueUserControl.Margin = new Thickness(0,10,0,0);
                xActionInputControlsPnl.Children.Add(actionInputValueUserControl);               
            }
        }


        //private void GingerAction_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (((ActionParam)sender).Value != null)
        //        mAct.AddOrUpdateInputParamValue(((ActionParam)sender).Name, ((ActionParam)sender).Value.ToString());
        //}

        // We Bind the controls on the page to the action input params based on the Xaml object Name - x:Name=
        //this is recursive function which drill down when needed
        //private void BindControlsToAction(Panel container)
        //{         
        //    foreach (UIElement e in container.Children)  
        //    {
        //        // recursive bind for example for StackPanle or other controls Panel
        //        if (e is Panel) BindControlsToAction((Panel)e);

        //        string name = (string)e.GetValue(NameProperty);
        //        // We bind control based on their name
        //        if (!string.IsNullOrEmpty(name))
        //        {                    
        //            ActInputValue v = mAct.GetOrCreateInputParam(name);

        //            //Bind a text box
        //            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding((Control)e, TextBox.TextProperty, v, "Value");

        //            //TODO: check control type and bind per type
        //        }
        //    }
        //}


        //private void SetActionInputsControls()
        //{

        //    string pluginId = mAct.PluginId;
        //    string serviceId = mAct.ServiceId;
        //    string actionId = mAct.ActionId;
        //    PluginIdLabel.Content = pluginId;
        //    ServiceIdLabel.Content = serviceId;
        //    ActionIdLabel.Content = actionId;

        //    int rows = mAct.InputValues.Count;
        //    for (int i = 0; i < rows; i++)
        //    {
        //        ActionConfigGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(35, GridUnitType.Auto) });
        //    }

        //    ActionConfigGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(30, GridUnitType.Star) });
        //    ActionConfigGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(70, GridUnitType.Star) });

        //    int rnum = 0;

        //    List <ActionInputValueInfo> list = WorkSpace.Instance.PlugInsManager.GetActionEditInfo(pluginId, serviceId, actionId);


        //    foreach (ActInputValue param in mAct.InputValues)
        //    {                               

        //        // update the type based on the info json of the plugin
        //        param.ParamType = (from x in list where x.Param == param.Param select x.ParamType).SingleOrDefault();
        //        string labelText = char.ToUpper(param.Param[0]) + param.Param.Substring(1); // Make first letter upper case
        //        Label l = new Label() { Content = labelText };
        //        ActionConfigGrid.Children.Add(l);
        //        l.Style = App.GetStyle("@InputFieldLabelStyle");
        //        Grid.SetRow(l, rnum);

        //        // Add ActionInputValueUserControl for the param value to edit
        //        ActionInputValueUserControl actionInputValueUserControl = new ActionInputValueUserControl();
        //        actionInputValueUserControl.BindControl(param);
        //        actionInputValueUserControl.Margin = new Thickness(5);
        //        ActionConfigGrid.Children.Add(actionInputValueUserControl);
        //        Grid.SetRow(actionInputValueUserControl, rnum);
        //        Grid.SetColumn(actionInputValueUserControl, 1);
        //        rnum++;
        //    }
        //}
    }
}
