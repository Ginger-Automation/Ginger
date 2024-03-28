using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace Ginger
{
    public partial class ChatbotIcon : UserControl
    {
        public ChatbotIcon()
        {
            InitializeComponent();
        }


        private void ChatbotIcon_Click(object sender, RoutedEventArgs e)
        {
            ChatbotWindow chatWindow = new ChatbotWindow();
           
            
        }
    }
}
