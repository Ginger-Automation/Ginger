using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.GenAIServices;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.ServiceModel.Syndication;
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
            LisaIntro.Visibility = Visibility.Collapsed;
            string answer;
            string userInput = "Hello";
            if (chatPanel.Children.Count == 0)
            {
               answer =await brainAIServices.StartNewChat(userInput);

            }
            else
            {
                answer = await brainAIServices.ContinueChat(userInput);
            }

            AddMessage(GetUserName(), userInput);
            //txtInput.Text = "";
            //string botResponse = "test";// GenerateDummyResponse(userInput);

            AddMessage("Lisa", answer);
        }
        private void AddMessage(string sender, string message)
        {
            TextBlock newTextBlock = new TextBlock();
            newTextBlock.Text = $"{sender}: {message}";
            newTextBlock.Margin = new Thickness(5);
            chatPanel.Children.Add(newTextBlock);

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           
        }
    }
}