#region License
/*
Copyright © 2014-2023 European Support Limited

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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GingerCore.Environments;
using GingerCore.Actions;
using GingerCore.NoSqlBase;
using amdocs.ginger.GingerCoreNET;
using System.IO;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using Amdocs.Ginger.Common;
using System.Windows.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using GingerWPF.BusinessFlowsLib;
using GingerCore;
using GingerCore.GeneralLib;
using static GingerCore.ActOcr;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ValidationDBPage.xaml
    /// </summary>
    public partial class ActOcrEditPage : Page
    {
        private ActOcr mAct;

        public ActOcrEditPage(ActOcr act)
        {
            InitializeComponent();

            this.mAct = act;
            GingerCore.General.FillComboFromEnumObj(xOcrFileTypeCombo, mAct.SelectedOcrFileType);
            BindingHandler.ObjFieldBinding(xOcrFileTypeCombo, ComboBox.SelectedValueProperty, mAct, nameof(ActOcr.SelectedOcrFileType), BindingMode.TwoWay);

            xFilePathTextBox.Init(Context.GetAsContext(act.Context), act.GetOrCreateInputParam(nameof(act.OcrFilePath), string.Empty),
                               true, false);
            xSetPdfPasswordTextBox.Init(Context.GetAsContext(act.Context), act.GetOrCreateInputParam(nameof(act.OcrPassword), string.Empty),
                               true, false);
            xPageNosTextBox.Init(Context.GetAsContext(act.Context), act.GetOrCreateInputParam(nameof(act.PageNumber), string.Empty),
                               true, false);
            xFirstString.Init(Context.GetAsContext(act.Context), act.GetOrCreateInputParam(nameof(act.FirstString), string.Empty),
                               true, false);
            xSecondString.Init(Context.GetAsContext(act.Context), act.GetOrCreateInputParam(nameof(act.SecondString), string.Empty),
                               true, false);
            xRowNumber.Init(Context.GetAsContext(act.Context), act.GetOrCreateInputParam(nameof(act.GetFromRowNumber), string.Empty),
                   true, false);
            xColumnWhere.Init(Context.GetAsContext(act.Context), act.GetOrCreateInputParam(nameof(act.ConditionColumnName), string.Empty),
                   true, false);
            xColumnWhereValue.Init(Context.GetAsContext(act.Context), act.GetOrCreateInputParam(nameof(act.ConditionColumnValue), string.Empty),
                   true, false);
            GingerCore.General.FillComboFromEnumObj(xOperationCombo, mAct.ElementLocateBy);
            BindingHandler.ObjFieldBinding(xOperationCombo, ComboBox.SelectedValueProperty, mAct, nameof(ActOcr.ElementLocateBy), BindingMode.TwoWay);

            if (string.IsNullOrEmpty(mAct.UseRowNumber))
            {
                mAct.UseRowNumber = true.ToString();
            }

            SelectRowNumberRdb.IsChecked = bool.Parse(mAct.UseRowNumber);
            SelectColumnValueRdb.IsChecked = !bool.Parse(mAct.UseRowNumber);
        }

        private void xBrowseFilePath_Click(object sender, RoutedEventArgs e)
        {
            if (mAct.SelectedOcrFileType.Equals(eActOcrFileType.ReadTextFromImage))
            {
                if (General.SetupBrowseFile(new System.Windows.Forms.OpenFileDialog()
                {
                    DefaultExt = "*.jpg or .jpeg or .png",
                    Filter = "Image Files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png"
                }, false) is string fileName)
                {
                    fileName = WorkSpace.Instance.SolutionRepository.ConvertFullPathToBeRelative(fileName);
                    mAct.OcrFilePath = WorkSpace.Instance.SolutionRepository.GetFolderFullPath(fileName);
                    xFilePathTextBox.ValueTextBox.Text = fileName;
                }
            }
            else if (mAct.SelectedOcrFileType.Equals(eActOcrFileType.ReadTextFromPDF))
            {
                if (General.SetupBrowseFile(new System.Windows.Forms.OpenFileDialog()
                {
                    DefaultExt = "*.pdf",
                    Filter = "PDF Files (*.pdf)|*.pdf"
                }, false) is string fileName)
                {
                    fileName = WorkSpace.Instance.SolutionRepository.ConvertFullPathToBeRelative(fileName);
                    mAct.OcrFilePath = WorkSpace.Instance.SolutionRepository.GetFolderFullPath(fileName);
                    xFilePathTextBox.ValueTextBox.Text = fileName;
                }
            }
        }

        private void xOcrFileTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            xOcrOperationCombo.ClearControlsBindings();
            if (xOcrFileTypeCombo.SelectedValue.ToString().Equals(eActOcrFileType.ReadTextFromImage.ToString()))
            {
                mAct.SelectedOcrFileType = eActOcrFileType.ReadTextFromImage;
                xAdvancedSettingsExpander.Visibility = Visibility.Collapsed;
                GingerCore.General.FillComboFromEnumObj(xOcrOperationCombo, mAct.SelectedOcrImageOperation);
                BindingHandler.ObjFieldBinding(xOcrOperationCombo, ComboBox.SelectedValueProperty, mAct, nameof(ActOcr.SelectedOcrImageOperation), BindingMode.TwoWay);
            }
            else if (xOcrFileTypeCombo.SelectedValue.ToString().Equals(eActOcrFileType.ReadTextFromPDF.ToString()))
            {
                mAct.SelectedOcrFileType = eActOcrFileType.ReadTextFromPDF;
                xAdvancedSettingsExpander.Visibility = Visibility.Visible;
                GingerCore.General.FillComboFromEnumObj(xOcrOperationCombo, mAct.SelectedOcrPdfOperation);
                BindingHandler.ObjFieldBinding(xOcrOperationCombo, ComboBox.SelectedValueProperty, mAct, nameof(ActOcr.SelectedOcrPdfOperation), BindingMode.TwoWay);
            }
            xOcrOperationCombo_SelectionChanged(null, null);
        }

        private void xOcrOperationCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            xTableWhereStack.Visibility = Visibility.Collapsed;
            if (xOcrFileTypeCombo.SelectedValue.ToString().Equals(eActOcrFileType.ReadTextFromImage.ToString()))
            {
                if (xOcrOperationCombo.SelectedValue != null)
                {
                    if (xOcrOperationCombo.SelectedValue.ToString().Equals(eActOcrImageOperations.ReadAllText.ToString()))
                    {
                        lblPageNos.Visibility = Visibility.Collapsed;
                        xPageNosTextBox.Visibility = Visibility.Collapsed;
                        xLabelFirststring.Visibility = Visibility.Collapsed;
                        xFirstString.Visibility = Visibility.Collapsed;
                        xLabelSecondtring.Visibility = Visibility.Collapsed;
                        xSecondString.Visibility = Visibility.Collapsed;
                        xInfoPageNum.Visibility = Visibility.Collapsed;
                    }
                    else if (xOcrOperationCombo.SelectedValue.ToString().Equals(eActOcrImageOperations.ReadTextAfterLabel.ToString()))
                    {
                        lblPageNos.Visibility = Visibility.Collapsed;
                        xInfoPageNum.Visibility = Visibility.Collapsed;
                        xPageNosTextBox.Visibility = Visibility.Collapsed;
                        xLabelFirststring.Visibility = Visibility.Visible;
                        xLabelFirststring.Content = "Label: ";
                        xFirstString.Visibility = Visibility.Visible;
                        xLabelSecondtring.Visibility = Visibility.Collapsed;
                        xSecondString.Visibility = Visibility.Collapsed;
                    }
                    else if (xOcrOperationCombo.SelectedValue.ToString().Equals(eActOcrImageOperations.ReadTextBetweenTwoStrings.ToString()))
                    {
                        lblPageNos.Visibility = Visibility.Collapsed;
                        xInfoPageNum.Visibility = Visibility.Collapsed;
                        xPageNosTextBox.Visibility = Visibility.Collapsed;
                        xLabelFirststring.Visibility = Visibility.Visible;
                        xFirstString.Visibility = Visibility.Visible;
                        xLabelFirststring.Content = "Start String: ";
                        xLabelSecondtring.Visibility = Visibility.Visible;
                        xLabelSecondtring.Content = "End String: ";
                        xSecondString.Visibility = Visibility.Visible;
                    }
                }
            }
            else if (xOcrFileTypeCombo.SelectedValue.ToString().Equals(eActOcrFileType.ReadTextFromPDF.ToString()))
            {
                if (xOcrOperationCombo.SelectedValue != null)
                {
                    if (xOcrOperationCombo.SelectedValue.ToString().Equals(eActOcrPdfOperations.ReadTextAfterLabel.ToString()))
                    {
                        lblPageNos.Visibility = Visibility.Visible;
                        xInfoPageNum.Visibility = Visibility.Visible;
                        xPageNosTextBox.Visibility = Visibility.Visible;
                        xLabelFirststring.Visibility = Visibility.Visible;
                        xLabelFirststring.Content = "Label: ";
                        xFirstString.Visibility = Visibility.Visible;
                        xLabelSecondtring.Visibility = Visibility.Collapsed;
                        xSecondString.Visibility = Visibility.Collapsed;
                    }
                    else if (xOcrOperationCombo.SelectedValue.ToString().Equals(eActOcrPdfOperations.ReadTextBetweenTwoStrings.ToString()))
                    {
                        lblPageNos.Visibility = Visibility.Visible;
                        xInfoPageNum.Visibility = Visibility.Visible;
                        xPageNosTextBox.Visibility = Visibility.Visible;
                        xLabelFirststring.Visibility = Visibility.Visible;
                        xFirstString.Visibility = Visibility.Visible;
                        xLabelFirststring.Content = "Start String: ";
                        xLabelSecondtring.Visibility = Visibility.Visible;
                        xLabelSecondtring.Content = "End String: ";
                        xSecondString.Visibility = Visibility.Visible;
                    }
                    else if (xOcrOperationCombo.SelectedValue.ToString().Equals(eActOcrPdfOperations.ReadTextFromPDFSinglePage.ToString()))
                    {
                        lblPageNos.Visibility = Visibility.Visible;
                        xInfoPageNum.Visibility = Visibility.Visible;
                        xPageNosTextBox.Visibility = Visibility.Visible;
                        xLabelFirststring.Visibility = Visibility.Collapsed;
                        xFirstString.Visibility = Visibility.Collapsed;
                        xLabelSecondtring.Visibility = Visibility.Collapsed;
                        xSecondString.Visibility = Visibility.Collapsed;
                    }
                    else if (xOcrOperationCombo.SelectedValue.ToString().Equals(eActOcrPdfOperations.ReadTextFromTableInPdf.ToString()))
                    {
                        lblPageNos.Visibility = Visibility.Visible;
                        xInfoPageNum.Visibility = Visibility.Visible;
                        xPageNosTextBox.Visibility = Visibility.Visible;
                        xLabelFirststring.Visibility = Visibility.Visible;
                        xLabelFirststring.Content = "Column Name: ";
                        xFirstString.Visibility = Visibility.Visible;
                        xLabelSecondtring.Visibility = Visibility.Collapsed;
                        xSecondString.Visibility = Visibility.Collapsed;
                        xTableWhereStack.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void SelectColumnValueRdb_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = (RadioButton)sender;

            mAct.UseRowNumber = rb.IsChecked.Value.ToString();
            xRowNumber.IsEnabled = rb.IsChecked.Value;
            xColumnWhere.IsEnabled = !rb.IsChecked.Value;
            xColumnWhereValue.IsEnabled = !rb.IsChecked.Value;
            xOperationCombo.IsEnabled = !rb.IsChecked.Value;
        }
    }
}
