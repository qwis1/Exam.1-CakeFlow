using System;
using System.Collections.Generic;
using System.Linq;
using CakeFlow.Core;
using CakeFlow.Data;

namespace CakeFlow.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService()
        {
            _context = new AppDbContext();
            _context.Database.EnsureCreated();
        }

        public List<Order> GetAllOrders()
        {
            return _context.Orders.ToList();
        }

        public List<Order> GetOrdersByConfectioner(int userId)
        {
            return _context.Orders
                .Where(o => o.AssignedToUserId == userId)
                .ToList();
        }

        public Order GetOrderById(int id)
        {
            return _context.Orders.FirstOrDefault(o => o.Id == id);
        }

        public void CreateOrder(Order order)
        {
            order.OrderNumber = $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4)}";
            order.CreatedAt = DateTime.Now;
            order.Status = OrderStatus.New;
            _context.Orders.Add(order);
            _context.SaveChanges();

            AddHistory(order.Id, null, OrderStatus.New.ToString(), order.CreatedByUserId ?? 1);
        }

        public void UpdateOrder(Order order)
        {
            var existing = _context.Orders.Find(order.Id);
            if (existing != null)
            {
                existing.ClientName = order.ClientName;
                existing.CakeName = order.CakeName;
                existing.CakeType = order.CakeType;
                existing.Description = order.Description;
                existing.DesiredDate = order.DesiredDate;
                _context.SaveChanges();
            }
        }

        public void ChangeOrderStatus(int orderId, OrderStatus newStatus, int userId)
        {
            var order = _context.Orders.Find(orderId);
            if (order == null) throw new Exception("Заказ не найден");

            var oldStatus = order.Status.ToString();
            order.Status = newStatus;

            if (newStatus == OrderStatus.InProgress && order.StartedAt == null)
                order.StartedAt = DateTime.Now;

            if (newStatus == OrderStatus.Ready)
                order.ReadyAt = DateTime.Now;

            _context.SaveChanges();
            AddHistory(orderId, oldStatus, newStatus.ToString(), userId);
            
            // Уведомление о смене статуса
            AddComment(orderId, userId, $"📢 СТАТУС ИЗМЕНЕН: {oldStatus} → {newStatus}");
        }

        public void AssignConfectioner(int orderId, int confectionerId, int managerId)
        {
            var order = _context.Orders.Find(orderId);
            if (order == null) throw new Exception("Заказ не найден");

            var confectioner = _context.Users.Find(confectionerId);
            if (confectioner == null || confectioner.Role != UserRole.Confectioner)
                throw new Exception("Выбранный пользователь не является кондитером");

            order.AssignedToUserId = confectionerId;
            order.Status = OrderStatus.InProgress;
            order.StartedAt = DateTime.Now;
            _context.SaveChanges();

            AddHistory(orderId, null, $"Назначен кондитер {confectioner.FullName}", managerId);
            
            // Уведомление о назначении
            AddComment(orderId, managerId, $"📢 НАЗНАЧЕН КОНДИТЕР: {confectioner.FullName}");
        }

        public void RequestManagerHelp(int orderId, string reason, int confectionerId)
        {
            var order = _context.Orders.Find(orderId);
            if (order == null) throw new Exception("Заказ не найден");

            order.IsProblem = true;
            order.ProblemReason = reason;
            _context.SaveChanges();

            AddComment(orderId, confectionerId, $"🆘 ЗАПРОС ПОМОЩИ: {reason}");
        }

        public void ResolveProblem(int orderId, int managerId)
        {
            var order = _context.Orders.Find(orderId);
            if (order == null) throw new Exception("Заказ не найден");

            order.IsProblem = false;
            order.ProblemReason = null;
            _context.SaveChanges();

            AddComment(orderId, managerId, "✅ ПРОБЛЕМА РЕШЕНА менеджером");
        }

        public void ExtendDeadline(int orderId, DateTime newDesiredDate, int managerId, string reason)
        {
            var order = _context.Orders.Find(orderId);
            if (order == null) throw new Exception("Заказ не найден");

            var oldDate = order.DesiredDate;
            order.DesiredDate = newDesiredDate;
            _context.SaveChanges();

            AddComment(orderId, managerId, $"📅 СРОК ПЕРЕНЕСЕН с {oldDate:dd.MM.yyyy} на {newDesiredDate:dd.MM.yyyy}. Причина: {reason}");
        }

        public List<Order> SearchOrders(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllOrders();

            return _context.Orders
                .Where(o => o.OrderNumber.Contains(searchTerm) ||
                            o.ClientName.Contains(searchTerm) ||
                            o.CakeName.Contains(searchTerm))
                .ToList();
        }

        public List<User> GetConfectioners()
        {
            return _context.Users
                .Where(u => u.Role == UserRole.Confectioner)
                .ToList();
        }

        public Dictionary<string, object> GetStatistics()
        {
            var stats = new Dictionary<string, object>();

            var totalIssued = _context.Orders.Count(o => o.Status == OrderStatus.Issued);
            stats.Add("TotalIssued", totalIssued);

            var completedOrders = _context.Orders
                .Where(o => o.ReadyAt.HasValue && o.StartedAt.HasValue)
                .ToList();

            if (completedOrders.Any())
            {
                var avgHours = completedOrders.Average(o =>
                    (o.ReadyAt.Value - o.StartedAt.Value).TotalHours);
                stats.Add("AverageExecutionHours", Math.Round(avgHours, 1));
            }
            else
            {
                stats.Add("AverageExecutionHours", 0);
            }

            var cakeStats = _context.Orders
                .Where(o => o.Status == OrderStatus.Issued)
                .GroupBy(o => o.CakeType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionary(k => k.Type, v => v.Count);
            stats.Add("CakeTypeStatistics", cakeStats);

            var confectionerLoad = _context.Users
                .Where(u => u.Role == UserRole.Confectioner)
                .Select(u => new
                {
                    u.FullName,
                    InProgress = _context.Orders.Count(o => o.AssignedToUserId == u.Id && o.Status == OrderStatus.InProgress)
                })
                .ToDictionary(k => k.FullName, v => v.InProgress);
            stats.Add("ConfectionerLoad", confectionerLoad);

            return stats;
        }

        public void AddComment(int orderId, int userId, string text)
        {
            _context.OrderComments.Add(new OrderComment
            {
                OrderId = orderId,
                UserId = userId,
                CommentText = text,
                CreatedAt = DateTime.Now
            });
            _context.SaveChanges();
        }

        public List<OrderComment> GetComments(int orderId)
        {
            return _context.OrderComments
                .Where(c => c.OrderId == orderId)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
        }

        private void AddHistory(int orderId, string from, string to, int userId)
        {
            _context.OrderHistories.Add(new OrderHistory
            {
                OrderId = orderId,
                StatusFrom = from,
                StatusTo = to,
                ChangedAt = DateTime.Now,
                ChangedByUserId = userId
            });
            _context.SaveChanges();
        }
    }
}