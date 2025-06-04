using System.Collections.Generic;

namespace Icheon.DTO
{
    public class WorkPlanCompletionRequestDTO
    {
        public string production_date { get; set; }
        public string printer_number { get; set; }
        public string order_id { get; set; }
        public List<WorkPlanItemDTO> items { get; set; }
    }
}