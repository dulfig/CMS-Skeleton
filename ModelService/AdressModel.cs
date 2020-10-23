using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModelService
{
    public class AdressModel
    {
        [Key]
        public int AdressId { get; set; }
        public string Line1 { get; set; }
        public string Lin2 { get; set; }
        public string Unit { get; set; }
        [Required]
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string PostCode { get; set; }
        public string Type { get; set; }
        [ForeignKey("UserId")]
        public string UserId { get; set; }
        public virtual AppUser User{ get; set;}


    }
}
