using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MVCPilotProjectWeb.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }


        [DisplayName("Category name")]
        [MaxLength(30)]
        public required string Name { get; set; }

        [DisplayName("Display order")]
        [Range(1,100)]
        public int DisplayOrder{ get; set; }
    }
}
