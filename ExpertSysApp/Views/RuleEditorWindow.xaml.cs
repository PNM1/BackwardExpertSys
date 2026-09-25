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
using ExpertSysApp.Models;

namespace ExpertSysApp.Views
{
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
            if (LbRules.SelectedItem is Rule selectedRule)
            {
                TxtId.Text = selectedRule.Id.ToString();
                TxtConditions.Text = string.Join(", ", selectedRule.Conditions);
                TxtConclusion.Text = selectedRule.Conclusion;
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            int nextId = _controller.Rules.Any() ? _controller.Rules.Max(r => r.Id) + 1 : 1;
            TxtId.Text = nextId.ToString();
            TxtConditions.Clear();
            TxtConclusion.Clear();
            LbRules.SelectedItem = null;
        }
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtConclusion.Text))
            {
                MessageBox.Show("Заполните поле заключения (результата)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int newId = _controller.Rules.Any() ? _controller.Rules.Max(r => r.Id) + 1 : 1;

            var newRule = new Rule
            {
                Id = newId,
                Conditions = TxtConditions.Text.Split(',')
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToList(),
                Conclusion = TxtConclusion.Text.Trim()
            };

            _controller.Rules.Add(newRule);
            _controller.SaveRules();
            LoadData();
            BtnClear_Click(sender, e);
            MessageBox.Show("Правило успешно добавлено.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (LbRules.SelectedItem is Rule selectedRule)
            {
                selectedRule.Conditions = TxtConditions.Text.Split(',')
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToList();
                selectedRule.Conclusion = TxtConclusion.Text.Trim();

                _controller.SaveRules();
                LoadData();
                MessageBox.Show("Изменения сохранены.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Выберите правило из списка для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (LbRules.SelectedItem is Rule selectedRule)
            {
                var confirm = MessageBox.Show($"Вы действительно хотите удалить правило ID {selectedRule.Id}?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirm == MessageBoxResult.Yes)
                {
                    _controller.Rules.Remove(selectedRule);
                    _controller.SaveRules();
                    LoadData();
                    BtnClear_Click(sender, e);
                }
            }
            else
            {
                MessageBox.Show("Выберите правило для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
