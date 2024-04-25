using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Actions;
using Amdocs.Ginger.Repository;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Wordprocessing;
using Ginger.UserControls;
using Ginger.UserControlsLib.ActionInputValueUserControlLib;
using Ginger.UserControlsLib.TextEditor;
using GingerCore.Actions;
using Microsoft.TeamFoundation.SourceControl.WebApi.Legacy;
using Microsoft.VisualStudio.Services.FormInput;
using NPOI.HPSF;
using NPOI.OpenXmlFormats.Shared;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActCLIOrchestrationEditPage.xaml
    /// </summary>
    public partial class ActCLIOrchestrationEditPage : Page
    {
        private ActCLIOrchestration mAct;
        string SHFilesPath = System.IO.Path.Combine(WorkSpace.Instance.Solution.Folder, @"Documents\Scripts\");
        public ActCLIOrchestrationEditPage(ActCLIOrchestration act)
        {
            InitializeComponent();
            mAct = act;
            Context mContext = new Context();
            ScriptInterPreter.FileExtensions.Add(".*");
            ScriptInterPreter.Init(act, nameof(mAct.ScriptInterpreter), true);
            ScriptInterPreter.FilePathTextBox.TextChanged += FilePathTextBox_TextChanged;
            mAct.ScriptPath = SHFilesPath;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(WaitForProcess, System.Windows.Controls.CheckBox.IsCheckedProperty, act, nameof(mAct.WaitForProcess));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ParseResult, System.Windows.Controls.CheckBox.IsCheckedProperty, act, nameof(mAct.ParseResult));
            if (mAct.ParseResult)
            {
                xPanelDelimiter.Visibility = Visibility.Visible;
                xDelimiterTextBox.Init(mContext, act, nameof(mAct.Delimiter));
                xDelimiterTextBox.ValueTextBox.TextChanged += DelimiterTextBox_TextChanged;
            }
            else
            {
                xPanelDelimiter.Visibility = Visibility.Collapsed;
            }
            
        }

        private void DelimiterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(mAct.Delimiter))
            {
                return;
            }
            mAct.Delimiter = xDelimiterTextBox.ValueTextBox.Text;
            mAct.InvokPropertyChanngedForAllFields();
        }

        private void FilePathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(ScriptInterPreter.FilePathTextBox.Text))
            {
                return;
            }
            mAct.ScriptInterpreter = ScriptInterPreter.FilePathTextBox.Text;
            mAct.InvokPropertyChanngedForAllFields();
        }

        private void WaitForProcessChecked(object sender, RoutedEventArgs e)
        {
            mAct.WaitForProcess = true;
            mAct.InvokPropertyChanngedForAllFields();
        }

        private void WaitForProcessUnChecked(object sender, RoutedEventArgs e)
        {
            mAct.WaitForProcess = false;
            mAct.InvokPropertyChanngedForAllFields();

        }

        private void ParseResultChecked(object sender, RoutedEventArgs e)
        {
            mAct.ParseResult = true;
            xPanelDelimiter.Visibility = Visibility.Visible;
            mAct.InvokPropertyChanngedForAllFields();
        }

        private void ParseResultUnChecked(object sender, RoutedEventArgs e)
        {
            mAct.ParseResult = false;
            xPanelDelimiter.Visibility = Visibility.Collapsed;
            mAct.InvokPropertyChanngedForAllFields();
        }
    }
}
