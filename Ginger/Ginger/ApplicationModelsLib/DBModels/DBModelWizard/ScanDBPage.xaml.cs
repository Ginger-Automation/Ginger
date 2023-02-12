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

using Amdocs.Ginger.Repository;
using GingerWPF;
using GingerWPF.WizardLib;
using System.Data;
using System.Data.OleDb;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.ApplicationModels.DBModels.DBModelWizard
{
    /// <summary>
    /// Interaction logic for ScanDBPage.xaml
    /// </summary>
    public partial class ScanDBPage : Page, IWizardPage
    {
        ApplicationDBModel mApplicationDBModel;

        public ScanDBPage(ApplicationDBModel ApplicationDBModel)
        {
            InitializeComponent();

            mApplicationDBModel = ApplicationDBModel;

            NameTextBox.BindControl(mApplicationDBModel, nameof(ApplicationDBModel.Name));

            TableInfoGrid.ItemsSource = mApplicationDBModel.Tables;

            //TODO: TEMP - keep in AppDB RI or take from env
          
            // TODO: remove hard code conn string
            ConnectionStringTextBox.Text = (@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\yaron\Ginger\POM\POM\Documents\Customers.accdb");

            mApplicationDBModel.Name  = "splcustm";
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {

        }

        private void Learnbutton_Click(object sender, RoutedEventArgs e)
        {
            using (OleDbConnection connection = new OleDbConnection(ConnectionStringTextBox.Text))
            {
                connection.Open();
                DataTable DT = connection.GetSchema("Tables");

                foreach (System.Data.DataRow row in DT.Rows)
                {
                    ApplicationDBTableModel t = new ApplicationDBTableModel();
                    t.Name = (string)row["TABLE_NAME"];
                    mApplicationDBModel.Tables.Add(t);
                }
            }
        }
    }
}