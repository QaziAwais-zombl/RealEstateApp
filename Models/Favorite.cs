using Microsoft.AspNetCore.Identity;

namespace RealEstateApp.Models
{
    public class Favorite
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public virtual IdentityUser? User { get; set; }
        public int PropertyId { get; set; }
        public virtual Property? Property { get; set; }
    }
}