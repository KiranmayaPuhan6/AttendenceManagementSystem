using System.ComponentModel.DataAnnotations.Schema;
using UserMicroservices.Models.Domain.Entities;

namespace UserMicroservices.Models.DTO
{
    public class UserBaseDto
    {
        public int UserID { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
        public string Designation { get; set; }
        public string ManagerName { get; set; }
        public string ManagerEmail { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public IFormFile? FileUri { get; set; }
    }
}
