using System;
using System.Collections.Generic;

namespace Icheon.DTO
{
    public class OrderActiveResponseDTO
    {
        public string order_number { get; set; }
        public DateTime created_at { get; set; }
        public List<OrderItemDTO> order_items { get; set; }
    }
}