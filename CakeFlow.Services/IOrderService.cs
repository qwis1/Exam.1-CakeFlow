using System;
using System.Collections.Generic;
using CakeFlow.Core;

namespace CakeFlow.Services
{
    public interface IOrderService
    {
        List<Order> GetAllOrders();
        List<Order> GetOrdersByConfectioner(int userId);
        Order GetOrderById(int id);
        void CreateOrder(Order order);
        void UpdateOrder(Order order);
        void ChangeOrderStatus(int orderId, OrderStatus newStatus, int userId);
        void AssignConfectioner(int orderId, int confectionerId, int managerId);
        void RequestManagerHelp(int orderId, string reason, int confectionerId);
        void ResolveProblem(int orderId, int managerId);
        void ExtendDeadline(int orderId, DateTime newDesiredDate, int managerId, string reason);
        List<Order> SearchOrders(string searchTerm);
        Dictionary<string, object> GetStatistics();
        List<User> GetConfectioners();
        void AddComment(int orderId, int userId, string text);
        List<OrderComment> GetComments(int orderId);
    }
}