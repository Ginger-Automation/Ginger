#region License
/*
Copyright © 2014-2022 European Support Limited

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

            BindingHandler.ObjFieldBinding(xFilePathTextBox.ValueTextBox, TextBox.TextProperty, mAct, nameof(ActOcr.OcrFilePath), BindingMode.TwoWay);
            BindingHandler.ObjFieldBinding(xFilePathTextBox.ValueTextBox, TextBox.ToolTipProperty, mAct, nameof(ActOcr.OcrFilePath), BindingMode.TwoWay);
            xFilePathTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActOcr.OcrFilePath));
            xFilePathTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(mAct.OcrFilePath),
                (Context.GetAsContext(mAct.Context)).BusinessFlow.CurrentActivity.ActivityName), true, false);

            BindingHandler.ObjFieldBinding(xSetPdfPasswordTextBox.ValueTextBox, TextBox.TextProperty, mAct, nameof(ActOcr.OcrPassword), BindingMode.TwoWay);
            BindingHandler.ObjFieldBinding(xSetPdfPasswordTextBox.ValueTextBox, TextBox.ToolTipProperty, mAct, nameof(ActOcr.OcrPassword), BindingMode.TwoWay);
            xSetPdfPasswordTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActOcr.OcrPassword));
            xSetPdfPasswordTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(mAct.OcrPassword),
                (Context.GetAsContext(mAct.Context)).BusinessFlow.CurrentActivity.ActivityName), true, false);

            BindingHandler.ObjFieldBinding(xPageNosTextBox.ValueTextBox, TextBox.TextProperty, mAct, nameof(ActOcr.PageNumber), BindingMode.TwoWay);
            BindingHandler.ObjFieldBinding(xPageNosTextBox.ValueTextBox, TextBox.ToolTipProperty, mAct, nameof(ActOcr.PageNumber), BindingMode.TwoWay);
            xPageNosTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActOcr.PageNumber));
            xPageNosTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(mAct.PageNumber),
                (Context.GetAsContext(mAct.Context)).BusinessFlow.CurrentActivity.ActivityName), true, false);

            BindingHandler.ObjFieldBinding(xFirstString.ValueTextBox, TextBox.TextProperty, mAct, nameof(ActOcr.FirstString), BindingMode.TwoWay);
            BindingHandler.ObjFieldBinding(xFirstString.ValueTextBox, TextBox.ToolTipProperty, mAct, nameof(ActOcr.FirstString), BindingMode.TwoWay);
            xFirstString.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActOcr.FirstString));
            xFirstString.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(mAct.FirstString),
                string.Empty), true, false);

            BindingHandler.ObjFieldBinding(xSecondString.ValueTextBox, TextBox.TextProperty, mAct, nameof(ActOcr.SecondString), BindingMode.TwoWay);
            BindingHandler.ObjFieldBinding(xSecondString.ValueTextBox, TextBox.ToolTipProperty, mAct, nameof(ActOcr.SecondString), BindingMode.TwoWay);
            xSecondString.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActOcr.SecondString));
            xSecondString.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(mAct.SecondString),
                string.Empty), true, false);

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
                    Filter = "Pdf Files (*.pdf)|*.pdf"
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
                GingerCore.General.FillComboFromEnumObj(xOcrOperationCombo, mAct.SelectedOcrImageOperation);
                BindingHandler.ObjFieldBinding(xOcrOperationCombo, ComboBox.SelectedValueProperty, mAct, nameof(ActOcr.SelectedOcrImageOperation), BindingMode.TwoWay);
            }
            else if (xOcrFileTypeCombo.SelectedValue.ToString().Equals(eActOcrFileType.ReadTextFromPDF.ToString()))
            {
                GingerCore.General.FillComboFromEnumObj(xOcrOperationCombo, mAct.SelectedOcrPdfOperation);
                BindingHandler.ObjFieldBinding(xOcrOperationCombo, ComboBox.SelectedValueProperty, mAct, nameof(ActOcr.SelectedOcrPdfOperation), BindingMode.TwoWay);
            }
            xOcrOperationCombo_SelectionChanged(null, null);
        }

        private void xOcrOperationCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
                        xLabelFirststring.Content = "First String: ";
                        xLabelSecondtring.Visibility = Visibility.Visible;
                        xLabelSecondtring.Content = "Second String: ";
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
                        xLabelFirststring.Content = "First String: ";
                        xLabelSecondtring.Visibility = Visibility.Visible;
                        xLabelSecondtring.Content = "Second String: ";
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
                    }
                }
            }
        }
    }
}
