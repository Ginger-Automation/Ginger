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
using Amdocs.Ginger.Common.Enums;
using Ginger;
using GingerCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ginger.Drivers.DriversConfigsEditPages
{
    /// <summary>
    /// Interaction logic for UFTCredentialsDialog.xaml
    /// </summary>
    public partial class UFTCredentialsDialog : Page
    {
        private const string DialogTitleText = "UFT Mobile Credentials";
        private GenericWindow mDialogWindow;
        private readonly DriverConfigParam mClientIdParam;
        private readonly DriverConfigParam mClientSecretParam;
        private readonly DriverConfigParam mTenantIdParam;
        private readonly Func<Task<string>> mFetchDevicesFunc;
        private const string PhonesResultBorderName = "xPhonesResultBorder";
        private const string PhonesListBoxName = "xPhonesListBox";
        private const string PhonesMessageTextBlockName = "xPhonesMessageTextBlock";
        private readonly ObservableCollection<UftPhoneViewModel> mPhones = new();
        private UftPhoneViewModel mSelectedPhone;
        private string mPreferredDeviceName;
        private string mPreferredDeviceUuid;

        public string SelectedPhoneUuid { get; private set; }

        public string SelectedPhoneName { get; private set; }

        public bool? DialogResult { get; private set; }

        public UFTCredentialsDialog(DriverConfigParam clientIdParam, DriverConfigParam clientSecretParam, DriverConfigParam tenantIdParam, Func<Task<string>> fetchDevicesFunc, string initialDeviceName = null, string initialDeviceUuid = null)
        {
            InitializeComponent();

            mClientIdParam = clientIdParam;
            mClientSecretParam = clientSecretParam;
            mTenantIdParam = tenantIdParam;
            mFetchDevicesFunc = fetchDevicesFunc;
            DialogResult = false;
            mPreferredDeviceName = initialDeviceName;
            mPreferredDeviceUuid = initialDeviceUuid;

            InitCredentialsEditors();
            InitPhonesList();
        }

        public bool? ShowDialog(Window ownerWindow = null)
        {
            return ShowAsWindow(eWindowShowStyle.Dialog, ownerWindow);
        }

        public bool? ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, Window ownerWindow = null)
        {
            DialogResult = false;

            ObservableList<Button> buttons = [];
            Button okButton = new Button
            {
                Content = "OK",
                IsDefault = true,
                MinWidth = 90
            };

            if (Application.Current?.TryFindResource("$InputButtonStyle") is Style okStyle)
            {
                okButton.Style = okStyle;
            }

            okButton.Click += OkButton_Click;
            buttons.Add(okButton);

            GingerCore.General.LoadGenericWindow(ref mDialogWindow, ownerWindow ?? App.MainWindow, windowStyle, DialogTitleText, this, buttons, true, "Cancel");
            mDialogWindow = null;
            return DialogResult;
        }

        private void InitCredentialsEditors()
        {
            xClientIdVE?.Init(null, mClientIdParam, nameof(DriverConfigParam.Value));
            xClientSecretVE?.Init(null, mClientSecretParam, nameof(DriverConfigParam.Value));
            xTenantIdVE?.Init(null, mTenantIdParam, nameof(DriverConfigParam.Value));
        }

        private void InitPhonesList()
        {
            if (xPhonesListBox != null)
            {
                xPhonesListBox.ItemsSource = mPhones;
                xPhonesListBox.SelectionChanged += PhonesListBox_SelectionChanged;
            }
        }

        private async void ShowPhonesButton_Click(object sender, RoutedEventArgs e)
        {
            if (mFetchDevicesFunc == null)
            {
                DisplayPhonesResult("Fetching devices is not available in the current context.", treatContentAsMessage: true);
                return;
            }

            Button triggerButton = sender as Button;
            if (triggerButton != null)
            {
                triggerButton.IsEnabled = false;
                triggerButton.Content = "Loading...";
            }

            try
            {
                string summary = await mFetchDevicesFunc();
                if (string.IsNullOrWhiteSpace(summary))
                {
                    DisplayPhonesResult("No devices fetched or authentication failed.", treatContentAsMessage: true);
                }
                else
                {
                    DisplayPhonesResult(summary);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to fetch UFT Mobile devices from dialog", ex);
                DisplayPhonesResult($"Error: {ex.Message}", treatContentAsMessage: true);
            }
            finally
            {
                if (triggerButton != null)
                {
                    triggerButton.IsEnabled = true;
                    triggerButton.Content = "Show Phones";
                }
            }
        }

        private void DisplayPhonesResult(string content, bool treatContentAsMessage = false)
        {
            if (xPhonesResultBorder == null)
            {
                return;
            }

            xPhonesResultBorder.Visibility = Visibility.Visible;

            if (!treatContentAsMessage)
            {
                List<UftPhoneViewModel> parsedPhones = ParsePhones(content);
                mPhones.Clear();
                foreach (UftPhoneViewModel phone in parsedPhones)
                {
                    mPhones.Add(phone);
                }

                xPhonesListBox.Visibility = mPhones.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                mPhones.Clear();
                xPhonesListBox.SelectedItem = null;
                xPhonesListBox.Visibility = Visibility.Collapsed;
            }

            if (xPhonesMessageTextBlock != null)
            {
                bool showMessage = treatContentAsMessage || mPhones.Count == 0;
                xPhonesMessageTextBlock.Visibility = showMessage ? Visibility.Visible : Visibility.Collapsed;
                if (showMessage)
                {
                    xPhonesMessageTextBlock.Text = string.IsNullOrWhiteSpace(content)
                        ? "No devices fetched or authentication failed."
                        : content;
                }
            }

            ApplyPreferredSelection();

        }

        private List<UftPhoneViewModel> ParsePhones(string rawContent)
        {
            List<UftPhoneViewModel> phones = new();
            if (string.IsNullOrWhiteSpace(rawContent))
            {
                return phones;
            }

            if (TryParsePhonesFromJson(rawContent, phones))
            {
                return phones;
            }

            return ParsePhonesFromPlainText(rawContent);
        }

        private bool TryParsePhonesFromJson(string rawContent, List<UftPhoneViewModel> result)
        {
            try
            {
                JToken token = JToken.Parse(rawContent);
                ExtractPhonesFromToken(token, result);
                return result.Count > 0;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }

        private void ExtractPhonesFromToken(JToken token, List<UftPhoneViewModel> result)
        {
            if (token == null)
            {
                return;
            }

            if (token is JArray array)
            {
                foreach (JToken child in array)
                {
                    ExtractPhonesFromToken(child, result);
                }
            }
            else if (token is JObject obj)
            {
                if (obj.TryGetValue("devices", StringComparison.OrdinalIgnoreCase, out JToken devicesToken))
                {
                    ExtractPhonesFromToken(devicesToken, result);
                    return;
                }

                if (obj.TryGetValue("data", StringComparison.OrdinalIgnoreCase, out JToken dataToken) && dataToken != obj)
                {
                    ExtractPhonesFromToken(dataToken, result);
                    if (result.Count > 0)
                    {
                        return;
                    }
                }

                UftPhoneViewModel phone = CreatePhoneFromObject(obj);
                if (phone != null)
                {
                    result.Add(phone);
                }
            }
        }

        private UftPhoneViewModel CreatePhoneFromObject(JObject obj)
        {
            string uuid = GetStringValue(obj, "uuid", "udid", "id", "deviceId");
            string nickname = GetStringValue(obj, "deviceNickname", "nickname", "displayName");
            string platform = GetStringValue(obj, "platformName", "osType", "platform", "os");
            string platformVersion = GetStringValue(obj, "platformVersion", "osVersion", "version");
            string status = GetStringValue(obj, "status", "state", "availability");
            string reservationStatus = obj.SelectToken("currentReservation.status")?.ToString();
            string name = GetStringValue(obj, "name", "deviceName", "model");
            string deviceName = GetStringValue(obj, "deviceName", "model");
            string deviceType = GetStringValue(obj, "deviceType");
            string hostingType = GetStringValue(obj, "deviceHostingType");
            bool? isConnected = obj.TryGetValue("connected", out JToken connectedToken) ? connectedToken.Value<bool?>() : null;
            bool? healthError = obj.SelectToken("healthStatus.error")?.Value<bool?>();
            string healthMessage = ExtractHealthMessage(obj.SelectToken("healthStatus.message"));

            if (string.IsNullOrWhiteSpace(uuid) && string.IsNullOrWhiteSpace(nickname) && string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            return new UftPhoneViewModel
            {
                Uuid = uuid,
                Nickname = nickname,
                Status = reservationStatus ?? status,
                Platform = platform,
                PlatformVersion = platformVersion,
                Name = name,
                DeviceName = deviceName,
                DeviceType = deviceType,
                HostingType = hostingType,
                IsConnected = isConnected,
                HealthError = healthError,
                HealthMessage = healthMessage
            };
        }

        private List<UftPhoneViewModel> ParsePhonesFromPlainText(string rawContent)
        {
            List<UftPhoneViewModel> phones = new();
            string[] lines = rawContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                UftPhoneViewModel phone = CreatePhoneFromPlainText(line);
                if (phone != null)
                {
                    phones.Add(phone);
                }
            }
            return phones;
        }

        private UftPhoneViewModel CreatePhoneFromPlainText(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            string[] segments = line.Split(new[] { '|', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            string nickname = segments.Length > 0 ? segments[0].Trim() : null;

            string uuid = ExtractValueFromSegments(segments, "uuid", "udid", "id", "device");
            string status = ExtractValueFromSegments(segments, "status", "state", "availability");
            string platform = ExtractValueFromSegments(segments, "os", "platform", "ostype");
            string platformVersion = ExtractValueFromSegments(segments, "platformversion", "osversion", "version");
            string name = ExtractValueFromSegments(segments, "name", "model");

            if (string.IsNullOrWhiteSpace(nickname) && string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(uuid))
            {
                nickname = line.Trim();
            }

            return new UftPhoneViewModel
            {
                Nickname = nickname,
                Name = name,
                Status = status,
                Platform = platform,
                Uuid = uuid,
                DeviceName = name,
                PlatformVersion = platformVersion
            };
        }

        private static string ExtractValueFromSegments(IEnumerable<string> segments, params string[] keys)
        {
            foreach (string segment in segments)
            {
                foreach (string key in keys)
                {
                    int separatorIndex = segment.IndexOf(':');
                    if (separatorIndex < 0)
                    {
                        separatorIndex = segment.IndexOf('=');
                    }

                    if (separatorIndex > -1)
                    {
                        string segmentKey = segment[..separatorIndex].Trim();
                        if (segmentKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            return segment[(separatorIndex + 1)..].Trim();
                        }
                    }
                }
            }

            return null;
        }

        private static string GetStringValue(JObject obj, params string[] propertyNames)
        {
            foreach (string propertyName in propertyNames)
            {
                if (obj.TryGetValue(propertyName, StringComparison.OrdinalIgnoreCase, out JToken valueToken))
                {
                    return valueToken?.ToString();
                }
            }

            return null;
        }

        private void PhonesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                mSelectedPhone = listBox.SelectedItem as UftPhoneViewModel;
                if (mSelectedPhone == null)
                {
                    SelectedPhoneUuid = null;
                    SelectedPhoneName = null;
                }
                else
                {
                    SelectedPhoneUuid = mSelectedPhone.Uuid;
                    SelectedPhoneName = string.IsNullOrWhiteSpace(mSelectedPhone.DeviceName)
                        ? mSelectedPhone.DisplayName
                        : mSelectedPhone.DeviceName;
                }

                mPreferredDeviceUuid = SelectedPhoneUuid;
                mPreferredDeviceName = SelectedPhoneName;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            mDialogWindow?.Close();
        }

        private static string ExtractHealthMessage(JToken token)
        {
            if (token == null)
            {
                return null;
            }

            if (token.Type == JTokenType.Array)
            {
                IEnumerable<string> messages = token.Values<string>().Where(m => !string.IsNullOrWhiteSpace(m));
                return string.Join(" ", messages);
            }

            string value = token.ToString();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private void ApplyPreferredSelection()
        {
            if (xPhonesListBox == null)
            {
                return;
            }

            UftPhoneViewModel targetPhone = null;

            if (!string.IsNullOrWhiteSpace(mPreferredDeviceUuid))
            {
                targetPhone = mPhones.FirstOrDefault(phone =>
                    string.Equals(phone.Uuid, mPreferredDeviceUuid, StringComparison.OrdinalIgnoreCase));
            }

            if (targetPhone == null && !string.IsNullOrWhiteSpace(mPreferredDeviceName))
            {
                targetPhone = mPhones.FirstOrDefault(phone =>
                    string.Equals(phone.DeviceName, mPreferredDeviceName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(phone.DisplayName, mPreferredDeviceName, StringComparison.OrdinalIgnoreCase));
            }

            xPhonesListBox.SelectedItem = targetPhone;
            if (targetPhone != null)
            {
                xPhonesListBox.ScrollIntoView(targetPhone);
            }
        }

        private sealed class UftPhoneViewModel
        {
            public string Uuid { get; init; }
            public string Nickname { get; init; }
            public string Status { get; init; }
            public string Platform { get; init; }
            public string PlatformVersion { get; init; }
            public string Name { get; init; }
            public string DeviceName { get; init; }
            public string DeviceType { get; init; }
            public string HostingType { get; init; }
            public bool? IsConnected { get; init; }
            public bool? HealthError { get; init; }
            public string HealthMessage { get; init; }

            public string DisplayName
            {
                get
                {
                    if (!string.IsNullOrWhiteSpace(Nickname))
                    {
                        return Nickname;
                    }

                    if (!string.IsNullOrWhiteSpace(Name))
                    {
                        return Name;
                    }

                    return string.IsNullOrWhiteSpace(Uuid) ? "Unnamed Device" : Uuid;
                }
            }

            public string DeviceNameDisplay => string.IsNullOrWhiteSpace(DeviceName) ? string.Empty : DeviceName;

            public string StatusDisplay => string.IsNullOrWhiteSpace(Status) ? string.Empty : $"Status: {Status}";

            public string StatusText => string.IsNullOrWhiteSpace(Status) ? "UNKNOWN" : Status.Trim();

            public string PlatformDisplay
            {
                get
                {
                    if (string.IsNullOrWhiteSpace(Platform) && string.IsNullOrWhiteSpace(PlatformVersion))
                    {
                        return "Platform: Unknown";
                    }

                    if (!string.IsNullOrWhiteSpace(Platform) && !string.IsNullOrWhiteSpace(PlatformVersion))
                    {
                        return $"{Platform} {PlatformVersion}";
                    }

                    return !string.IsNullOrWhiteSpace(Platform) ? Platform : PlatformVersion;
                }
            }

            public string UuidDisplay => string.IsNullOrWhiteSpace(Uuid) ? "UUID: Unknown" : $"UUID: {Uuid}";

            public string ConnectionText => IsConnected == true ? "Connected" : IsConnected == false ? "Offline" : "Unknown";

            public eImageType IconType => string.Equals(Platform, "ios", StringComparison.OrdinalIgnoreCase)
                ? eImageType.Ios
                : eImageType.Android;

            public Brush StatusBrush => GetStatusBrush();

            public Brush ConnectionBrush => IsConnected == true ? Brushes.DodgerBlue : IsConnected == false ? Brushes.Gray : Brushes.SlateGray;

            public string HealthMessageDisplay => string.IsNullOrWhiteSpace(HealthMessage) ? string.Empty : HealthMessage;

            public Brush HealthBrush
            {
                get
                {
                    if (string.IsNullOrWhiteSpace(HealthMessage))
                    {
                        return Brushes.Transparent;
                    }

                    return HealthError == true ? Brushes.IndianRed : Brushes.ForestGreen;
                }
            }

            private Brush GetStatusBrush()
            {
                string normalized = StatusNormalized;
                return normalized switch
                {
                    "free" or "available" => Brushes.MediumSeaGreen,
                    "used" or "busy" or "in use" or "reserved" => Brushes.DarkOrange,
                    "off" or "offline" or "disconnected" => Brushes.DimGray,
                    _ => Brushes.SlateGray
                };
            }

            private string StatusNormalized => string.IsNullOrWhiteSpace(Status)
                ? string.Empty
                : Status.Trim().ToLowerInvariant();
        }
    }
}
