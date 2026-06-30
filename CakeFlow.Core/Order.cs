using System;

namespace CakeFlow.Core
{
    public enum OrderStatus
    {
        New,
        InProgress,
        Ready,
        Issued
    }

    public enum UserRole
    {
        Admin,
        Manager,
        Confectioner
    }

    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string CakeName { get; set; } = string.Empty;
        public string CakeType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DesiredDate { get; set; }
        public OrderStatus Status { get; set; }
        public int? AssignedToUserId { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? ReadyAt { get; set; }
        public bool IsProblem { get; set; }
        public string ProblemReason { get; set; } = string.Empty;
        public int? CreatedByUserId { get; set; }
    }
}