using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EFCoreConfiguration.Models
{
    [Table("TBL_Country")]
    public class Country
    {
        [Key]
        public int KeyId { get; set; }

        [MaxLength(20)]
        public string Name { get; set; }
    }
}
