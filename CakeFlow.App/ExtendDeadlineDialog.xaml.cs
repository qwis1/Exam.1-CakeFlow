using System;
using System.Windows;
using CakeFlow.Core;

namespace CakeFlow.App
{
    public partial class ExtendDeadlineDialog : Window
    {
        public DateTime NewDeadline { get; private set; }
        public string Reason { get; private set; }

        public ExtendDeadlineDialog(Order order)
        {
            InitializeComponent();
            lblCurrentDate.Text = order.DesiredDate.ToString("dd.MM.yyyy");
            dpNewDeadline.SelectedDate = order.DesiredDate.AddDays(3);
        }

        private void Extend_Click(object sender, RoutedEventArgs e)
        {
            if (dpNewDeadline.SelectedDate == null)
            {
                MessageBox.Show("Выберите новую дату!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                dpNewDeadline.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                MessageBox.Show("Укажите причину продления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtReason.Focus();
                return;
            }

            NewDeadline = dpNewDeadline.SelectedDate.Value;
            Reason = txtReason.Text.Trim();

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}