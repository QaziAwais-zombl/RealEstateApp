using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Data;
using RealEstateApp.Models;

namespace RealEstateApp.Controllers
{
    [Authorize]
    public class PropertiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PropertiesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Properties
        public async Task<IActionResult> Index()
        {
            var properties = await _context.Properties
                .Include(p => p.Category)
                .ToListAsync();
            return View(properties);
        }

        // GET: Properties/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.Properties
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (property == null)
            {
                return NotFound();
            }

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
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Price,Address,CategoryId,ImageFile")] Property property)
        {
            if (ModelState.IsValid)
            {
                // Handle image file upload
                if (property.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");

                    // Create images folder if it doesn't exist
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate unique filename
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + property.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save file to wwwroot/images
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await property.ImageFile.CopyToAsync(fileStream);
                    }

                    // Save relative path to database
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
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.Properties.FindAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", property.CategoryId);
            return View(property);
        }

        // POST: Properties/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Price,Address,CategoryId,ImageUrl,ImageFile")] Property property)
        {
            if (id != property.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle new image file upload
                    if (property.ImageFile != null)
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(property.ImageUrl))
                        {
                            string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, property.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Upload new image
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");

                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + property.ImageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await property.ImageFile.CopyToAsync(fileStream);
                        }

                        property.ImageUrl = "/images/" + uniqueFileName;
                    }

                    _context.Update(property);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Property updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PropertyExists(property.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", property.CategoryId);
            return View(property);
        }

        // GET: Properties/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.Properties
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        // POST: Properties/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var property = await _context.Properties.FindAsync(id);

            if (property != null)
            {
                // Delete associated image file
                if (!string.IsNullOrEmpty(property.ImageUrl))
                {
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, property.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Properties.Remove(property);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Property deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PropertyExists(int id)
        {
            return _context.Properties.Any(e => e.Id == id);
        }
    }
}