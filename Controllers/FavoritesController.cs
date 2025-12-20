using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Data;
using RealEstateApp.Models;
using System.Security.Claims;

namespace RealEstateApp.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FavoritesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Toggle(int propertyId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existing = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.PropertyId == propertyId);

            if (existing != null)
            {
                _context.Favorites.Remove(existing); // Remove if exists
            }
            else
            {
                _context.Favorites.Add(new Favorite { UserId = userId, PropertyId = propertyId }); // Add if not
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Properties");
        }

        public async Task<IActionResult> MyFavorites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Property) // Load property data
                .Select(f => f.Property)
                .ToListAsync();

            return View(favorites);
        }
    }
}