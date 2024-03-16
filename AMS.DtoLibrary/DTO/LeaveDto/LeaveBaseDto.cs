namespace AMS.DtoLibrary.DTO.LeaveDto
{
    public class LeaveBaseDto
    {
        public int TotalLeaves { get; set; }
        public DateTime LeaveStartDate { get; set; }
        public DateTime LeaveEndDate { get; set; }
        public bool StartHalfDay { get; set; }
        public double NumberOfDaysLeave { get; set; }
        public double TotalLeavesTaken { get; set; }
        public double TotalLeavesLeft { get; set; }
        public bool IsApproved { get; set; }
        public int UserId { get; set; }
    }
}
