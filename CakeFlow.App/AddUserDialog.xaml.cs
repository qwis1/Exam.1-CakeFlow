using System;
using System.Windows;
using System.Windows.Controls;
using CakeFlow.Core;

namespace CakeFlow.App
{
    public partial class AddUserDialog : Window
    {
        public string Login { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;
        public string FullName { get; private set; } = string.Empty;
        public UserRole Role { get; private set; }

        public AddUserDialog()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLogin.Text))
            {
                MessageBox.Show("Введите логин!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtLogin.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Введите пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPassword.Focus();
                return;
            }

            if (txtPassword.Password.Length < 4)
            {
                MessageBox.Show("Пароль должен быть не менее 4 символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPassword.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Введите ФИО!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFullName.Focus();
                return;
            }

            Login = txtLogin.Text.Trim();
            Password = txtPassword.Password;
            FullName = txtFullName.Text.Trim();

            var selectedItem = cmbRole.SelectedItem as ComboBoxItem;
            Role = selectedItem?.Tag?.ToString() switch
            {
                "Admin" => UserRole.Admin,
                "Manager" => UserRole.Manager,
                "Confectioner" => UserRole.Confectioner,
                _ => UserRole.Confectioner
            };

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