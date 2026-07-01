using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CakeFlow.Core;
using CakeFlow.Services;

namespace CakeFlow.App
{
    public partial class MainWindow : Window
    {
        private readonly IOrderService _orderService;
        private User _currentUser;
        private Order _selectedOrder;

        public MainWindow(User user)
        {
            try
            {
                InitializeComponent();
                _currentUser = user;
                _orderService = new OrderService();
                LoadUserInfo();
                LoadOrders();
                SetPermissions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в MainWindow: {ex.Message}\n\n{ex.StackTrace}", 
                               "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUserInfo()
        {
            if (lblUserFullName != null)
                lblUserFullName.Text = _currentUser.FullName;
            if (lblUserRole != null)
                lblUserRole.Text = _currentUser.Role.ToString();
        }

        private void LoadOrders()
        {
            try
            {
                var orders = _currentUser.Role == UserRole.Confectioner
                    ? _orderService.GetOrdersByConfectioner(_currentUser.Id)
                    : _orderService.GetAllOrders();
                
                if (dgOrders != null)
                    dgOrders.ItemsSource = orders;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetPermissions()
        {
            try
            {
                bool isAdminOrManager = _currentUser.Role == UserRole.Admin || _currentUser.Role == UserRole.Manager;
                bool isConfectioner = _currentUser.Role == UserRole.Confectioner;

                if (btnAssignConfectioner != null)
                    btnAssignConfectioner.Visibility = isAdminOrManager ? Visibility.Visible : Visibility.Collapsed;
                if (btnResolveProblem != null)
                    btnResolveProblem.Visibility = isAdminOrManager ? Visibility.Visible : Visibility.Collapsed;
                if (btnExtendDeadline != null)
                    btnExtendDeadline.Visibility = isAdminOrManager ? Visibility.Visible : Visibility.Collapsed;
                if (btnRequestHelp != null)
                    btnRequestHelp.Visibility = isConfectioner ? Visibility.Visible : Visibility.Collapsed;
                if (btnManageUsers != null)
                    btnManageUsers.Visibility = _currentUser.Role == UserRole.Admin ? Visibility.Visible : Visibility.Collapsed;

                bool hasSelection = _selectedOrder != null;
                if (btnChangeStatus != null)
                    btnChangeStatus.IsEnabled = hasSelection;
                if (btnAssignConfectioner != null)
                    btnAssignConfectioner.IsEnabled = hasSelection && isAdminOrManager;
                if (btnResolveProblem != null)
                    btnResolveProblem.IsEnabled = hasSelection && isAdminOrManager && _selectedOrder?.IsProblem == true;
                if (btnExtendDeadline != null)
                    btnExtendDeadline.IsEnabled = hasSelection && isAdminOrManager;
                if (btnAddComment != null)
                    btnAddComment.IsEnabled = hasSelection;
                if (btnShowComments != null)
                    btnShowComments.IsEnabled = hasSelection;
                if (btnRequestHelp != null)
                    btnRequestHelp.IsEnabled = hasSelection && isConfectioner;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка настройки прав: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void Filter_Changed(object sender, SelectionChangedEventArgs e) => ApplyFilters();

        private void ApplyFilters()
        {
            try
            {
                var searchTerm = txtSearch?.Text ?? "";
                var orders = _orderService.SearchOrders(searchTerm);

                if (cmbStatus?.SelectedItem is ComboBoxItem item && item.Content.ToString() != "Все")
                {
                    // Исправлено: безопасный парсинг с проверкой на null
                    if (Enum.TryParse<OrderStatus>(item.Content.ToString(), out var status))
                    {
                        orders = orders.Where(o => o.Status == status).ToList();
                    }
                }

                if (dgOrders != null)
                    dgOrders.ItemsSource = orders;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Order_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                _selectedOrder = dgOrders?.SelectedItem as Order;
                if (_selectedOrder != null && txtOrderDetails != null)
                {
                    var confectionerName = _selectedOrder.AssignedToUserId.HasValue 
                        ? $"ID: {_selectedOrder.AssignedToUserId}" 
                        : "Не назначен";

                    txtOrderDetails.Text =
                        $"🍰 Торт: {_selectedOrder.CakeName}\n" +
                        $"📌 Тип: {_selectedOrder.CakeType}\n" +
                        $"📝 Описание: {_selectedOrder.Description}\n" +
                        $"👤 Клиент: {_selectedOrder.ClientName}\n" +
                        $"📅 Создан: {_selectedOrder.CreatedAt:dd.MM.yyyy HH:mm}\n" +
                        $"⏰ Желаемая дата: {_selectedOrder.DesiredDate:dd.MM.yyyy}\n" +
                        $"👨‍🍳 Кондитер: {confectionerName}\n" +
                        $"⚠️ Проблема: {(_selectedOrder.IsProblem ? _selectedOrder.ProblemReason : "Нет")}";
                }
                SetPermissions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка выбора заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ManageUsers_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser.Role != UserRole.Admin)
            {
                MessageBox.Show("Доступ запрещен! Только администратор может управлять пользователями.", 
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new ManageUsersWindow();
            dialog.ShowDialog();
        }

        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OrderDialog(_currentUser);
                if (dialog.ShowDialog() == true)
                {
                    _orderService.CreateOrder(dialog.Order);
                    if (!string.IsNullOrWhiteSpace(dialog.Comment))
                    {
                        _orderService.AddComment(dialog.Order.Id, _currentUser.Id, dialog.Comment);
                    }
                    LoadOrders();
                    MessageBox.Show("✅ Заказ создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangeStatus_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrder == null) return;

            var statuses = Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>().ToList();
            var currentStatus = _selectedOrder.Status;
            var nextIndex = statuses.IndexOf(currentStatus) + 1;

            if (nextIndex >= statuses.Count)
            {
                MessageBox.Show("Заказ уже в финальном статусе", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var nextStatus = statuses[nextIndex];
            if (MessageBox.Show($"Изменить статус с '{currentStatus}' на '{nextStatus}'?",
                                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _orderService.ChangeOrderStatus(_selectedOrder.Id, nextStatus, _currentUser.Id);
                LoadOrders();
                MessageBox.Show("✅ Статус изменен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AssignConfectioner_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrder == null) return;

            var dialog = new SelectConfectionerDialog(_orderService, _currentUser);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _orderService.AssignConfectioner(_selectedOrder.Id, dialog.SelectedUserId, _currentUser.Id);
                    LoadOrders();
                    MessageBox.Show("✅ Кондитер назначен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddComment_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrder == null) return;

            var comment = Microsoft.VisualBasic.Interaction.InputBox(
                "💬 Введите комментарий:",
                "Добавление комментария",
                "");

            if (!string.IsNullOrWhiteSpace(comment))
            {
                _orderService.AddComment(_selectedOrder.Id, _currentUser.Id, comment);
                LoadOrders();
                Order_SelectionChanged(null, null);
                MessageBox.Show("✅ Комментарий добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ShowComments_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrder == null) return;

            var comments = _orderService.GetComments(_selectedOrder.Id);
            if (!comments.Any())
            {
                MessageBox.Show("📝 Комментариев нет.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var text = $"📝 КОММЕНТАРИИ К ЗАКАЗУ {_selectedOrder.OrderNumber}\n\n";
            foreach (var comment in comments)
            {
                text += $"👤 {comment.UserId} | {comment.CreatedAt:dd.MM.yyyy HH:mm}\n";
                text += $"💬 {comment.CommentText}\n\n";
            }

            MessageBox.Show(text, "Комментарии", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RequestHelp_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrder == null) return;

            var reason = Microsoft.VisualBasic.Interaction.InputBox(
                "🆘 Опишите проблему:",
                "Запрос помощи менеджеру",
                "");

            if (!string.IsNullOrWhiteSpace(reason))
            {
                _orderService.RequestManagerHelp(_selectedOrder.Id, reason, _currentUser.Id);
                LoadOrders();
                MessageBox.Show("✅ Запрос отправлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ResolveProblem_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrder == null || !_selectedOrder.IsProblem) return;

            if (MessageBox.Show("Проблема решена?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _orderService.ResolveProblem(_selectedOrder.Id, _currentUser.Id);
                LoadOrders();
                MessageBox.Show("✅ Проблема решена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExtendDeadline_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrder == null) return;

            var dialog = new ExtendDeadlineDialog(_selectedOrder);
            if (dialog.ShowDialog() == true)
            {
                _orderService.ExtendDeadline(_selectedOrder.Id, dialog.NewDeadline, _currentUser.Id, dialog.Reason);
                LoadOrders();
                MessageBox.Show("✅ Срок продлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ShowStatistics_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var stats = _orderService.GetStatistics();
                var statsText = $"📊 СТАТИСТИКА\n\n" +
                               $"✅ Выполнено: {stats["TotalIssued"]}\n" +
                               $"⏱ Среднее время: {stats["AverageExecutionHours"]} ч.\n\n" +
                               $"📈 ПО ТИПАМ:\n";

                var cakeStats = stats["CakeTypeStatistics"] as Dictionary<string, int>;
                if (cakeStats != null)
                {
                    foreach (var item in cakeStats)
                        statsText += $"   {item.Key}: {item.Value} шт.\n";
                }

                MessageBox.Show(statsText, "Статистика", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            new LoginWindow().Show();
        }
    }
}