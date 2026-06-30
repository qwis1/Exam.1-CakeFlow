using System;
using System.Windows;
using System.Windows.Controls;
using CakeFlow.Core;

namespace CakeFlow.App
{
    public partial class OrderDialog : Window
    {
        public Order Order { get; private set; }
        public string Comment { get; private set; } = string.Empty;
        private readonly User _currentUser;
        private readonly bool _isEditMode;
        private readonly Order _existingOrder;

        public OrderDialog(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _isEditMode = false;
            Title = "🍰 Новый заказ";
            btnSave.Content = "✅ Создать";
        }

        public OrderDialog(User currentUser, Order existingOrder) : this(currentUser)
        {
            _existingOrder = existingOrder;
            _isEditMode = true;
            Title = "✏️ Редактирование заказа";
            btnSave.Content = "💾 Сохранить";
            LoadOrderData();
        }

        private void LoadOrderData()
        {
            txtClient.Text = _existingOrder.ClientName;
            txtCakeName.Text = _existingOrder.CakeName;
            txtDescription.Text = _existingOrder.Description;
            dpDesiredDate.SelectedDate = _existingOrder.DesiredDate;

            for (int i = 0; i < cmbCakeType.Items.Count; i++)
            {
                var item = cmbCakeType.Items[i] as ComboBoxItem;
                if (item != null && item.Content.ToString() == _existingOrder.CakeType)
                {
                    cmbCakeType.SelectedIndex = i;
                    break;
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtClient.Text))
            {
                MessageBox.Show("Введите имя клиента!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtClient.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCakeName.Text))
            {
                MessageBox.Show("Введите название торта!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCakeName.Focus();
                return;
            }

            if (dpDesiredDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату получения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                dpDesiredDate.Focus();
                return;
            }

            if (dpDesiredDate.SelectedDate < DateTime.Today)
            {
                MessageBox.Show("Дата получения не может быть в прошлом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                dpDesiredDate.Focus();
                return;
            }

            Comment = txtComment.Text.Trim();

            if (_isEditMode)
            {
                Order = _existingOrder;
                Order.ClientName = txtClient.Text.Trim();
                Order.CakeName = txtCakeName.Text.Trim();
                Order.CakeType = (cmbCakeType.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Другой";
                Order.Description = txtDescription.Text.Trim();
                Order.DesiredDate = dpDesiredDate.SelectedDate.Value;
            }
            else
            {
                Order = new Order
                {
                    ClientName = txtClient.Text.Trim(),
                    CakeName = txtCakeName.Text.Trim(),
                    CakeType = (cmbCakeType.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Другой",
                    Description = txtDescription.Text.Trim(),
                    DesiredDate = dpDesiredDate.SelectedDate.Value,
                    CreatedByUserId = _currentUser.Id
                };
            }

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