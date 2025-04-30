using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Application_Models;
using Ginger.UserControlsLib.TextEditor;
using GingerWPF.WizardLib;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Interaction logic for POMScreenshotHTMLViewPage.xaml
    /// </summary>
    public partial class AIGeneratedPreviewWizardPage : Page, IWizardPage
    {
        private AddPOMFromScreenshotWizard mWizard;


        ApiSettings ApiSettings { get; set; }
        public AIGeneratedPreviewWizardPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddPOMFromScreenshotWizard)WizardEventArgs.Wizard;
                    ApiSettings = LoadApiSettings();
                    if (ApiSettings == null)
                    {
                        Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "OpenAI settings could not be loaded.");
                        WizardEventArgs.Wizard.Cancel();   // or disable page
                        return;
                    }
                    break;
                case EventType.Active:
                    if(string.IsNullOrEmpty(ApiSettings.ApiKey) || ApiSettings.ApiKey.Equals("YouuAPiKey"))
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
                string directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

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
            if(!string.IsNullOrEmpty(mWizard.HtmlFilePath))
            {
                xGenerateAIPanel.Visibility = Visibility.Collapsed;
                return;
            }
            mWizard.ProcessStarted();
            string fileName = Path.GetFileName(mWizard.ScreenShotImagePath);
            xGenerateAIPanel.Visibility = Visibility.Visible;
            await GenerateResponseAsync(fileName: fileName, FilePath: mWizard.ScreenShotImagePath);
            if (!string.IsNullOrEmpty(mWizard.HtmlFilePath))
            {
                MyWebView.Source = new Uri(mWizard.HtmlFilePath);
                xGenerateAIPanel.Visibility = Visibility.Collapsed;
                MyWebView.Visibility = Visibility.Visible;
                xViewGenerateHTMLButton.Visibility = Visibility.Visible;
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to load OpenAI API Response");
                xGenerateAIPanel.Visibility = Visibility.Collapsed;
                MyWebView.Visibility = Visibility.Collapsed;
            }
            xReGenerateButton.Visibility = Visibility.Visible;
            mWizard.ProcessEnded();
        }

        private async Task GenerateResponseAsync(string fileName, string FilePath = null)
        {
            // Extract filename with extension
            string fileNameWithExtension = Path.GetFileName(FilePath);

            // Remove the extension
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileNameWithExtension);

            string response = await GetChatGPTResponse(FilePath);
            if(!response.EndsWith("```"))
            {
                Reporter.ToLog(eLogLevel.ERROR, "Re-generate the Preview or faced same issue again contact to system administration");
                return;
            }
            response = response.Replace("```html", "").Replace("```", "");

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);
            string SolutionFolder = WorkSpace.Instance.Solution.Folder;
            string targetPath = System.IO.Path.Combine(SolutionFolder, @"Documents\AIGeneratePreview");
            if (!System.IO.Directory.Exists(targetPath))
            {
                System.IO.Directory.CreateDirectory(targetPath);
            }
            string filePath = Path.Combine(targetPath, $"{fileNameWithoutExtension}.html");
            File.WriteAllText(filePath, response);
            mWizard.HtmlFilePath = filePath;
        }


        private async Task<string> GetChatGPTResponse(string imagePath)
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("api-key", ApiSettings.ApiKey);

            // Read the image file and convert it to a base64 string
            byte[] imageBytes = File.ReadAllBytes(imagePath);
            string base64Image = Convert.ToBase64String(imageBytes);
            string imageUrl = $"data:image/jpeg;base64,{base64Image}";
            string prompt = ApiSettings.Prompt;

            string userprompt = ApiSettings.UserPrompt; //"Make sure you are generating HTML code only and every element should have id and name as per the element type";

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

            string requestUrl = $"{ApiSettings.endpoint}openai/deployments/{ApiSettings.deploymentName}/chat/completions?api-version={ApiSettings.apiVersion}";
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
            catch (HttpRequestException e)
            {
                // Log and handle the error
                Reporter.ToLog(eLogLevel.ERROR, $"Request error: {e.Message}");
                return $"Error: {e.Message}";
            }
            catch (Exception e)
            {
                // Handle other exceptions
                Reporter.ToLog(eLogLevel.ERROR, $"Unexpected error: {e.Message}");
                return $"Error: {e.Message}";
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
                docPage.ShowAsWindow("View source code");
                return;
            }

        }
    }
}
