using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.External.WireMock;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using Ginger.UserControlsLib.TextEditor;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static Amdocs.Ginger.CoreNET.External.WireMock.WireMockMapping;

namespace Ginger.ApplicationModelsLib.WireMockAPIModels
{
    /// <summary>
    /// Interaction logic for WireMockTemplatePage.xaml
    /// </summary>
    public partial class WireMockTemplatePage : Page
    {

        public WireMockMappingController wmController;
        public ApplicationAPIModel mApplicationAPIModel;
        public WireMockTemplatePage(ApplicationAPIModel applicationAPIModel, Ginger.General.eRIPageViewMode pageViewMode = Ginger.General.eRIPageViewMode.Standalone)
        {
            wmController = new WireMockMappingController();
            mApplicationAPIModel = applicationAPIModel;
            InitializeComponent();
            SetGridView();
            SetGridData();
        }

        private void SetGridView()
        {
            //Set the grid name
            xGridMappingOutput.SetTitleLightStyle = true;

            //Set the Tool Bar look
            xGridMappingOutput.ShowUpDown = Visibility.Collapsed;
            xGridMappingOutput.ShowUndo = Visibility.Visible;

            //Set the Data Grid columns
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
                [
                    new GridColView() { Field = nameof(Mapping.Name), WidthWeight = 100 },
                    new GridColView() { Field = "Request.Url", Header="Request URL", WidthWeight = 60, BindingMode = BindingMode.OneWay },
                    new GridColView() { Field = "Request.Method", Header="Request Method", WidthWeight = 50, BindingMode = BindingMode.OneWay },
                    new GridColView() { Field = "Response.Status", Header="Response Status Code", WidthWeight = 50, BindingMode = BindingMode.OneWay },
                    new GridColView() { Field = "Response.Body", WidthWeight = 150, Header = "Response Body" },
                    new GridColView() { Field = "Operations", WidthWeight = 60,  StyleType = GridColView.eGridColStyleType.Template,Header = "Operations", CellTemplate = CreateOperationTemplate() },
                    ]
            };

            xGridMappingOutput.SetAllColumnsDefaultView(view);
            xGridMappingOutput.InitViewItems();
            xGridMappingOutput.AddToolbarTool("@ArrowDown_16x16.png", "Download Mapping", xImportMapping_Click, 0);
            xGridMappingOutput.AddToolbarTool(Amdocs.Ginger.Common.Enums.eImageType.ID, "Copy selected item ID", CopySelectedItemID);
            xGridMappingOutput.AddButton("", AddNewMappingAsync);

        }

