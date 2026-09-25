using ExpertSysApp.Controllers;
using ExpertSysApp.Models;
using ExpertSysApp.Views;
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
        private Controller _controller;
        public MainWindow()
        {
            InitializeComponent();
            _controller = new Controller();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            var facts = TxtInputFacts.Text.Split(',')
                .Select(f => f.Trim())
                .Where(f => !string.IsNullOrEmpty(f))
                .ToList();

            var (result, trace) = _controller.SearchAnswer(facts);
            TxtResult.Text = result;

            var uiTrace = trace.Select(t => new {
                t.RuleDescription,
                StatusColor = t.Status == TraceStatus.Success ? Brushes.LightGreen : Brushes.LightYellow
            }).ToList();

            LbTrace.ItemsSource = uiTrace;
        }

        private void ChkTrace_Checked(object sender, RoutedEventArgs e)
        {
            TraceColumnDef.Width = new GridLength(360);
            TracePanel.Visibility = Visibility.Visible;
            this.Width = 950;
        }

        private void ChkTrace_Unchecked(object sender, RoutedEventArgs e)
        {
            TraceColumnDef.Width = new GridLength(0);
            TracePanel.Visibility = Visibility.Collapsed;
            this.Width = 600;
        }

        private void OpenEditor_Click(object sender, RoutedEventArgs e)
        {
            var editor = new RuleEditorWindow(_controller);
            editor.ShowDialog();
        }
    }
}