using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeLoan.Data;
using HomeLoan.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeLoan.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        public AccountController(AppDbContext db) => _db = db;

        // ---------- REGISTER ----------
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AppUser user)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(kvp => kvp.Value.Errors.Count > 0)
                    .SelectMany(kvp => kvp.Value.Errors.Select(e =>
                        string.IsNullOrWhiteSpace(kvp.Key) ? e.ErrorMessage : $"{kvp.Key}: {e.ErrorMessage}"
                    ))
                    .ToList();

                TempData["ErrorMessage"] = string.Join(" | ", errors);
                return View(user);
            }

            bool emailExists = await _db.AppUsers.AnyAsync(u => u.Email == user.Email);
            if (emailExists)
            {
                ModelState.AddModelError(nameof(user.Email), "This email is already registered. Please login.");
                TempData["ErrorMessage"] = "This email is already registered. Please login.";
                return View(user);
            }

            _db.AppUsers.Add(user);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registration successful! Please login to continue.";
            return RedirectToAction("Login");
        }

        // ---------- LOGIN ----------
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, int? applicationId, string applicationPassword)
        {
            var errors = new List<string>();

            // Application ID login path (if values provided)
            if (applicationId.HasValue || !string.IsNullOrWhiteSpace(applicationPassword))
            {
                // require both if either is provided
                if (!applicationId.HasValue) errors.Add("Application ID is required when using Application password.");
                if (string.IsNullOrWhiteSpace(applicationPassword)) errors.Add("Application password is required when using Application ID.");

                if (errors.Any())
                {
                    TempData["ErrorMessage"] = string.Join(" | ", errors);
                    return View();
                }

                // Placeholder behavior
                TempData["ErrorMessage"] = "Application ID login isn't implemented in this demo. Use Email login.";
                return View();
            }

            // Email login path
            if (string.IsNullOrWhiteSpace(email)) errors.Add("Email is required.");
            if (string.IsNullOrWhiteSpace(password)) errors.Add("Password is required.");

            if (errors.Any())
            {
                TempData["ErrorMessage"] = string.Join(" | ", errors);
                return View();
            }

            if (email == "admin@hotmail.com" && password == "Test@123")
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                HttpContext.Session.SetString("UserEmail", email);
                TempData["SuccessMessage"] = "Welcome back, Admin!";
                return RedirectToAction("Dashboard", "Admin");
            }

            var user = await _db.AppUsers
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == password);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Invalid email or password.";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", user.FirstName + " " + user.LastName);
            HttpContext.Session.SetString("IsAdmin", "false");

            TempData["SuccessMessage"] = $"Welcome back, {user.FirstName}!";
            return RedirectToAction("Dashboard", "Account");
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "No account found with that email.";
                return View();
            }

            TempData["SuccessMessage"] = "The Reset Instructions Have been sent to your mail.";
            return RedirectToAction("Login");
        }

        // ---------- DASHBOARD ----------
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please login first to access your dashboard.";
                return RedirectToAction("Login");
            }

            int uid = userId.Value;
            var user = await _db.AppUsers.FindAsync(uid);
            var loans = await _db.LoanApplications
                                 .Where(l => l.UserId == uid)
                                 .ToListAsync();

            ViewBag.User = user;
            return View(loans);
        }

        // ---------- LOGOUT ----------
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have successfully logged out.";
            return RedirectToAction("Index", "Home");
        }
    }
}
