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
using Amdocs.Ginger.Repository;
using Ginger.UserControlsLib.ActionInputValueUserControlLib;
using GingerCore.Actions;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.ActionLib
{
    /// <summary>
    /// Interaction logic for ActionConfigPage.xaml
    /// </summary>
    public partial class ActionConfigPage : Page
    {
        Act mAct;
        public ActionConfigPage(Act act)
        {
            InitializeComponent();
            mAct = act;
            LoadEditPage();            
        }

        private void LoadEditPage()
        {
            // we can have several types of Edit page
            // 1. default grid - created automatically if action didn't define edit page
            // 2. Simple Xaml only no code behind layout only - will load and bind
            // 3. Xaml with code behind, will create as object and load
            // 4. auto generated form 

            //ParamsDataGrid.ItemsSource = mAct.InputValues;
            //mAct.UpdateInputParamsType();
            //AutoCreateEditPage();
        }       

        private void LoadXamlWithoutCode()
        {
        }

        //!!! DO NOT DELETE - will reuse when we need action edit page with Xaml
        //private void LoadXamlWithCode()
        //{
        //    // temp - get from plugin the correct dll location !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //    Assembly a = Assembly.LoadFrom(@"C:\Yaron\TFS\Ginger\Devs\GingerNextVer_Dev\SeleniumPluginWPF\bin\Debug\SeleniumPluginWPF.dll");

        //    string EditPage = "SeleniumPluginWPF.SpeedTestEditPage";
        //    UIElement uc = (UIElement)a.CreateInstance(EditPage);
        //    if (uc != null)
        //    {
        //        Page p1 = (Page)uc;
        //        MainFrame.Content = p1;
        //        BindControlsToAction((Grid)p1.Content);
        //    }
        //    else
        //    {
        //        MainFrame.Content = "ERROR: Cannot create action EditPage - " + EditPage;
        //    }
        //}

        private void BindControlsToAction(Panel container)
        {
            foreach (UIElement e in container.Children)
            {
                // recursive bind for example for StackPanel or other controls Panel
                if (e is Panel) BindControlsToAction((Panel)e);
                
                // If we have property Tag filled it means it will be Binded to InputValue - Tag value = Param name
                string tag = (string)e.GetValue(TagProperty);                
                
                // We bind control based on their tag value 
                if (!string.IsNullOrEmpty(tag))
                {
                    ActInputValue v = mAct.GetOrCreateInputParam(tag);
                    if (e is TextBox)
                    {
                        GingerCore.GeneralLib.BindingHandler.ObjFieldBinding((Control)e, TextBox.TextProperty, v, "Value");
                    }

                    if (e is ComboBox)
                    {
                        //TODO: fill combo from enum val
                        GingerCore.GeneralLib.BindingHandler.ObjFieldBinding((Control)e, ComboBox.SelectedValueProperty, v, "Value");
                    }
                    //TODO: check control type and bind per type
                }

                //TODO: automatic styling
                if (e is Label)
                {
                    ((Label)e).Style = App.GetStyle("@InputFieldLabelStyle");     // TODO: use const/enum so will pass compile check                             

                }
                else if (e is TextBox)
                {
                     ((TextBox)e).Style = App.GetStyle("@TextBoxStyle");   // TODO: use const/enum so will pass compile check             
                }
            }
        }
    }
}