        private void CopySelectedItemID(object sender, RoutedEventArgs e)
        {
            if (xGridMappingOutput.Grid.SelectedItem != null)
            {
                GingerCore.General.SetClipboardText(((WireMockMapping.Mapping)xGridMappingOutput.Grid.SelectedItem).Id.ToString());
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }

        private async void AddNewMappingAsync(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Mapping mapping)
            {
                try
                {
                    string mappingJson = await wmController.mockAPI.GetStubAsync(mapping.Id);
                    if (!string.IsNullOrEmpty(mappingJson))
                    {

                        var jsonObject = JsonNode.Parse(mappingJson);
                        // Remove "id" and "uuid" if they exist
                        jsonObject!.AsObject().Remove("id");
                        jsonObject.AsObject().Remove("uuid");

                        // Convert back to JSON string
                        string formattedJson = jsonObject.ToJsonString(new JsonSerializerOptions { WriteIndented = true });

                        // Create a new window to display and edit the JSON
                        Window jsonWindow = new Window
                        {
                            Title = "Edit WireMock Mapping",
                            Width = 600,
                            Height = 400
                        };

                        TextBox jsonTextBox = new TextBox
                        {
                            Text = formattedJson,
                            IsReadOnly = false,
                            TextWrapping = TextWrapping.Wrap,
                            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                            AcceptsReturn = true
                        };

                        Button updateButton = new Button
                        {
                            Content = "Update",
                            Width = 100,
                            Height = 30,
                            Margin = new Thickness(10)
                        };

                        updateButton.Click += async (_, args) =>
                        {
                            try
                            {
                                string updatedJson = jsonTextBox.Text;
                                string result = await wmController.mockAPI.CreateStubAsync(updatedJson);
                                if (!string.IsNullOrEmpty(result))
                                {
                                    // Update the mapping in the grid
                                    int index = xGridMappingOutput.DataSourceList.IndexOf(mapping);
                                    xGridMappingOutput.DataSourceList[index] = JsonConvert.DeserializeObject<Mapping>(updatedJson);
                                    jsonWindow.Close();
                                    Reporter.ToUser(eUserMsgKey.WireMockMappingUpdateSuccess);
                                }
                                else
                                {
                                    Reporter.ToUser(eUserMsgKey.WireMockMappingUpdateFail);
                                }
                            }
                            catch (Exception ex)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, "Failed to update WireMock mapping", ex);
                                Reporter.ToUser(eUserMsgKey.WireMockAPIError);
                            }
                        };

                        StackPanel panel = new StackPanel();
                        panel.Children.Add(jsonTextBox);
                        panel.Children.Add(updateButton);

                        jsonWindow.Content = panel;
                        jsonWindow.ShowDialog();
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKey.WireMockAPIError);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to edit WireMock mapping", ex);
                    Reporter.ToUser(eUserMsgKey.WireMockAPIError);
                }
            }

        }

        private DataTemplate CreateOperationTemplate()
        {
            return (DataTemplate)xWMMappingTemplatePage.Resources["xMappingOperationTab"];
        }

        private async Task SetGridData()
        {

            var mappings = await wmController.DeserializeWireMockResponseAsync();
            if (mappings.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.WireMockMappingEmpty);
                return;
            }

            string ApiName = mApplicationAPIModel.Name;

            // Filter the mappings based on the trimmed endpoint URL
            var filteredMappings = mappings.Where(mapping => mapping.Name == ApiName).ToList();

            xGridMappingOutput.DataSourceList = new ObservableList<Mapping>(filteredMappings);
        }

        private async void xImportMapping_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                string mappingJson = await wmController.DownloadWireMockMappingsAsync();

                if (string.IsNullOrEmpty(mappingJson))
                {
                    Reporter.ToUser(eUserMsgKey.WireMockMappingDownloadFailed);
                    return;
                }
                // Save the JSON to a file
                string filePath = "WireMockAPI_mappings.json";
                File.WriteAllText(filePath, mappingJson);
                Reporter.ToUser(eUserMsgKey.WireMockMappingDownload);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Mapping downloading got failed", ex);
                Reporter.ToUser(eUserMsgKey.WireMockMappingDownloadFailed);
            }
        }

        private async void xViewMappingbtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Mapping mapping)
            {
                try
                {
                    string mappingJson = await wmController.mockAPI.GetStubAsync(mapping.Id);
                    if (!string.IsNullOrEmpty(mappingJson))
                    {

                        string tempFilePath = GingerCoreNET.GeneralLib.General.CreateTempJsonFile(mappingJson);
                        if (!string.IsNullOrEmpty(tempFilePath))
                        {
                            DocumentEditorPage docPage = new DocumentEditorPage(tempFilePath, enableEdit: false, UCTextEditorTitle: string.Empty, isFromWireMock: true, wireMockmappingId: mapping.Id)
                            {
                                Width = 800,
                                Height = 800
                            };
                            docPage.ShowAsWindow("API Mapping Details");
                            System.IO.File.Delete(tempFilePath);
                            return;
                        }
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKey.WireMockAPIError);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to view WireMock mapping", ex);
                    Reporter.ToUser(eUserMsgKey.WireMockAPIError);
                }
            }
        }

        private async void xEditMappingbtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Mapping mapping)
            {
                try
                {
                    string mappingJson = await wmController.mockAPI.GetStubAsync(mapping.Id);
                    if (!string.IsNullOrEmpty(mappingJson))
                    {
                        // Create a new window to display and edit the JSON
                        Window jsonWindow = new Window
                        {
                            Title = "Edit WireMock Mapping",
                            Width = 600,
                            Height = 400
                        };

                        TextBox jsonTextBox = new TextBox
                        {
                            Text = mappingJson,
                            IsReadOnly = false,
                            TextWrapping = TextWrapping.Wrap,
                            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                            AcceptsReturn = true
                        };

                        Button updateButton = new Button
                        {
                            Content = "Update",
                            Width = 100,
                            Height = 30,
                            Margin = new Thickness(10)
                        };

                        updateButton.Click += async (s, args) =>
                        {
                            try
                            {
                                string updatedJson = jsonTextBox.Text;
                                string result = await wmController.mockAPI.UpdateStubAsync(mapping.Id, updatedJson);
                                if (!string.IsNullOrEmpty(result))
                                {
                                    // Update the mapping in the grid
                                    int index = xGridMappingOutput.DataSourceList.IndexOf(mapping);
                                    xGridMappingOutput.DataSourceList[index] = JsonConvert.DeserializeObject<Mapping>(updatedJson);
                                    jsonWindow.Close();
                                    Reporter.ToUser(eUserMsgKey.WireMockMappingUpdateSuccess);
                                }
                                else
                                {
                                    Reporter.ToUser(eUserMsgKey.WireMockMappingUpdateFail);
                                }
                            }
                            catch (Exception ex)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, "Failed to update WireMock mapping", ex);
                                Reporter.ToUser(eUserMsgKey.WireMockAPIError);
                            }
                        };

                        StackPanel panel = new StackPanel();
                        panel.Children.Add(jsonTextBox);
                        panel.Children.Add(updateButton);

                        jsonWindow.Content = panel;
                        jsonWindow.ShowDialog();
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKey.WireMockAPIError);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to edit WireMock mapping", ex);
                    Reporter.ToUser(eUserMsgKey.WireMockAPIError);
                }
            }
        }

        private async void xDeleteMappingBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Mapping mapping)
            {
                try
                {
                    string result = await wmController.mockAPI.DeleteStubAsync(mapping.Id);
                    if (!string.IsNullOrEmpty(result))
                    {
                        // Remove the mapping from the grid
                        xGridMappingOutput.DataSourceList.Remove(mapping);
                        Reporter.ToUser(eUserMsgKey.WireMockMappingDeleteSuccess);
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKey.WireMockMappingDeleteFail);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to delete WireMock mapping", ex);
                    Reporter.ToUser(eUserMsgKey.WireMockAPIError);
                }
            }
        }
    }
}
