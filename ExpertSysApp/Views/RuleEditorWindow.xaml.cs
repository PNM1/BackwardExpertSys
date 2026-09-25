using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using ExpertSysApp.Controllers;

namespace ExpertSysApp.Views
{
    /// <summary>
    /// Логика взаимодействия для RuleEditorWindow.xaml
    /// </summary>
    public partial class RuleEditorWindow : Window
    {
        private Controller _controller;

        public RuleEditorWindow(Controller controller)
        {
            InitializeComponent();
            _controller = controller;
            LoadData();
        }

        private void LoadData()
        {
            LbRules.ItemsSource = null;
            LbRules.ItemsSource = _controller.Rules;
        }

        private void LbRules_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
        }
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
        }
        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
