namespace Icheon.DTO
{
    public class OrderItemDTO
    {
        public BagTypeDTO bag_type { get; set; }
        public int quantity { get; set; }
        public int get_produced_count { get; set; }
        public int get_remaining_count { get; set; }
    }
}