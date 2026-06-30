using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CakeFlow.Core;
using CakeFlow.Data;

namespace CakeFlow.App
{
    public partial class ManageUsersWindow : Window
    {
        private readonly AppDbContext _context;
        private User _selectedUser;

        public ManageUsersWindow()
        {
            InitializeComponent();
            _context = new AppDbContext();
            LoadUsers();
        }

        private void LoadUsers()
        {
            var users = _context.Users.OrderBy(u => u.Id).ToList();
            dgUsers.ItemsSource = users;
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddUserDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    if (_context.Users.Any(u => u.Login == dialog.Login))
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка", 
                                       MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var user = new User
                    {
                        Login = dialog.Login,
                        PasswordHash = HashPassword(dialog.Password),
                        FullName = dialog.FullName,
                        Role = dialog.Role
                    };

                    _context.Users.Add(user);
                    _context.SaveChanges();
                    LoadUsers();
                    MessageBox.Show("✅ Пользователь добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для удаления!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_selectedUser.Login == "admin")
            {
                MessageBox.Show("Нельзя удалить главного администратора!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить пользователя '{_selectedUser.FullName}'?",
                               "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Users.Remove(_selectedUser);
                    _context.SaveChanges();
                    LoadUsers();
                    MessageBox.Show("✅ Пользователь удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedUser = dgUsers.SelectedItem as User;
        }

        private static string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }

    public class RoleColorConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is UserRole role)
            {
                return role switch
                {
                    UserRole.Admin => "#D32F2F",
                    UserRole.Manager => "#1976D2",
                    UserRole.Confectioner => "#388E3C",
                    _ => "Black"
                };
            }
            return "Black";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}