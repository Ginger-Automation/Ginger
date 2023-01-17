using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
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

namespace Ginger.UserControlsLib.UCSendEMailConfig
{
    public static class BodyContentTypeFlag
    {
        public const int FreeText = 1<<0; //1
        public const int HTMLReport = 1<<1; //2
    }

    public enum eEmailMethod
    {
        SMTP,
        Outlook
    }
    /// <summary>
    /// Interaction logic for UCSendEMailConfigView.xaml
    /// </summary>
    public partial class UCSendEMailConfigView : UserControl
    {
        public class Options
        {
            public int SupportedBodyContentType { get; set; } = BodyContentTypeFlag.HTMLReport;
            public int MaxBodyCharCount { get; set; } = 0;
            public bool FromDisplayEnabled { get; set; } = true;
            public bool CCEnabled { get; set; } = true;
            public eEmailMethod DefaultEmailMethod { get; set; } = eEmailMethod.SMTP;
        }

        private bool isFromDisplayEnabled;

        public delegate void EmailMethodChangeHandler(eEmailMethod selectedEmailMethod);
        public event EmailMethodChangeHandler? EmailMethodChanged;

        public UCSendEMailConfigView()
        {
            InitializeComponent();
        }

        public void Initialize(Options options)
        {
            SetBodyContentType(options.SupportedBodyContentType);
            SetMaxBodyCharCount(options.MaxBodyCharCount);
            isFromDisplayEnabled = options.FromDisplayEnabled;
            SetCCVisibility(ConvertBooleanToVisibility(options.CCEnabled));
            SetDefaultEmailMethod(options.DefaultEmailMethod);
        }

        private void SetBodyContentType(int supportedBodyContentType)
        {
            bool doesSupportMultipleBodyContentType = CountSetBits(supportedBodyContentType) > 1;
            if (doesSupportMultipleBodyContentType)
                ShowBodyContentTypeRadioButtons();
            else
                ShowOnlySupportedBodyContentType(supportedBodyContentType);
        }

        private void ShowBodyContentTypeRadioButtons()
        {
            xBodyContentTypeRadioButtonsContainer.Visibility = Visibility.Visible;
            SelectDefaultBodyContentTypeRadioButton();
        }

        private void SelectDefaultBodyContentTypeRadioButton()
        {
            xBodyContentTypeFreeTextRadioButton.IsChecked = true;
        }

        private void ShowOnlySupportedBodyContentType(int supportedBodyContentType)
        {
            if ((supportedBodyContentType & BodyContentTypeFlag.FreeText) > 0)
                xBodyContentTypeFreeTextContainer.Visibility = Visibility.Visible;
            else if ((supportedBodyContentType & BodyContentTypeFlag.HTMLReport) > 0)
                xBodyContentTypeHTMLReportContainer.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Counts the number of set bits in the given integer.
        /// </summary>
        private int CountSetBits(int num)
        {
            if (num <= 0)
                return 0;
            return (num % 2 == 0 ? 0 : 1) + CountSetBits(num / 2);
        }

        private void SetMaxBodyCharCount(int maxBodyCharCount)
        {
            if (maxBodyCharCount <= 0 || maxBodyCharCount == int.MaxValue)
                return;

            xBodyValueExpression.ValueTextBox.MaxLength = maxBodyCharCount;
            xMaxBodyLengthLabel.Content = $"(upto {maxBodyCharCount} characters)";
            xMaxBodyLengthLabel.Visibility = Visibility.Visible;
        }

        private void SetCCVisibility(Visibility visibility)
        {
            xCCContainer.Visibility = visibility;
        }

        private void SetDefaultEmailMethod(eEmailMethod defaultEmailMethod)
        {
            switch(defaultEmailMethod)
            {
                case eEmailMethod.SMTP:
                    xEmailMethodSMTP.IsSelected = true;
                    return;
                case eEmailMethod.Outlook:
                    xEmailMethodOutlook.IsSelected = true;
                    return;
                default:
                    throw new NotImplementedException($"setting {defaultEmailMethod} as default email method is not implemented");
            }
        }

        private void xBodyContentType_Changed(object sender, RoutedEventArgs e)
        {
            xBodyContentTypeFreeTextContainer.Visibility = ConvertBooleanToVisibility(xBodyContentTypeFreeTextRadioButton.IsChecked ?? false);
            xBodyContentTypeHTMLReportContainer.Visibility = ConvertBooleanToVisibility(xBodyContentTypeHTMLReportRadioButton.IsChecked ?? false);
        }

        private Visibility ConvertBooleanToVisibility(bool boolean)
        {
            return boolean ? Visibility.Visible : Visibility.Collapsed;
        }

        private void xEmailMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xEmailMethodSMTP.IsSelected && isFromDisplayEnabled)
                xFromDisplayContainer.Visibility = Visibility.Visible;
            else
                xFromDisplayContainer.Visibility = Visibility.Collapsed;

            ComboBoxItem selectedEmailMethodComboBoxItem = 
            TriggerEmailMethodChangedEvent()
        }


    }
}