using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        // Navigation property
        public ICollection<Property>? Properties { get; set; }
    }
}