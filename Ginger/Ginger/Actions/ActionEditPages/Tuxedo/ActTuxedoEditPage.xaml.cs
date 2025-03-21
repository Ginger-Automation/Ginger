#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Ginger.UserControls;
using GingerCore.Actions;
using GingerCore.Actions.Tuxedo;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.Actions.Tuxedo
{
    /// <summary>
    /// Interaction logic for XLSReadDataToVariablesPage.xaml
    /// </summary>
    public partial class ActTuxedoEditPage : Page
    {
        ActTuxedo mAct { get; set; }

        public ActTuxedoEditPage(Act act)
        {
            InitializeComponent();

            this.mAct = (ActTuxedo)act;

            // Bind Controls
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(PCPath, TextBox.TextProperty, mAct.PCPath, nameof(ActInputValue.Value));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(PreComamndTextBox, TextBox.TextProperty, mAct.PreCommand, nameof(ActInputValue.Value));

            UnixPath.Init(Context.GetAsContext(mAct.Context), mAct.UnixPath);
            HostUCVE.Init(Context.GetAsContext(mAct.Context), mAct.Host);
            Port.Init(Context.GetAsContext(mAct.Context), mAct.Port);
            UserName.Init(Context.GetAsContext(mAct.Context), mAct.UserName);
            Password.Init(Context.GetAsContext(mAct.Context), mAct.Password);

            PrivateKey.Init(Context.GetAsContext(mAct.Context), mAct.PrivateKey);
            KeyPassPhrase.Init(Context.GetAsContext(mAct.Context), mAct.PrivateKeyPassPhrase);

            SetGridView();
        }

        private void SetGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(ActInputValue.Param), Header = "Parameter", WidthWeight = 150 },
                new GridColView() { Field = nameof(ActInputValue.Value), Header = "Value", WidthWeight = 150 },
                new GridColView() { Field = "...", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.Resources["InputValueExpressionButton"] },
                new GridColView() { Field = nameof(ActInputValue.ValueForDriver), Header = "Value For Driver", WidthWeight = 150, BindingMode = BindingMode.OneWay },
            ]
            };

            UDParamsGrid.SetAllColumnsDefaultView(view);
            UDParamsGrid.InitViewItems();

            UDParamsGrid.DataSourceList = mAct.DynamicUDElements;
        }

        private void InputGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActInputValue AIV = (ActInputValue)UDParamsGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(AIV, nameof(ActInputValue.Value), Context.GetAsContext(mAct.Context));
            VEEW.ShowAsWindow();
        }

        private void BrowsePCPathButton_Click(object sender, RoutedEventArgs e)
        {
            if (General.SetupBrowseFile(new System.Windows.Forms.OpenFileDialog()
            {
                DefaultExt = "*.UD",
                Filter = "UD Input Data Files (*.UD)|*.UD"
            }) is string fileName)
            {
                PCPath.Text = fileName;
                ProcessUDFile();
            }
        }

        private void ProcessUDFile()
        {
            string FileName = PCPath.Text;
            //if (FileName.Contains("~\\"))
            //{
            //    FileName = FileName.Replace("~",  WorkSpace.Instance.Solution.ContainingFolderFullPath);
            //}
            FileName = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(FileName);

            mAct.DynamicUDElements.Clear();
            string[] lines = System.IO.File.ReadAllLines(FileName);
            foreach (string s in lines)
            {
                int firstSpace = s.IndexOf("\t");
                string Param;
                string Value;
                if (firstSpace > 0)
                {
                    Param = s[..firstSpace];
                    Value = s[firstSpace..].Trim();
                }
                else
                {
                    Param = s;
                    Value = "";
                }
                ActInputValue AIV = new ActInputValue
                {
                    Param = Param,
                    Value = Value
                };
                mAct.DynamicUDElements.Add(AIV);
            }
        }
    }
}
