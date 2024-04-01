using ICSharpCode.AvalonEdit.Rendering;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Amdocs.Ginger.UserControls
{
    public partial class ChatbotWindow : UserControl
    {
        public ChatbotWindow()
        {
            InitializeComponent();
        }

        private void SendMessage(object sender, RoutedEventArgs e)
        {
            string userInput = "Hello";// txtInput.Text.Trim();

            AddMessage("You", userInput);

            //txtInput.Text = "";
            string botResponse = "test";// GenerateDummyResponse(userInput);

            AddMessage("Lisa", botResponse);
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
            return "Welcome to Ginger, I'm Lisa";
        }

        

    }
}