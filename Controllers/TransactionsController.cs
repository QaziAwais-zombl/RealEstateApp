using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Data;
using RealEstateApp.Models;
using System.Security.Claims;

namespace RealEstateApp.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> SendBuyRequest(int id)
        {
            var property = await _context.Properties
                .Include(p => p.Owner)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null || property.Status != PropertyStatus.Available || property.PropertyType != PropertyType.ForSale)
            {
                TempData["ErrorMessage"] = "This property is not available for purchase.";
                return RedirectToAction("Index", "Properties");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (property.OwnerId == userId)
            {
                TempData["ErrorMessage"] = "You cannot buy your own property.";
                return RedirectToAction("Details", "Properties", new { id });
            }

            var existingRequest = await _context.PropertyRequests
                .FirstOrDefaultAsync(pr => pr.PropertyId == id && pr.InterestedUserId == userId && pr.Status == RequestStatus.Pending);

            if (existingRequest != null)
            {
                TempData["ErrorMessage"] = "You already have a pending request for this property.";
                return RedirectToAction("Details", "Properties", new { id });
            }

            var request = new PropertyRequest
            {
                PropertyId = id,
                SellerId = property.OwnerId ?? "",
                RequestType = RequestType.Buy
            };

            ViewBag.Property = property;
            return View("SendRequest", request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendBuyRequest(int id, [Bind("BuyerRenterName,BuyerRenterEmail,BuyerRenterPhone,BuyerRenterAddress,Message")] PropertyRequest request)
        {
            var property = await _context.Properties.FindAsync(id);
            
            if (property == null || property.Status != PropertyStatus.Available || property.PropertyType != PropertyType.ForSale)
            {
                TempData["ErrorMessage"] = "This property is not available for purchase.";
                return RedirectToAction("Index", "Properties");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (property.OwnerId == userId)
            {
                TempData["ErrorMessage"] = "You cannot buy your own property.";
                return RedirectToAction("Details", "Properties", new { id });
            }

            request.PropertyId = id;
            request.InterestedUserId = userId;
            request.SellerId = property.OwnerId ?? "";
            request.RequestType = RequestType.Buy;
            request.Status = RequestStatus.Pending;
            request.CreatedAt = DateTime.Now;

            _context.PropertyRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your purchase request has been sent to the seller!";
            return RedirectToAction("MyRequests");
        }

        [HttpGet]
        public async Task<IActionResult> SendRentRequest(int id)
        {
            var property = await _context.Properties
                .Include(p => p.Owner)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null || property.Status != PropertyStatus.Available || property.PropertyType != PropertyType.ForRent)
            {
                TempData["ErrorMessage"] = "This property is not available for rent.";
                return RedirectToAction("Index", "Properties");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (property.OwnerId == userId)
            {
                TempData["ErrorMessage"] = "You cannot rent your own property.";
                return RedirectToAction("Details", "Properties", new { id });
            }

            var existingRequest = await _context.PropertyRequests
                .FirstOrDefaultAsync(pr => pr.PropertyId == id && pr.InterestedUserId == userId && pr.Status == RequestStatus.Pending);

            if (existingRequest != null)
            {
                TempData["ErrorMessage"] = "You already have a pending request for this property.";
                return RedirectToAction("Details", "Properties", new { id });
            }

            var request = new PropertyRequest
            {
                PropertyId = id,
                SellerId = property.OwnerId ?? "",
                RequestType = RequestType.Rent
            };

            ViewBag.Property = property;
            return View("SendRequest", request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendRentRequest(int id, [Bind("BuyerRenterName,BuyerRenterEmail,BuyerRenterPhone,BuyerRenterAddress,Message")] PropertyRequest request)
        {
            var property = await _context.Properties.FindAsync(id);
            
            if (property == null || property.Status != PropertyStatus.Available || property.PropertyType != PropertyType.ForRent)
            {
                TempData["ErrorMessage"] = "This property is not available for rent.";
                return RedirectToAction("Index", "Properties");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (property.OwnerId == userId)
            {
                TempData["ErrorMessage"] = "You cannot rent your own property.";
                return RedirectToAction("Details", "Properties", new { id });
            }

            request.PropertyId = id;
            request.InterestedUserId = userId;
            request.SellerId = property.OwnerId ?? "";
            request.RequestType = RequestType.Rent;
            request.Status = RequestStatus.Pending;
            request.CreatedAt = DateTime.Now;

            _context.PropertyRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your rental request has been sent to the seller!";
            return RedirectToAction("MyRequests");
        }

        public async Task<IActionResult> MyRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var requests = await _context.PropertyRequests
                .Include(pr => pr.Property)
                    .ThenInclude(p => p.Category)
                .Include(pr => pr.Seller)
                .Where(pr => pr.InterestedUserId == userId)
                .OrderByDescending(pr => pr.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        public async Task<IActionResult> IncomingRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var requests = await _context.PropertyRequests
                .Include(pr => pr.Property)
                    .ThenInclude(p => p.Category)
                .Include(pr => pr.InterestedUser)
                .Where(pr => pr.SellerId == userId && pr.Status == RequestStatus.Pending)
                .OrderByDescending(pr => pr.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptRequest(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var request = await _context.PropertyRequests
                .Include(pr => pr.Property)
                .FirstOrDefaultAsync(pr => pr.Id == id && pr.SellerId == userId);

            if (request == null)
            {
                TempData["ErrorMessage"] = "Request not found.";
                return RedirectToAction(nameof(IncomingRequests));
            }

            if (request.Status != RequestStatus.Pending)
            {
                TempData["ErrorMessage"] = "This request has already been processed.";
                return RedirectToAction(nameof(IncomingRequests));
            }

            request.Status = RequestStatus.Accepted;
            request.RespondedAt = DateTime.Now;

            if (request.Property != null)
            {
                request.Property.Status = request.RequestType == RequestType.Buy ? PropertyStatus.Sold : PropertyStatus.Rented;

                var transaction = new Transaction
                {
                    PropertyId = request.PropertyId,
                    BuyerRenterId = request.InterestedUserId,
                    TransactionDate = DateTime.Now,
                    TransactionType = request.RequestType == RequestType.Buy ? PropertyStatus.Sold : PropertyStatus.Rented,
                    TransactionAmount = request.Property.Price,
                    BuyerRenterName = request.BuyerRenterName,
                    BuyerRenterEmail = request.BuyerRenterEmail,
                    BuyerRenterPhone = request.BuyerRenterPhone,
                    BuyerRenterAddress = request.BuyerRenterAddress
                };

                _context.Transactions.Add(transaction);

                var otherPendingRequests = await _context.PropertyRequests
                    .Where(pr => pr.PropertyId == request.PropertyId && pr.Id != id && pr.Status == RequestStatus.Pending)
                    .ToListAsync();

                foreach (var pr in otherPendingRequests)
                {
                    pr.Status = RequestStatus.Rejected;
                    pr.RespondedAt = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Request accepted successfully!";
            return RedirectToAction(nameof(IncomingRequests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRequest(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var request = await _context.PropertyRequests
                .FirstOrDefaultAsync(pr => pr.Id == id && pr.SellerId == userId);

            if (request == null)
            {
                TempData["ErrorMessage"] = "Request not found.";
                return RedirectToAction(nameof(IncomingRequests));
            }

            if (request.Status != RequestStatus.Pending)
            {
                TempData["ErrorMessage"] = "This request has already been processed.";
                return RedirectToAction(nameof(IncomingRequests));
            }

            request.Status = RequestStatus.Rejected;
            request.RespondedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Request rejected.";
            return RedirectToAction(nameof(IncomingRequests));
        }

        public async Task<IActionResult> MyPurchases()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var purchases = await _context.Transactions
                .Include(t => t.Property)
                    .ThenInclude(p => p.Category)
                .Include(t => t.Property.Owner)
                .Where(t => t.BuyerRenterId == userId && t.TransactionType == PropertyStatus.Sold)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            return View(purchases);
        }

        public async Task<IActionResult> MyRentals()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rentals = await _context.Transactions
                .Include(t => t.Property)
                    .ThenInclude(p => p.Category)
                .Include(t => t.Property.Owner)
                .Where(t => t.BuyerRenterId == userId && t.TransactionType == PropertyStatus.Rented)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            return View(rentals);
        }

        public async Task<IActionResult> SoldProperties()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var soldProperties = await _context.Transactions
                .Include(t => t.Property)
                    .ThenInclude(p => p.Category)
                .Include(t => t.BuyerRenter)
                .Where(t => t.Property.OwnerId == userId && t.TransactionType == PropertyStatus.Sold)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            return View(soldProperties);
        }

        public async Task<IActionResult> RentedProperties()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rentedProperties = await _context.Transactions
                .Include(t => t.Property)
                    .ThenInclude(p => p.Category)
                .Include(t => t.BuyerRenter)
                .Where(t => t.Property.OwnerId == userId && t.TransactionType == PropertyStatus.Rented)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            return View(rentedProperties);
        }
    }
}
