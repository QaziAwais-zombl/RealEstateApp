using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateApp.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [ForeignKey("PropertyId")]
        public virtual Property? Property { get; set; }

        [Required]
        public string BuyerRenterId { get; set; } = string.Empty;

        [ForeignKey("BuyerRenterId")]
        public virtual IdentityUser? BuyerRenter { get; set; }

        [Required]
        [Display(Name = "Transaction Date")]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Transaction Type")]
        public PropertyStatus TransactionType { get; set; }

        [Required]
        [Display(Name = "Transaction Amount")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TransactionAmount { get; set; }

        [StringLength(500)]
        [Display(Name = "Buyer/Renter Name")]
        public string? BuyerRenterName { get; set; }

        [StringLength(100)]
        [Display(Name = "Buyer/Renter Email")]
        [EmailAddress]
        public string? BuyerRenterEmail { get; set; }

        [StringLength(20)]
        [Display(Name = "Buyer/Renter Phone")]
        [Phone]
        public string? BuyerRenterPhone { get; set; }

        [StringLength(300)]
        [Display(Name = "Buyer/Renter Address")]
        public string? BuyerRenterAddress { get; set; }
    }
}
