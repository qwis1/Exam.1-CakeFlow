using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using CakeFlow.Data;

namespace CakeFlow.App
{
    public partial class LoginWindow : Window
    {
        // Лог-файл для отладки
        private static readonly string LogPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "login_debug.log"
        );

        private void Log(string message)
        {
            try
            {
                File.AppendAllText(LogPath, $"{DateTime.Now:HH:mm:ss} - {message}\n");
            }
            catch { }
        }

        public LoginWindow()
        {
            InitializeComponent();
            Log("=== ОКНО ВХОДА СОЗДАНО ===");
            Log($"Путь к папке: {AppDomain.CurrentDomain.BaseDirectory}");
        }

        private void Login_Click(object sender, RoutedEventArgs e)
{
    Log("=== НАЖАТА КНОПКА ВХОДА ===");
    
    try
    {
        var login = txtLogin.Text.Trim();
        var password = txtPassword.Password;

        Log($"Логин: {login}");

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            lblError.Text = "❌ Введите логин и пароль!";
            Log("Ошибка: пустые логин или пароль");
            return;
        }

        Log("Создание контекста БД...");
        using var context = new AppDbContext();
        Log("Контекст создан");

        Log("Вызов EnsureCreated()...");
        context.Database.EnsureCreated();
        Log("EnsureCreated() выполнен");

        var hash = HashPassword(password);
        Log($"Хэш пароля: {hash}");

        Log("Поиск пользователя в БД...");
        var user = context.Users.FirstOrDefault(u => u.Login == login && u.PasswordHash == hash);
        Log($"Пользователь найден: {(user != null ? "ДА" : "НЕТ")}");

        if (user != null)
        {
            Log($"Успешный вход пользователя: {user.FullName} (ID: {user.Id}, Роль: {user.Role})");
            
            MessageBox.Show($"Добро пожаловать, {user.FullName}!", "Успешный вход", MessageBoxButton.OK, MessageBoxImage.Information);
            
            this.Hide();
            Log("Окно входа скрыто. Создание MainWindow...");
            
            try
            {
                var mainWindow = new MainWindow(user);
                Log("MainWindow создан. Показ...");
                mainWindow.ShowDialog();
                Log("MainWindow.ShowDialog() завершен");
            }
            catch (Exception ex)
            {
                Log($"ОШИБКА при создании MainWindow: {ex.Message}");
                MessageBox.Show($"Ошибка открытия главного окна: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            this.Show();
            Log("Окно входа показано снова");
            txtLogin.Text = "";
            txtPassword.Password = "";
            lblError.Text = "";
        }
        else
        {
            Log("Пользователь не найден");
            var userCount = context.Users.Count();
            Log($"Всего пользователей в БД: {userCount}");
            if (userCount == 0)
            {
                lblError.Text = "❌ В базе нет пользователей! Перезапустите приложение.";
            }
            else
            {
                lblError.Text = "❌ Неверный логин или пароль!";
            }
        }
    }
    catch (Exception ex)
    {
        Log($"КРИТИЧЕСКАЯ ОШИБКА: {ex.Message}");
        MessageBox.Show($"Ошибка: {ex.Message}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        lblError.Text = $"❌ Ошибка: {ex.Message}";
    }

    Log("=== ОКОНЧАНИЕ Login_Click ===");
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