using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CakeFlow.Core;
using CakeFlow.Services;

namespace CakeFlow.App
{
    public partial class SelectConfectionerDialog : Window
    {
        public int SelectedUserId { get; private set; }
        private readonly List<User> _confectioners;

        public SelectConfectionerDialog(IOrderService orderService, User currentUser)
        {
            InitializeComponent();
            _confectioners = orderService.GetConfectioners().ToList();

            if (_confectioners.Count == 0)
            {
                MessageBox.Show("Нет доступных кондитеров! Сначала добавьте кондитеров в систему.",
                                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = false;
                Close();
                return;
            }

            lstConfectioners.ItemsSource = _confectioners;
            lstConfectioners.SelectedIndex = 0;
        }

        private void Assign_Click(object sender, RoutedEventArgs e)
        {
            if (lstConfectioners.SelectedItem is User selected)
            {
                SelectedUserId = selected.Id;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Выберите кондитера из списка!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}