using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExpertSysApp
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ChkTrace_Checked(object sender, RoutedEventArgs e)
        {
            TraceColumnDef.Width = new GridLength(360);
            TracePanel.Visibility = Visibility.Visible;
            this.Width = 1150;
        }

        private void ChkTrace_Unchecked(object sender, RoutedEventArgs e)
        {
            TraceColumnDef.Width = new GridLength(0);
            TracePanel.Visibility = Visibility.Collapsed;
            this.Width = 800;
        }

        private void OpenEditor_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}