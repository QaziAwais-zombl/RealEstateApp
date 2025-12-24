using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Data;
using RealEstateApp.Models;
using System.Security.Claims; // Needed for User.FindFirstValue

namespace RealEstateApp.Controllers
{
    [Authorize] // Forces users to be logged in for most actions
    public class PropertiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PropertiesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Properties (Show All)
        [AllowAnonymous] // Anyone can view the list
        public async Task<IActionResult> Index()
        {
            var properties = await _context.Properties
                .Include(p => p.Category)
                .Include(p => p.Owner) // Shows who listed the property
                .ToListAsync();
            return View(properties);
        }

        // GET: Properties/MyProperties (NEW: Show only logged-in user's properties)
        public async Task<IActionResult> MyProperties()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var myProperties = await _context.Properties
                .Where(p => p.OwnerId == userId)
                .Include(p => p.Category)
                .ToListAsync();
            return View("Index", myProperties); // Reuse Index view but with filtered data
        }

        // GET: Properties/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var property = await _context.Properties
                .Include(p => p.Category)
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (property == null) return NotFound();

            return View(property);
        }

        // GET: Properties/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Properties/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Price,Address,CategoryId,PropertyType,ImageFile")] Property property)
        {
            // 1. Assign the logged-in user as the Owner
            property.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            property.Status = PropertyStatus.Available;

            if (ModelState.IsValid)
            {
                // 2. Handle Image Upload
                if (property.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + property.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await property.ImageFile.CopyToAsync(fileStream);
                    }
                    property.ImageUrl = "/images/" + uniqueFileName;
                }

                _context.Add(property);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Property created successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", property.CategoryId);
            return View(property);
        }

        // GET: Properties/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var property = await _context.Properties.FindAsync(id);
            if (property == null) return NotFound();

            // 3. SECURITY CHECK: Only Owner can edit
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (property.OwnerId != userId)
            {
                return Forbid(); // Return 403 Forbidden
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", property.CategoryId);
            return View(property);
        }

        // POST: Properties/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Price,Address,CategoryId,PropertyType,Status,ImageUrl,ImageFile,OwnerId")] Property property)
        {
            if (id != property.Id) return NotFound();

            // 4. SECURITY CHECK AGAIN (Prevent Form Tampering)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var originalProperty = await _context.Properties.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

            if (originalProperty == null || originalProperty.OwnerId != userId)
            {
                return Forbid();
            }

            // Ensure OwnerId is preserved
            property.OwnerId = userId;

            if (ModelState.IsValid)
            {
                try
                {
                    // 5. Handle Image Update
                    if (property.ImageFile != null)
                    {
                        // Delete old image
                        if (!string.IsNullOrEmpty(property.ImageUrl))
                        {
                            string oldPath = Path.Combine(_webHostEnvironment.WebRootPath, property.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                        }

                        // Save new image
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + property.ImageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await property.ImageFile.CopyToAsync(fileStream);
                        }
                        property.ImageUrl = "/images/" + uniqueFileName;
                    }
                    else
                    {
                        // Keep old image URL if no new file uploaded
                        property.ImageUrl = originalProperty.ImageUrl;
                    }

                    _context.Update(property);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Property updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PropertyExists(property.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", property.CategoryId);
            return View(property);
        }

        // GET: Properties/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var property = await _context.Properties
                .Include(p => p.Category)
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (property == null) return NotFound();

            // 6. Security Check: Hide delete page if not owner
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (property.OwnerId != userId)
            {
                return Forbid();
            }

            return View(property);
        }

        // POST: Properties/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 7. FINAL SECURITY CHECK
            if (property != null && property.OwnerId == userId)
            {
                // Delete image file
                if (!string.IsNullOrEmpty(property.ImageUrl))
                {
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, property.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath)) System.IO.File.Delete(imagePath);
                }

                _context.Properties.Remove(property);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Property deleted successfully!";
            }
            else if (property != null && property.OwnerId != userId)
            {
                return Forbid();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PropertyExists(int id)
        {
            return _context.Properties.Any(e => e.Id == id);
        }
    }
}