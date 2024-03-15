namespace AMS.DtoLibrary.DTO.UserDto
{
    public class UserDto : UserBaseDto
    {
        public int UserId { get; set; }
        public virtual IEnumerable<AMS.DtoLibrary.DTO.AttendenceDto.AttendenceDto> Attendence { get; set; }
        public virtual IEnumerable<AMS.DtoLibrary.DTO.LeaveDto.LeaveDto> Leave { get; set; }
    }
}
