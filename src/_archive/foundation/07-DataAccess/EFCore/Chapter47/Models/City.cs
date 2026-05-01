using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EFCoreConfiguration.Models
{
    [Table("TBL_City")]
    public class City
    {
        [Key]
        public int KeyId { get; set; }
        [Column("CityName", TypeName = "varchar(25)")]
        public string Name { get; set; }
        [NotMapped]
        public int Population { get; set; }
        [ForeignKey("FKid")]
        public Country Country { get; set; }
    }
}
