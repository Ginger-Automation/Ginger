using Amdocs.Ginger.Common;
using Amdocs.Ginger.Core;
using System;
using System.Collections.Generic;
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

namespace Ginger.User
{
    /// <summary>
    /// Interaction logic for UserSettings.xaml
    /// </summary>
    public partial class UserSettingsPage : Page
    {
        GenericWindow _pageGenericWin;
        readonly GingerCore.eTerminologyType mOriginalTerminologyType;

        public UserSettingsPage()
        {
            InitializeComponent();

            mOriginalTerminologyType = App.UserProfile.TerminologyDictionaryType;
            xTerminologyTypeComboBox.BindControl(App.UserProfile, nameof(UserProfile.TerminologyDictionaryType));
            xTerminologyTypeNoteLbl.Visibility = Visibility.Collapsed;

            xLoggingLevelComboBox.BindControl(App.UserProfile, nameof(UserProfile.AppLogLevel));

            GingerWPF.BindingLib.ControlsBinding.ObjFieldBinding(xAutoLoadLastSolutionCheckBox, CheckBox.IsCheckedProperty, App.UserProfile, nameof(UserProfile.AutoLoadLastSolution));
            GingerWPF.BindingLib.ControlsBinding.ObjFieldBinding(xAskToUpgradeSolutionCheckBox, CheckBox.IsCheckedProperty, App.UserProfile, nameof(UserProfile.DoNotAskToUpgradeSolutions));
            GingerWPF.BindingLib.ControlsBinding.ObjFieldBinding(xAskToRecoverSolutionCheckBox, CheckBox.IsCheckedProperty, App.UserProfile, nameof(UserProfile.DoNotAskToRecoverSolutions));            
        }

        private void xTerminologyTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((GingerCore.eTerminologyType)xTerminologyTypeComboBox.SelectedValue != mOriginalTerminologyType)
            {
                xTerminologyTypeNoteLbl.Visibility = Visibility.Visible;
            }
            else
            {
                xTerminologyTypeNoteLbl.Visibility = Visibility.Collapsed;
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool startupLocationWithOffset = false)
        {
            App.UserProfile.SaveBackup();

            ObservableList<Button> winButtons = new ObservableList<Button>();            
            Button undoBtn = new Button();
            undoBtn.Content = "Undo & Close";
            undoBtn.Click += new RoutedEventHandler(UndoBtn_Click);
            winButtons.Add(undoBtn);

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, "Edit User Settings", this, winButtons, startupLocationWithOffset: startupLocationWithOffset);
        }

        private void UndoBtn_Click(object sender, RoutedEventArgs e)
        {
            App.UserProfile.RestoreFromBackup(true);
            _pageGenericWin.Close();
        }
    }
}
