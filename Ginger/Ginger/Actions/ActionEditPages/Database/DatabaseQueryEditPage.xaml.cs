using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions.ActionEditPages.Database
{
    /// <summary>
    /// Interaction logic for DatabaseQueryEditPage.xaml
    /// </summary>
    public partial class DatabaseQueryEditPage : Page
    {
        ActDBValidation mAct;

        public DatabaseQueryEditPage(ActDBValidation act)
        {
            InitializeComponent();
            
            mAct = act;

            // SQLUCValueExpression.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(ActDBValidation.SQL)));
            SQLUCValueExpression.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActDBValidation.SQL));
            

            //Read from sql file
            // QueryFile.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(ActDBValidation.QueryFile)), true, true, UCValueExpression.eBrowserType.File, "sql", BrowseQueryFile_Click);

            // QueryFile.ValueTextBox.TextChanged += ValueTextBox_TextChanged;
            QueryTypeRadioButton.Init(typeof(ActDBValidation.eQueryType), SqlSelection, mAct.GetOrCreateInputParam(nameof(ActDBValidation.QueryTypeRadioButton), ActDBValidation.eQueryType.FreeSQL.ToString()), QueryType_SelectionChanged);

            //Read from sql file
            // QueryFile.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(ActDBValidation.QueryFile)), true, true, UCValueExpression.eBrowserType.File, "sql", BrowseQueryFile_Click);

            // QueryFile.ValueTextBox.TextChanged += ValueTextBox_TextChanged;

            //Import SQL file in to solution folder

            // !!!!!!!!!!!!!!!!!!! ??????????????????? Fix me !!??? missing causing compile err
            // GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(ImportFile, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(nameof (ActDBValidation.ImportFile), "True"));
        }

        public void parseScriptHeader(string FileName)
        {
            mAct.QueryParams.Clear();
            string[] script = File.ReadAllLines(amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(FileName));

            foreach (string line in script)
            {
                var pattern = @"<<([^<^>].*?)>>"; // like div[1]
                                                  // Parse the XPath to extract the nodes on the path
                var matches = Regex.Matches(line, pattern);
                foreach (Match match in matches)
                {
                    ActInputValue AIV = (from aiv in mAct.QueryParams where aiv.Param == match.Groups[1].Value select aiv).FirstOrDefault();
                    if (AIV == null)
                    {
                        AIV = new ActInputValue();
                        // AIV.Active = true;

                        AIV.Param = match.Groups[1].Value;
                        mAct.QueryParams.Add(AIV);
                        AIV.Value = "";
                    }
                }
            }

            if (mAct.QueryParams.Count > 0)
                QueryParamsPanel.Visibility = Visibility.Visible;
            else
                QueryParamsPanel.Visibility = Visibility.Collapsed;
            QueryParamsGrid.DataSourceList = mAct.QueryParams;
        }



        public void QueryType_SelectionChanged(object sender, RoutedEventArgs e)
        {
            mAct.AddOrUpdateInputParamValue(nameof(ActDBValidation.QueryTypeRadioButton), (((RadioButton)sender).Tag).ToString());
            if (nameof(ActDBValidation.QueryTypeRadioButton) == ActDBValidation.eQueryType.FreeSQL.ToString())
            {

                SqlFile.Visibility = Visibility.Collapsed;
                FreeSQLStackPanel.Visibility = Visibility.Visible;

            }
            else if (nameof(ActDBValidation.QueryTypeRadioButton) == ActDBValidation.eQueryType.SqlFile.ToString())
            {
                SqlFile.Visibility = Visibility.Visible;
                FreeSQLStackPanel.Visibility = Visibility.Collapsed;
            }
        }

        public void BrowseQueryFile_Click(object sender, RoutedEventArgs e)
        {
            string SolutionFolder = WorkSpace.Instance.Solution.Folder.ToUpper();
            if (!String.IsNullOrEmpty(QueryFile.ValueTextBox.Text))
            {
                if (!System.IO.File.Exists(QueryFile.ValueTextBox.Text))
                {
                    return;
                }
                string FileName = QueryFile.ValueTextBox.Text.ToUpper();
                if (FileName.Contains(SolutionFolder))
                {
                    FileName = FileName.Replace(SolutionFolder, @"~\");
                }

                QueryFile.ValueTextBox.Text = FileName;
            }
        }

    }
}
