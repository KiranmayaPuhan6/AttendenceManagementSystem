namespace AMS.DtoLibrary.DTO.AttendenceDto
{
    public class AttendenceBaseDto
    {
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public double TotalLoggedInTime { get; set; }
        public string AttendenceType { get; set; }
        public int UserId { get; set; }
    }
}
