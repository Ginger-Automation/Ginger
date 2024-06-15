﻿using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using GingerCoreNET.GenAIServices;
using Ginger;
using Ginger.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Amdocs.Ginger.UserControls
{
    public partial class ChatbotWindow : UserControl
    {
        GenAIServiceHelper genAIServiceHelper;
        static List<(string, string)> messages = [];
        public ChatbotWindow()
        {
            InitializeComponent();
            xProfileImageImgBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/Lisa.jpg", UriKind.RelativeOrAbsolute));
            //string introMessage = "Hello I'm Lisa, the Ginger AI Assistent. How can i help you today?";
            // AddMessage("Lisa", introMessage, false);
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
                // Reporter.ToUser(eUserMsgKey.EnvParamNameEmpty);
                return;
            }
            xLisaIntroPanel.Visibility = Visibility.Collapsed;
            AddMessage(GetUserName(), userInput, true);
            ShowLoader();
            try
            {

                genAIServiceHelper = new GenAIServiceHelper();


                if (chatPanel.Children.Count == 1)
                {
                   
                    answer = await genAIServiceHelper.StartNewChat(userInput);
                }
                else
                {
                    answer = await genAIServiceHelper.ContinueChat(userInput);
                }

                if(answer == null)
                {                    
                    answer = "Sorry, seems like there is an issue, i'm not able to reply, please check it with support team.";
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


        private async void AddMessage(string sender, string message, bool isUserMessage)
        {
            messages.Add((sender, message));
            StackPanel messageContainer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(5),
                MaxWidth = 400
            };

            TextBlock messageText = new TextBlock
            {
                Text = isUserMessage ? message: "",
                //Background = isUserMessage ? System.Windows.Media.Brushes.LightBlue : System.Windows.Media.Brushes.LightGray,
                Padding = new Thickness(10),
                MaxWidth = 315,
                TextWrapping = TextWrapping.Wrap
            };
            // Create a border with curved corners
            Border messageBorder = new Border
            {
                CornerRadius = new CornerRadius(10), // Adjust corner radius as needed
                Background = isUserMessage ? System.Windows.Media.Brushes.AliceBlue : System.Windows.Media.Brushes.WhiteSmoke,
                Child = messageText, // Set the TextBlock as the child of the border
                Margin = new Thickness(0, 0, 0, 5) // Add margin at the bottom for spacing
            };

            ImageMakerControl copyButton = new ImageMakerControl
            {
                ImageType = Common.Enums.eImageType.Copy,
                Visibility = Visibility.Collapsed, // Initially hide the copy button
                Width = 10
            };



            // Add the copy button to the message container

            messageContainer.Children.Add(messageBorder);
            messageContainer.Children.Add(copyButton);
            EllipseGeometry ellipse = new EllipseGeometry(new System.Windows.Point(12.5, 15), 15, 15);            // Add user icon based on the message sender
            if (isUserMessage)
            {
                // Add user icon (right side)
                messageContainer.HorizontalAlignment = HorizontalAlignment.Left;

                messageContainer.Children.Insert(0, new System.Windows.Controls.Image
                {
                    Source = string.IsNullOrEmpty(WorkSpace.Instance.UserProfile.ProfileImage) ? ImageMakerControl.GetImageSource(Amdocs.Ginger.Common.Enums.eImageType.User,
                    foreground: (System.Windows.Media.SolidColorBrush)FindResource("$BackgroundColor_DarkGray")) : General.GetImageStream(General.Base64StringToImage(WorkSpace.Instance.UserProfile.ProfileImage)),
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 30,
                    Height = 30,
                    Margin = new Thickness(5),
                    ToolTip = sender,
                    Clip = ellipse,
                });
            }
            else
            {
                // Add user icon (left side)
                messageContainer.HorizontalAlignment = HorizontalAlignment.Left;

                messageContainer.Children.Insert(0, new System.Windows.Controls.Image
                {
                    // Source = ImageMakerControl.GetImageSource(Amdocs.Ginger.Common.Enums.eImageType.User, foreground: (System.Windows.Media.SolidColorBrush)FindResource("$BackgroundColor_DarkGray")),
                    Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/Lisa.jpg", UriKind.RelativeOrAbsolute)),
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 30,
                    Height = 30,
                    Margin = new Thickness(5),
                    ToolTip = sender,
                    Clip = ellipse
                });
            }

            messageContainer.MouseEnter += (sender, e) =>
            {
                copyButton.Visibility = Visibility.Visible;
            };

            // Handle MouseLeave event to hide the copy button
            messageContainer.MouseLeave += (sender, e) =>
            {
                copyButton.Visibility = Visibility.Collapsed;
            };

            // Handle Click event of the copy button
            copyButton.MouseDown += (sender, e) =>
            {
                // Get the message from the Tag property of the Grid
                //string messageToCopy = ((StackPanel)sender).Tag.ToString();
                string message = ((Border)((sender as FrameworkElement).Parent as StackPanel).Children[1]).Child.GetValue(TextBlock.TextProperty).ToString();
                // Copy the message to the clipboard
                Clipboard.SetText(message);
            };

            // Add time below the message  new BitmapImage(new Uri(@"/Images/" + ImageFile, UriKind.RelativeOrAbsolute))
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
            xScrollViewer.ScrollToBottom();
            if (!isUserMessage)
            {
                foreach (char c in message)
                {
                    messageText.Text += c;
                    await Task.Delay(20); // Adjust typing speed here
                }
            }
        }
        private async void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers == ModifierKeys.Control ||
                Keyboard.Modifiers == ModifierKeys.Shift) &&
                e.Key == Key.Enter)
            {
                TextBox textBox = sender as TextBox;
                int caretIndex = textBox.CaretIndex;
                textBox.Text = textBox.Text.Insert(caretIndex, Environment.NewLine);
                textBox.CaretIndex = caretIndex + Environment.NewLine.Length;
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
            //chatPanel.Children.Clear();
        }

        private void ShowLoader()
        {
            xLoader.Visibility = Visibility.Visible;
            xUserInputTextBox.Clear();
            xUserInputTextBox.IsEnabled = false;
            xNewChat.IsEnabled = false;
            // xSend.IsEnabled = false;


        }

        private void HideLoader()
        {
            xLoader.Visibility = Visibility.Collapsed;
            xUserInputTextBox.IsEnabled = true;
            xNewChat.IsEnabled = true;
            //xSend.IsEnabled = true;
        }

        //private void ScrollToBottom()
        //{
        //    if (xScrollViewer != null && xScrollViewer.ScrollableHeight > 0)
        //    {
        //        xScrollViewer.ScrollToVerticalOffset(xScrollViewer.ScrollableHeight);
        //    }
        //}

        private void xUserInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            if (textBox != null)
            {
                TextBlock placeholder = FindPlaceholder(textBox);
                if (placeholder != null)
                {
                    if (!string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        placeholder.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        placeholder.Visibility = Visibility.Visible;
                    }
                }
            }
        }
        private TextBlock FindPlaceholder(Visual parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                Visual child = (Visual)VisualTreeHelper.GetChild(parent, i);
                if (child is TextBlock && ((TextBlock)child).Name == "xPlaceholder")
                {
                    return (TextBlock)child;
                }
                else
                {
                    TextBlock placeholder = FindPlaceholder(child);
                    if (placeholder != null)
                        return placeholder;
                }
            }
            return null;
        }

        private void xNewChat_Click(object sender, RoutedEventArgs e)
        {
            chatPanel.Children.Clear();
            xLisaIntroPanel.Visibility = Visibility.Visible;
            messages.Clear();
        }

        private void xCopyAll_Click(object sender, RoutedEventArgs e)
        {
            if (messages.Count != 0)
            {
                StringBuilder sb = new StringBuilder();

                foreach (var item in messages)
                {
                    sb.Append($"{item.Item1}: {item.Item2}");
                }

                Clipboard.SetText(sb.ToString());
            }
        }
    }
}