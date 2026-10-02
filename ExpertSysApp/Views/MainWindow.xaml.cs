using ExpertSysApp.Controllers;
using ExpertSysApp.Models;
using ExpertSysApp.Views;
using Microsoft.Win32;
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

            string targetGoal = TxtTargetGoal.Text.Trim();

            var (result, trace, memoryLog) = _controller.SearchBackward(facts, targetGoal);
            TxtResult.Text = result;
            TxtWorkingMemory.Text = memoryLog;

            var uiTrace = trace.Select(t => {
                Brush color = Brushes.LightCoral;
                if (t.Status == TraceStatus.Success)
                    color = Brushes.LightGreen;
                else if (t.Status == TraceStatus.Partial)
                    color = Brushes.LightYellow;

                return new
                {
                    t.RuleDescription,
                    StatusColor = color
                };
            }).ToList();

            LbTrace.ItemsSource = uiTrace;
        }

        private void ChkTrace_Checked(object sender, RoutedEventArgs e)
        {
            TraceColumnDef.Width = new GridLength(1, GridUnitType.Star);
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
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _controller.LoadRulesFromPath(openFileDialog.FileName);
                MessageBox.Show("База правил успешно загружена из файла.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void SaveFileAs_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                FileName = "rules.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                _controller.SaveRulesToPath(saveFileDialog.FileName);
                MessageBox.Show("База правил успешно сохранена.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}