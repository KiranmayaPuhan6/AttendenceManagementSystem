using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMS.Entities.Models.Domain.Entities
{
    [Table("Holidays")]
    public class Holidays
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime Holiday {  get; set; }
    }
}
