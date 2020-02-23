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
    /// Interaction logic for InputBoxView.xaml
    /// </summary>
    public partial class InputBoxView : Window
    {

        public InputBoxView(bool showWarning)
        {
            InitializeComponent();
            lblWarning.Visibility = showWarning ? Visibility.Visible : Visibility.Hidden;
        }

        static InputBoxView box;
        static string dialogResult;
        public static string Show(bool showWarning)
        {
            box = new InputBoxView(showWarning);

            box.ShowDialog();
            return dialogResult;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            dialogResult = tbxValue.Text;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
