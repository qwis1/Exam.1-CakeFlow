using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using CakeFlow.Data;

namespace CakeFlow.App
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в конструкторе: {ex.Message}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var context = new AppDbContext();
                context.Database.EnsureCreated();

                var login = txtLogin.Text.Trim();
                var password = txtPassword.Password;

                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                {
                    lblError.Text = "❌ Введите логин и пароль!";
                    return;
                }

                var hash = HashPassword(password);
                var user = context.Users.FirstOrDefault(u => u.Login == login && u.PasswordHash == hash);

                if (user != null)
                {
                    this.Hide();
                    var mainWindow = new MainWindow(user);
                    mainWindow.ShowDialog();
                    this.Show();
                    txtLogin.Text = "";
                    txtPassword.Password = "";
                    lblError.Text = "";
                }
                else
                {
                    lblError.Text = "❌ Неверный логин или пароль!";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка входа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                lblError.Text = $"❌ Ошибка: {ex.Message}";
            }
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}