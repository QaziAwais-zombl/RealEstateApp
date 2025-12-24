using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateApp.Models
{
    public class PropertyRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [ForeignKey("PropertyId")]
        public virtual Property? Property { get; set; }

        [Required]
        public string InterestedUserId { get; set; } = string.Empty;

        [ForeignKey("InterestedUserId")]
        public virtual IdentityUser? InterestedUser { get; set; }

        [Required]
        public string SellerId { get; set; } = string.Empty;

        [ForeignKey("SellerId")]
        public virtual IdentityUser? Seller { get; set; }

        [Required]
        public RequestType RequestType { get; set; }

        [Required]
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? RespondedAt { get; set; }

        [StringLength(500)]
        public string? BuyerRenterName { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? BuyerRenterEmail { get; set; }

        [StringLength(20)]
        [Phone]
        public string? BuyerRenterPhone { get; set; }

        [StringLength(300)]
        public string? BuyerRenterAddress { get; set; }

        [StringLength(1000)]
        public string? Message { get; set; }
    }
}
