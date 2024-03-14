using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMS.Entities.Models.Domain.Entities
{
    [Table("Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
        public string Password { get; set; }
        public string Role { get; set; }
        [NotMapped]
        public IFormFile? FileUri { get; set; }
        public string? ActualFileUrl { get; set; }
        public int? PasswordResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }
        public virtual IEnumerable<Attendence> Attendence { get; set; }
        //public virtual IList<Leave> Leaves { get; set; }
    }
}
