using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MemEditGUI.Views
{
    /// <summary>
    /// How important message is
    /// </summary>
    public enum MessageLevel
    {
        Message,
        Warning,
        Error
    }

    /// <summary>
    /// Interaction logic for MessageBoxView.xaml
    /// </summary>
    public partial class MessageBoxView : Window
    {
       
        public MessageBoxView(string title, string message, MessageLevel level)
        {
            InitializeComponent();
            Title = title;
            lblMessage.Text = message;
            switch(level)
            {
                case MessageLevel.Message:
                    lblMessage.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                    break;
                case MessageLevel.Warning:
                    lblMessage.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));
                    break;
                case MessageLevel.Error:
                    lblMessage.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                    break;
            }
        }

        static MessageBoxView box;
      
        /// <summary>
        /// Show dialog!
        /// </summary>
        public static void Show(string title, string message, MessageLevel level)
        {
            box = new MessageBoxView(title, message, level);
            box.ShowDialog();
        }

        /// <summary>
        /// Ok
        /// </summary>
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        
    }
}
