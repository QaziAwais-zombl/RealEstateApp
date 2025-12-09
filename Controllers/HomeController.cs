using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Data;
using RealEstateApp.Models;
using System.Diagnostics;

namespace RealEstateApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            // Start with all properties
            var properties = _context.Properties.Include(p => p.Category).AsQueryable();

            // Apply search filter if searchString is provided
            if (!string.IsNullOrEmpty(searchString))
            {
                properties = properties.Where(p =>
                    p.Title.Contains(searchString) ||
                    p.Address.Contains(searchString));

                ViewData["CurrentFilter"] = searchString;
            }

            var propertyList = await properties.ToListAsync();
            return View(propertyList);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}