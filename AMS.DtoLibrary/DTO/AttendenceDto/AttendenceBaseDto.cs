using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMS.DtoLibrary.DTO.AttendenceDto
{
    public class AttendenceBaseDto
    {
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public double TotalLoggedInTime { get; set; }
        public int UserId { get; set; }
    }
}
