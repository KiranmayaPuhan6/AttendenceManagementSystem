namespace AMS.DtoLibrary.DTO.LeaveDto
{
    public class LeaveCreationDto
    {
        public DateTime LeaveStartDate { get; set; }
        public DateTime LeaveEndDate { get; set; }
        public bool StartHalfDay { get; set; }
        public int UserId { get; set; }
    }
}
