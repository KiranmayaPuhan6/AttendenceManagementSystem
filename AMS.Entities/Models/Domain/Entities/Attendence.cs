using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMS.Entities.Models.Domain.Entities
{
    [Table("Attendence")]
    public class Attendence
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public double TotalLoggedInTime { get; set; }
        public string AttendenceType { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
