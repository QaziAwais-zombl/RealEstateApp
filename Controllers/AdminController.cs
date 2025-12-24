using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Data;

namespace RealEstateApp.Controllers
{
    [Authorize(Roles = "Admin")] // Critical Security Line
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(UserManager<IdentityUser> userManager, ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // List all users and properties
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var properties = await _context.Properties
                .Include(p => p.Owner)
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            ViewBag.Properties = properties;
            return View(users);
        }

        // Create User (Simplified)
        [HttpPost]
        public async Task<IActionResult> CreateUser(string email, string password)
        {
            var user = new IdentityUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // Default new users to "User" role
                await _userManager.AddToRoleAsync(user, "User");
                TempData["SuccessMessage"] = "User created successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to create user: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }
            return RedirectToAction(nameof(Index));
        }

        // Delete User
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Delete all properties owned by this user
                var userProperties = await _context.Properties.Where(p => p.OwnerId == id).ToListAsync();
                
                foreach (var property in userProperties)
                {
                    // Delete property image
                    if (!string.IsNullOrEmpty(property.ImageUrl))
                    {
                        string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, property.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    // Delete related favorites
                    var relatedFavorites = await _context.Favorites.Where(f => f.PropertyId == property.Id).ToListAsync();
                    _context.Favorites.RemoveRange(relatedFavorites);

                    // Delete related property requests
                    var relatedRequests = await _context.PropertyRequests.Where(pr => pr.PropertyId == property.Id).ToListAsync();
                    _context.PropertyRequests.RemoveRange(relatedRequests);

                    // Delete related transactions
                    var relatedTransactions = await _context.Transactions.Where(t => t.PropertyId == property.Id).ToListAsync();
                    _context.Transactions.RemoveRange(relatedTransactions);
                }

                // Remove the properties
                _context.Properties.RemoveRange(userProperties);

                // Delete favorites where this user is the favoriter
                var userFavorites = await _context.Favorites.Where(f => f.UserId == id).ToListAsync();
                _context.Favorites.RemoveRange(userFavorites);

                // Delete property requests where this user is interested or seller
                var userRequestsAsInterested = await _context.PropertyRequests.Where(pr => pr.InterestedUserId == id).ToListAsync();
                var userRequestsAsSeller = await _context.PropertyRequests.Where(pr => pr.SellerId == id).ToListAsync();
                _context.PropertyRequests.RemoveRange(userRequestsAsInterested);
                _context.PropertyRequests.RemoveRange(userRequestsAsSeller);

                // Delete transactions where this user is buyer/renter
                var userTransactions = await _context.Transactions.Where(t => t.BuyerRenterId == id).ToListAsync();
                _context.Transactions.RemoveRange(userTransactions);

                // Save all the deletions
                await _context.SaveChangesAsync();

                // Finally delete the user
                var result = await _userManager.DeleteAsync(user);
                
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "User and all associated data deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete user: " + string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting user: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // Delete Property
        [HttpPost]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            
            if (property != null)
            {
                try
                {
                    // Delete property image
                    if (!string.IsNullOrEmpty(property.ImageUrl))
                    {
                        string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, property.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    // Delete related favorites
                    var relatedFavorites = await _context.Favorites.Where(f => f.PropertyId == id).ToListAsync();
                    _context.Favorites.RemoveRange(relatedFavorites);

                    // Delete related property requests
                    var relatedRequests = await _context.PropertyRequests.Where(pr => pr.PropertyId == id).ToListAsync();
                    _context.PropertyRequests.RemoveRange(relatedRequests);

                    // Delete related transactions
                    var relatedTransactions = await _context.Transactions.Where(t => t.PropertyId == id).ToListAsync();
                    _context.Transactions.RemoveRange(relatedTransactions);

                    // Delete the property
                    _context.Properties.Remove(property);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Property deleted successfully!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error deleting property: " + ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Property not found.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}