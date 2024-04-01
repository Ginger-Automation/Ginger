using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace Amdocs.Ginger.UserControls
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
