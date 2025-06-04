namespace Icheon.DTO
{
    // 중앙서버 API의 ReqBody 값이 변경될 경우 수정필요
    public class WorkPlanAssignmentDTO
    {
        public string ProductionLine { get; set; }
        public string Usage { get; set; }
        public string CurrentPrintText { get; set; }
        public int PlanningAmount { get; set; }
        public int Order_id { get; set; }
        public string st_serial { get; set; }
        public string end_serial { get; set; }
    }
}