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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Ginger.UserControlsLib.TextEditor;
using GingerWPF.WizardLib;
using HtmlAgilityPack;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Interaction logic for AIGeneratedPreviewWizardPage.xaml
    /// </summary>
    public partial class AIGeneratedPreviewWizardPage : Page, IWizardPage
    {
        private AddPOMFromScreenshotWizard mWizard;
        

        ApiSettings ApiSettings { get; set; }
        public AIGeneratedPreviewWizardPage()
        {
            InitializeComponent();
        }

        public async void InitializeWebView()
        {
            try
            {
                // Define the path to the local application data folder
                mWizard.userTempDataFolderPath = Path.Combine(Path.GetTempPath(), "GingerWebView2");

                // Create the directory if it doesn't exist
                Directory.CreateDirectory(mWizard.userTempDataFolderPath);

                // Initialize WebView2 with the custom user data folder
                var environment = await CoreWebView2Environment.CreateAsync(null, mWizard.userTempDataFolderPath);
                await MyWebView.EnsureCoreWebView2Async(environment);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to load preview",ex);
            }
        }


        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddPOMFromScreenshotWizard)WizardEventArgs.Wizard;
                    InitializeWebView();
                    ApiSettings = LoadApiSettings();
                    if (ApiSettings == null)
                    {
                        Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "OpenAI settings could not be loaded.");
                        WizardEventArgs.Wizard.Cancel();   // or disable page
                        return;
                    }
                    break;
                case EventType.Active:
                    if (string.IsNullOrEmpty(ApiSettings.ApiKey) || ApiSettings.ApiKey.Equals("YourAPIKey",StringComparison.InvariantCultureIgnoreCase))
                    {
                        Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "OpenAI setting are not valid.");
                        WizardEventArgs.Wizard.Cancel();   // or disable page
                        break;
                    }
                    GenerateHtmlAsync();
                    break;
                case EventType.LeavingForNextPage:
                    break;
            }
        }

        public ApiSettings LoadApiSettings()
        {
            try
            {
                // Get the directory of the current class
                Assembly assembly = typeof(AIGeneratedPreviewWizardPage).Assembly;
                string directory = Path.GetDirectoryName(assembly.Location);

                // Combine the directory with the file name
                string filePath = Path.Combine(directory, "OpenAIappsetting.json");

                string json = File.ReadAllText(filePath);


                var root = JsonConvert.DeserializeObject<Root>(json);

                return root.ApiSettings;

            }

            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to load OpenAI API settings", ex);
                return null;
            }
        }

        private async void GenerateHtmlAsync()
        {
            
            try
            {
                if (!string.IsNullOrEmpty(mWizard.HtmlFilePath))
                {
                    xGenerateAIPanel.Visibility = Visibility.Collapsed;
                    return;
                }
                mWizard.ProcessStarted();
                string fileName = Path.GetFileName(mWizard.ScreenShotImagePath);
                xGenerateAIPanel.Visibility = Visibility.Visible;
                await GenerateResponseAsync(FilePath: mWizard.ScreenShotImagePath);
                if (!string.IsNullOrEmpty(mWizard.HtmlFilePath))
                {
                    MyWebView.Source = new Uri(mWizard.HtmlFilePath);
                    xGenerateAIPanel.Visibility = Visibility.Collapsed;
                    MyWebView.Visibility = Visibility.Visible;
                    xViewGenerateHTMLButton.Visibility = Visibility.Visible;
                    mWizard.mPomLearnUtils.IsGeneratedByAI = true;
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to load OpenAI API Response");
                    xGenerateAIPanel.Visibility = Visibility.Collapsed;
                    MyWebView.Visibility = Visibility.Collapsed;
                }
                xReGenerateButton.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to load OpenAI API Response", ex);
            }
            finally
            {
                mWizard.ProcessEnded();
            }
        }

        private async Task GenerateResponseAsync(string FilePath = null)
        {
            // Extract filename with extension
            string fileNameWithExtension = Path.GetFileName(FilePath);

            // Remove the extension
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileNameWithExtension);

            string response = await GetAzureOpenAIResponse(FilePath);
            if (!response.EndsWith("```"))
            {
                Reporter.ToLog(eLogLevel.ERROR, "Re-generate the Preview or faced same issue again contact to system administration");
                return;
            }
            response = response.Replace("```html", "").Replace("```", "");

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);
            string targetPath = System.IO.Path.Combine(WorkSpace.Instance.Solution.Folder, $"Documents{Path.DirectorySeparatorChar}AIGeneratePreview");
            if (!System.IO.Directory.Exists(targetPath))
            {
                System.IO.Directory.CreateDirectory(targetPath);
            }
            string filePath = Path.Combine(targetPath, $"{fileNameWithoutExtension}.html");
            File.WriteAllText(filePath, response);
            mWizard.HtmlFilePath = filePath;
        }


        private async Task<string> GetAzureOpenAIResponse(string imagePath)
        {
            try
            {
                using HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", ApiSettings.ApiKey);

                // Read the image file and convert it to a base64 string
                byte[] imageBytes = File.ReadAllBytes(imagePath);
                string base64Image = Convert.ToBase64String(imageBytes);
                string imageUrl = $"data:image/jpeg;base64,{base64Image}";
                string prompt = ApiSettings.SystemPrompt;

                string userprompt = ApiSettings.UserPrompt;

                var requestBody = new
                {
                    model = ApiSettings.Modelname, //"gpt-4o",
                    messages = new object[]
                    {
            new { role = "system", content = prompt },
            new
            {
                role = "user",
                content = new object[]
                {
                    new { type = "text", text = userprompt },
                    new
                    {
                        type = "image_url",
                        image_url = new { url = imageUrl },
                    }
                }
            }
                    }
                };
                string requestJson = System.Text.Json.JsonSerializer.Serialize(requestBody);
                StringContent content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                string requestUrl = $"{ApiSettings.Endpoint}openai/deployments/{ApiSettings.DeploymentName}/chat/completions?api-version={ApiSettings.ApiVersion}";
                try
                {
                    HttpResponseMessage response = await client.PostAsync(requestUrl, content);
                    response.EnsureSuccessStatusCode();

                    string responseJson = await response.Content.ReadAsStringAsync();
                    using JsonDocument jsonDoc = JsonDocument.Parse(responseJson);
                    return jsonDoc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();
                }
                catch (HttpRequestException ex)
                {
                    // Log and handle the error
                    Reporter.ToLog(eLogLevel.ERROR, $"Request error: {ex.Message}", ex);
                    return $"Error: {ex.Message}";
                }
                catch (Exception ex)
                {
                    // Handle other exceptions
                    Reporter.ToLog(eLogLevel.ERROR, $"Unexpected error: {ex.Message}", ex);
                    return $"Error: {ex.Message}";
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Unexpected error: {ex.Message}", ex);
                return $"Error: {ex.Message}";
            }
        }

        private void ReGenerateButtonClicked(object sender, RoutedEventArgs e)
        {
            mWizard.HtmlFilePath = string.Empty;
            GenerateHtmlAsync();

        }

        private void ViewGenerateHTMLButtonClicked(object sender, RoutedEventArgs e)
        {
            if (System.IO.File.Exists(mWizard.HtmlFilePath))
            {
                DocumentEditorPage docPage = new DocumentEditorPage(mWizard.HtmlFilePath, enableEdit: false, UCTextEditorTitle: string.Empty)
                {
                    Width = 500,
                    Height = 600
                };
                docPage.ShowAsWindow("Generated source code");
                return;
            }

        }
    }
}
