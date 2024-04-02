using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.GenAIServices;
using Ginger.Extensions;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.ObjectModel;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Amdocs.Ginger.UserControls
{
    public partial class ChatbotWindow : UserControl
    {
        BrainAIServices brainAIServices;

        public ChatbotWindow()
        {
            InitializeComponent();
            brainAIServices = new BrainAIServices();

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
            //LisaIntro.Visibility = Visibility.Collapsed;
            string answer;
            string userInput = xUserInputTextBox.Text;
            if (userInput.IsNullOrEmpty())
            {
                // need to change to the specific message
                Reporter.ToUser(eUserMsgKey.EnvParamNameEmpty);
                return;
            }

            AddMessage(GetUserName(), userInput);
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
            AddMessage("Lisa", answer);
        }

        private void AddMessage(string sender, string message)
        {
            TextBlock newTextBlock = new TextBlock();
            newTextBlock.Text = $"{sender}: {message}";
            newTextBlock.Margin = new Thickness(5);
            newTextBlock.TextWrapping = TextWrapping.Wrap;
            newTextBlock.Width = 300;
            chatPanel.Children.Add(newTextBlock);
            

        }

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
    }
}