using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.GenAIServices;
using Ginger.Extensions;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Amdocs.Ginger.UserControls
{
    public partial class ChatbotWindow : UserControl
    {
        BrainAIServices brainAIServices;

        public ChatbotWindow()
        {
            InitializeComponent();
            brainAIServices = new BrainAIServices();
            string introMessage = "Hello I'm Lisa, the Ginger AI Assistent. How can i help you today?";
            AddMessage("Lisa", introMessage, false);
        }


        private string GetUserName()
        {
            string userName;
            if (String.IsNullOrEmpty(WorkSpace.Instance.UserProfile.UserFirstName))
            {
                userName = WorkSpace.Instance.UserProfile.UserName;
            }
            else
            {
                userName = WorkSpace.Instance.UserProfile.UserFirstName;
            }
            if (userName.Length > 10)
            {
                userName = userName.Substring(0, 7) + "...";
            }

            return userName;
        }

        private async void SendMessage(object sender, RoutedEventArgs e)
        {
            await SendMessageToAPI();
        }

        private async Task SendMessageToAPI()
        {
            string answer;
            string userInput = xUserInputTextBox.Text;
            if (userInput.IsNullOrEmpty())
            {
                // need to change to the specific message
                Reporter.ToUser(eUserMsgKey.EnvParamNameEmpty);
                return;
            }

            AddMessage(GetUserName(), userInput, true);
            ShowLoader();
            try
            {
                if (chatPanel.Children.Count == 1)
                {
                    answer = await brainAIServices.StartNewChat(userInput);
                    chatPanel.Children.Clear();
                }
                else
                {
                    answer = await brainAIServices.ContinueChat(userInput);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Unable to connect to the host", ex);
                answer = "Sorry, I am unable to answer right now";
            }
            finally
            {
                HideLoader();
            }
            AddMessage("Lisa", answer, false);
        }


        private void AddMessage(string sender, string message, bool isUserMessage)
        {
            StackPanel messageContainer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(5),
                MaxWidth = 400
            };

            TextBlock messageText = new TextBlock
            {
                Text = message,
                //Background = isUserMessage ? System.Windows.Media.Brushes.LightBlue : System.Windows.Media.Brushes.LightGray,
                Padding = new Thickness(10),
                MaxWidth = 300,
                TextWrapping = TextWrapping.Wrap
            };
            // Create a border with curved corners
            Border messageBorder = new Border
            {
                CornerRadius = new CornerRadius(10), // Adjust corner radius as needed
                Background = isUserMessage ? System.Windows.Media.Brushes.LightBlue : System.Windows.Media.Brushes.LightGray,
                Child = messageText, // Set the TextBlock as the child of the border
                Margin = new Thickness(0, 0, 0, 5) // Add margin at the bottom for spacing
            };

            messageContainer.Children.Add(messageBorder);

            // Add user icon based on the message sender
            if (isUserMessage)
            {
                // Add user icon (right side)
                messageContainer.HorizontalAlignment = HorizontalAlignment.Left;

                //messageContainer.Children.Add(new System.Windows.Controls.Image
                //{
                //    Source = ImageMakerControl.GetImageSource(Amdocs.Ginger.Common.Enums.eImageType.User,
                //    foreground: (System.Windows.Media.SolidColorBrush)FindResource("$BackgroundColor_DarkGray")),
                //    VerticalAlignment = VerticalAlignment.Top,
                //    Width = 30,
                //    Height = 30,
                //    Margin = new Thickness(5),
                //    ToolTip = sender
                //});

                messageContainer.Children.Insert(0, new System.Windows.Controls.Image
                {
                    Source = ImageMakerControl.GetImageSource(Amdocs.Ginger.Common.Enums.eImageType.User,
                    foreground: (System.Windows.Media.SolidColorBrush)FindResource("$BackgroundColor_DarkGray")),
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 30,
                    Height = 30,
                    Margin = new Thickness(5),
                    ToolTip = sender
                });
            }
            else
            {
                // Add user icon (left side)
                messageContainer.HorizontalAlignment = HorizontalAlignment.Left;

                messageContainer.Children.Insert(0, new System.Windows.Controls.Image
                {
                    Source = ImageMakerControl.GetImageSource(Amdocs.Ginger.Common.Enums.eImageType.User,
                    foreground: (System.Windows.Media.SolidColorBrush)FindResource("$BackgroundColor_DarkGray")),
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 30,
                    Height = 30,
                    Margin = new Thickness(5),
                    ToolTip = sender
                });
            }

            // Add time below the message
            //TextBlock timeText = new TextBlock
            //{
            //    Text = DateTime.Now.ToString("HH:mm"), // Display current time in HH:mm format
            //    FontSize = 10,
            //    FontStyle = FontStyles.Italic,
            //    HorizontalAlignment = HorizontalAlignment.Right,
            //    Margin = new Thickness(5, 0, 5, 0) // Adjust margin for spacing
            //};
            //messageContainer.Children.Add(timeText);


            chatPanel.Children.Add(messageContainer);
        }
        //private void AddMessage(string sender, string message)
        //{
        //    TextBlock newTextBlock = new TextBlock();

        //    newTextBlock.Inlines.Add(new Run("Bold text") { FontWeight = FontWeights.Bold });
        //    newTextBlock.Inlines.Add(new Bold(new Run("TextBlock")));
        //    boldRun.FontWeight = FontWeights.Bold;

        //    newTextBlock.Text = $"{sender}: {message}";
        //    newTextBlock.Margin = new Thickness(5);
        //    newTextBlock.TextWrapping = TextWrapping.Wrap;
        //    newTextBlock.Width = 300;
        //    chatPanel.Children.Add(newTextBlock);


        //}

        private async void TextBox_KeyDown(object sender, KeyEventArgs e)
        {


            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.Enter)
            {
                xUserInputTextBox.AppendText(Environment.NewLine);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                await SendMessageToAPI();
                e.Handled = true;
            }
        }


        private void Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            chatPanel.Children.Clear();
        }

        private void ShowLoader()
        {
            xLoader.Visibility = Visibility.Visible;
            xUserInputTextBox.Clear();
            xUserInputTextBox.IsEnabled = false;
            // xSend.IsEnabled = false;


        }

        private void HideLoader()
        {
            xLoader.Visibility = Visibility.Collapsed;
            xUserInputTextBox.IsEnabled = true;
            //xSend.IsEnabled = true;
        }

        private void ScrollToBottom()
        {
            if (scrollViewer != null && scrollViewer.ScrollableHeight > 0)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.ScrollableHeight);
            }
        }
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Auto-scroll to the bottom when a new message is added
            scrollViewer.ScrollToBottom();
        }

        private void xUserInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ControlTemplate template = xUserInputTextBox.Template;
            TextBlock xy = (TextBlock)template.FindName("xPlaceholder", xUserInputTextBox);
            xy.Visibility = string.IsNullOrEmpty(xUserInputTextBox.Text) ? Visibility.Visible : Visibility.Hidden;
        }

        
    }
}