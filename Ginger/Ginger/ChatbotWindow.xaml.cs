using System.Windows;
using System.Windows.Controls;

namespace Ginger
{
    public partial class ChatbotWindow : UserControl
    {
        public ChatbotWindow()
        {
            InitializeComponent();
        }

        private void SendMessage(object sender, RoutedEventArgs e)
        {
            string userInput = txtInput.Text.Trim();

            AddMessage("You", userInput);

            txtInput.Text = "";
            string botResponse = GenerateDummyResponse(userInput);

            AddMessage("Bot", botResponse);
        }
        private void AddMessage(string sender, string message)
        {
            TextBlock newTextBlock = new TextBlock();
            newTextBlock.Text = $"{sender}: {message}";
            newTextBlock.Margin = new Thickness(5);
            chatPanel.Children.Add(newTextBlock);

        }

        private string GenerateDummyResponse(string userMessage)
        {
            return "Welcome to Ginger bot!";
        }
    }
}