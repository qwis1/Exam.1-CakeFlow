using System;

namespace CakeFlow.Core
{
    public class OrderHistory
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string StatusFrom { get; set; } = string.Empty;
        public string StatusTo { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public int ChangedByUserId { get; set; }
    }
}