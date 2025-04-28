using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Windows;
using System.Windows.Data;

namespace GingerCore.GeneralLib
{
    /// <summary>
    /// Interaction logic for EmptyPOMWindow.xaml
    /// </summary>
    public partial class EmptyPOMWindow : Window
    {
        public static ApplicationPlatform TargetApplicationvalue;
        public string pomName = string.Empty;
        public bool OK = false;

        public static EmptyPOMWindow CurrentPOMWindow = null;

        ~EmptyPOMWindow()
        {
            CurrentPOMWindow = null;
        }
        public static ApplicationPOMModel OpenDialog(ApplicationPOMModel emptyPOM, ObservableList<ApplicationPlatform> listofTargetApplication, bool isMultiline = false)
        {
            EmptyPOMWindow emptyPOMWindow = new EmptyPOMWindow();
            if (listofTargetApplication == null || listofTargetApplication.Count == 0)
            {
                Reporter.ToLog(eLogLevel.ERROR, "List of TargetApplication is null or empty");
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "List of Target Application Empty or Not present");
                return emptyPOM;
            }

            emptyPOMWindow.Init(emptyPOM, listofTargetApplication, isMultiline);

            CurrentPOMWindow = emptyPOMWindow;
            emptyPOMWindow.ShowDialog();
            if (emptyPOMWindow.OK)
            {
                emptyPOM.Name = emptyPOMWindow.pomName;
                emptyPOM.TargetApplicationKey = TargetApplicationvalue.Key;
                return emptyPOM;
            }
            else
            {
                return emptyPOM;
            }

        }

        public void Init(ApplicationPOMModel emptyPOM, ObservableList<ApplicationPlatform> listofTargetApplication, bool isMultiline)
        {
            if (!isMultiline)
            {
                xPOMNameText.TextWrapping = TextWrapping.NoWrap;
                xPOMNameText.AcceptsReturn = false;
            }

            xPOMNameText.Text = emptyPOM.Name;
            xPOMNameText.Focus();
            xTargetApplicationComboBox.ItemsSource = listofTargetApplication;
            xTargetApplicationComboBox.SelectedIndex = 0;//auto selecting first value
            xTargetApplicationComboBox.Focus();

        }

        private void xOKButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(xPOMNameText.Text))
            {
                pomName = xPOMNameText.Text;
                xTargetApplicationComboBox.SelectedItem = TargetApplicationvalue;
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.RequiredFieldsEmpty);
            }
            OK = true;
            this.Close();
        }

        private void xCancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void xTargetApplicationComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (xTargetApplicationComboBox.SelectedItem != null)
            {
                TargetApplicationvalue = (ApplicationPlatform)xTargetApplicationComboBox.SelectedValue;
                Reporter.ToLog(eLogLevel.INFO, $"Target application selected: {TargetApplicationvalue}");
            }
            else
            {
                Reporter.ToLog(eLogLevel.WARN, "No target application selected.");
            }
        }

        void ObjFieldBinding(System.Windows.Controls.Control control, DependencyProperty dependencyProperty, object obj, string property)
        {
            Binding b = new Binding
            {
                Source = obj,
                Path = new PropertyPath(property),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            control.SetBinding(dependencyProperty, b);
        }

        public EmptyPOMWindow()
        {
            InitializeComponent();
        }
    }
}
