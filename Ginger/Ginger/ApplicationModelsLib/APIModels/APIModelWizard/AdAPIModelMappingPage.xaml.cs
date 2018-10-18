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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib;
using Amdocs.Ginger.Repository;
using Ginger.ApplicationModelsLib.ModelOptionalValue;
using Ginger.UserControls;
using GingerCore;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.ApplicationModelsLib.APIModels.APIModelWizard
{
    /// <summary>
    /// Interaction logic for AdAPIModelMappingPage.xaml
    /// </summary>
    public partial class AdAPIModelMappingPage : Page, IWizardPage
    {
        public AddAPIModelWizard AddAPIModelWizard;

        public AdAPIModelMappingPage()
        {
            InitializeComponent();
            ApplicationModelsGrid.SetTitleLightStyle = true;
            XMLOptionalValuesTemplatesGrid.SetTitleLightStyle = true;
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            if (WizardEventArgs.EventType == EventType.Init)
            {
                AddAPIModelWizard = ((AddAPIModelWizard)WizardEventArgs.Wizard);
                SetXMLOptionalValuesTemplatesGrid();

            }
            //else if (WizardEventArgs.EventType == EventType.Finish)
            //{
            //    //AddAPIModelWizard.FinishEnabled = false;
                
            //}
            else if (WizardEventArgs.EventType == EventType.Active)
            {
                SetApplicationModelsGrid();
                //ApplicationModelsGrid.DataSourceList = AddAPIModelWizard.SelectedAAMList;
                ApplicationModelsGrid.DataSourceList = General.ConvertListToObservableList(AddAPIModelWizard.AAMList.Where(x => x.IsSelected == true).ToList());
                XMLOptionalValuesTemplatesGrid.DataSourceList = CurrentSelectedAPIModel.OptionalValuesTemplates;
            }
        }

        

        private void SetApplicationModelsGrid()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationAPIModel.Name), Header = "API Name" });
            ApplicationModelsGrid.SetAllColumnsDefaultView(view);
            ApplicationModelsGrid.InitViewItems();
            ApplicationModelsGrid.RowChangedEvent -= APIModelSelectionChange;
            ApplicationModelsGrid.RowChangedEvent += APIModelSelectionChange;
        }

        private ApplicationAPIModel CurrentSelectedAPIModel;
        private void APIModelSelectionChange(object sender, EventArgs e)
        {
            CurrentSelectedAPIModel = ApplicationModelsGrid.CurrentItem as ApplicationAPIModel;
            XMLOptionalValuesTemplatesGrid.DataSourceList = CurrentSelectedAPIModel.OptionalValuesTemplates;
        }

        private void SetXMLOptionalValuesTemplatesGrid()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(TemplateFile.FilePath), Header = "File Path" });

            XMLOptionalValuesTemplatesGrid.SetAllColumnsDefaultView(view);
            XMLOptionalValuesTemplatesGrid.InitViewItems();
            XMLOptionalValuesTemplatesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddOptionalValuesTemplate));
            XMLOptionalValuesTemplatesGrid.btnClearAll.AddHandler(Button.ClickEvent, new RoutedEventHandler(ClearAllXMLOptionalValuesTemplates));
            XMLOptionalValuesTemplatesGrid.btnDelete.AddHandler(Button.ClickEvent, new RoutedEventHandler(DeleteXMLOptionalValuesTemplate));
        }

        private void DeleteXMLOptionalValuesTemplate(object sender, RoutedEventArgs e)
        {
            foreach (TemplateFile XMLTF in CurrentSelectedAPIModel.OptionalValuesTemplates)
            {
                if (XMLTF.FilePath == "")
                {

                }
            }
        }

        private void ClearAllXMLOptionalValuesTemplates(object sender, RoutedEventArgs e)
        {
            CurrentSelectedAPIModel.OptionalValuesTemplates.Clear();
        }

        private void AddOptionalValuesTemplate(object sender, RoutedEventArgs e)
        {
            BrowseForTemplateFiles();
        }

        private void BrowseForTemplateFiles()
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Multiselect = true;
            if (AddAPIModelWizard.APIType == AddAPIModelWizard.eAPIType.XMLTemplates)
            {
                dlg.Filter = "XML Files (*.xml)|*.xml" + "|WSDL Files (*.wsdl)|*.wsdl" + "|All Files (*.*)|*.*";
            }
            else if (AddAPIModelWizard.APIType == AddAPIModelWizard.eAPIType.JsonTemplate)
            {
                dlg.Filter = "JSON Files (*.json)|*.json" + "|All Files (*.*)|*.*";
            }
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                foreach (String file in dlg.FileNames)
                {
                    CurrentSelectedAPIModel.OptionalValuesTemplates.Add(new TemplateFile() { FilePath = file });
                }
            }
        }      
    }
}
