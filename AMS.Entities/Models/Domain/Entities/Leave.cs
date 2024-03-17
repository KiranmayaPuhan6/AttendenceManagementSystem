using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMS.Entities.Models.Domain.Entities
{
    [Table("Leave")]
    public class Leave
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int TotalLeaves { get; set; } = 15;
        public DateTime LeaveStartDate { get; set; }
        public DateTime LeaveEndDate { get; set; }
        public bool StartHalfDay { get; set; }
        public double NumberOfDaysLeave { get; set; }
        public double TotalLeavesTaken { get; set; }
        public double TotalLeavesLeft { get; set; }
        public bool IsApproved { get; set; }
        public string LeaveType { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
