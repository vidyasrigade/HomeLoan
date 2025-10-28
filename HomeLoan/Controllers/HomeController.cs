using Microsoft.AspNetCore.Mvc;
using HomeLoan.Data;
using Microsoft.EntityFrameworkCore;
using HomeLoan.Models;

namespace HomeLoan.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        public HomeController(AppDbContext db) => _db = db;

        public IActionResult Index() => View();

        public IActionResult Calculator() => View();

        public IActionResult FAQ() => View();

        public IActionResult AboutUs() => View();

        [HttpGet]
        public IActionResult Contact() => View();

        // POST: store contact message and show confirmation on redirect
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(string name, string email, string? address, string message)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
            {
                TempData["Msg"] = "Please fill all required fields.";
                return RedirectToAction("Contact");
            }

            var contact = new ContactMessage
            {
                Name = name.Trim(),
                Email = email.Trim(),
                Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim(),
                Message = message.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _db.ContactMessages.Add(contact);
            await _db.SaveChangesAsync();

            TempData["Msg"] = "Thank you for contacting us, " + name + "! Your message has been sent.";
            return RedirectToAction("Contact");
        }
    }
}
