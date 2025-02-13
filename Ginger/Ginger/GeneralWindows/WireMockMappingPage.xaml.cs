using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.External.WireMock;
using Amdocs.Ginger.UserControls;
using Ginger.SolutionGeneral;
using Ginger.UserControls;
using Ginger.UserControlsLib.TextEditor;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static Amdocs.Ginger.CoreNET.External.WireMock.WireMockMapping;

namespace Ginger.GeneralWindows
{
    /// <summary>
    /// Interaction logic for WireMockMappingPage.xaml
    /// </summary>
    public partial class WireMockMappingPage : Page
    {
        Solution mSolution;
        private GenericWindow genWin = null;
        private ImageMakerControl loaderElement = new ImageMakerControl();
        public WireMockMappingController wmController;
        public WireMockMappingPage()
        {
            wmController = new WireMockMappingController();
            InitializeComponent();
            SetGridView();
            SetGridData();
        }

        private void SetGridView()
        {
            //Set the grid name
            xGridMapping.SetTitleLightStyle = true;

            //Set the Tool Bar look
            xGridMapping.ShowUpDown = Visibility.Collapsed;
            xGridMapping.ShowUndo = Visibility.Visible;

            //Set the Data Grid columns
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
                [
                    new GridColView() { Field = nameof(Mapping.Name), WidthWeight = 100 },
                    new GridColView() { Field = "Request.Url", Header="Request URL", WidthWeight = 60, BindingMode = BindingMode.OneWay },
                    new GridColView() { Field = "Request.Method", Header="Request Method", WidthWeight = 50, BindingMode = BindingMode.OneWay },
                    new GridColView() { Field = "Response.Status", Header="Response Status Code", WidthWeight = 80, BindingMode = BindingMode.OneWay },
                    new GridColView() { Field = "Response.Body", WidthWeight = 150, Header = "Response Body" },
                    new GridColView() { Field = "Operations", WidthWeight = 60,  StyleType = GridColView.eGridColStyleType.Template,Header = "Operations", CellTemplate = CreateOperationTemplate() },
                    ]
            };

            xGridMapping.SetAllColumnsDefaultView(view);
            xGridMapping.InitViewItems();
            xGridMapping.AddToolbarTool(Amdocs.Ginger.Common.Enums.eImageType.Delete, "Delete All selected mapping", DeleteAllButton_Click);
            xGridMapping.AddToolbarTool("@ArrowDown_16x16.png", "Download Mapping", xDownloadMapping_Click, 0);
            xGridMapping.AddToolbarTool(Amdocs.Ginger.Common.Enums.eImageType.ID, "Copy selected item ID", CopySelectedItemID);

        }

        private void CopySelectedItemID(object sender, RoutedEventArgs e)
        {
            if (xGridMapping.Grid.SelectedItem != null)
            {
                GingerCore.General.SetClipboardText(((WireMockMapping.Mapping)xGridMapping.Grid.SelectedItem).Id.ToString());
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }
        private DataTemplate CreateOperationTemplate()
        {
            return (DataTemplate)xMappingWindowPage.Resources["xMappingOperationTab"];
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.OnlyDialog)
        {
            Button closeBtn = new Button
            {
                Content = "Close"
            };
            closeBtn.Click += CloseButton_Click;
            closeBtn.ToolTip = "Close Window";

            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, "WireMock Mapping Window", this, [closeBtn], loaderElement: loaderElement);
        }

        private async void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            genWin.Close();
        }

        /// <summary>
        /// Deletes All the mappings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DeleteAllButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (xGridMapping.DataSourceList.Count == 0)
                {
                    Reporter.ToUser(eUserMsgKey.WireMockMappingDeleteEmpty);
                    return;
                }

                HttpResponseMessage result = await wmController.mockAPI.DeleteAllMappingsAsync();
                if (result.IsSuccessStatusCode)
                {
                    // Remove the mapping from the grid
                    xGridMapping.DataSourceList.ClearAll();
                    Reporter.ToUser(eUserMsgKey.WireMockMappingDeleteSuccess);
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Failed to delete WireMock mapping, response received from API :{result}");
                    Reporter.ToUser(eUserMsgKey.WireMockAPIError);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to delete WireMock mapping", ex);
                Reporter.ToUser(eUserMsgKey.WireMockAPIError);
            }

        }


        private async Task SetGridData()
        {
            loaderElement.Name = "xProcessingImage";
            loaderElement.Height = 30;
            loaderElement.Width = 30;
            loaderElement.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Processing;

            xGridMapping.DataSourceList = await wmController.DeserializeWireMockResponseAsync();
            if (xGridMapping.DataSourceList.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.WireMockMappingEmpty);
                loaderElement.Visibility = Visibility.Collapsed;
                return;
            }
            loaderElement.Visibility = Visibility.Collapsed;
        }

        private async void xDownloadMapping_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string mappingJson = await wmController.DownloadWireMockMappingsAsync();

                if (string.IsNullOrEmpty(mappingJson))
                {
                    Reporter.ToUser(eUserMsgKey.WireMockMappingDownloadFailed);
                    return;
                }

                //default folder
                mSolution = WorkSpace.Instance.Solution;
                string SolFolder = mSolution.Folder;
                if (SolFolder.EndsWith(@"\"))
                {
                    SolFolder = SolFolder[..^1];
                }
                string mConfigFileFolderPath = SolFolder + @"\Documents\WireMockMappings\";
                if (!System.IO.Directory.Exists(mConfigFileFolderPath))
                {
                    System.IO.Directory.CreateDirectory(mConfigFileFolderPath);
                }

                string filePath = Path.Combine(mConfigFileFolderPath, "WireMockMappings.json");
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
                                    int index = xGridMapping.DataSourceList.IndexOf(mapping);
                                    xGridMapping.DataSourceList[index] = JsonConvert.DeserializeObject<Mapping>(updatedJson);
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

        /// <summary>
        /// Delete all mappings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void xDeleteMappingBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Mapping mapping)
            {
                try
                {
                    HttpResponseMessage result = await wmController.mockAPI.DeleteStubAsync(mapping.Id);
                    if (result.IsSuccessStatusCode)
                    {
                        // Remove the mapping from the grid
                        xGridMapping.DataSourceList.Remove(mapping);
                        Reporter.ToUser(eUserMsgKey.WireMockMappingDeleteSuccess);

                        // Refresh the grid data to ensure the mappings are updated
                        await wmController.DeserializeWireMockResponseAsync();
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