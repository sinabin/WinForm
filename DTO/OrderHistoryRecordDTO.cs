using System;

namespace Icheon.DTO
{
    public class OrderHistoryRecordDTO
    {
        public long order_number { get; set; }
        public DateTime order_date { get; set; }
        public string usage { get; set; }
        public string liter { get; set; }
        public int order_amount { get; set; }
    }
}