using HomeLoan.Data;
using HomeLoan.Models;
using HomeLoan.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

public class LoanController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly PasswordHasher<AppUser> _pwdHasher;

    public LoanController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
        _pwdHasher = new PasswordHasher<AppUser>();
    }

    [HttpGet]
    public IActionResult Apply()
    {
        if (HttpContext.Session.GetInt32("UserId") == null)
            return RedirectToAction("Login", "Account");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(LoanApplicationViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        // create loan entity
        var loan = new LoanApplication
        {
            FirstName = vm.FirstName,
            LastName = vm.LastName,
            DOB = vm.DOB!.Value,
            AadharNumber = vm.AadharNumber,
            PanNumber = vm.PanNumber,
            PropertyName = vm.PropertyName,
            EstimatedPropertyCost = vm.EstimatedPropertyCost!.Value,
            LoanAmount = vm.LoanAmount!.Value,
            TenureMonths = vm.TenureYears!.Value * 12,
            InterestRate = vm.InterestRate!.Value,
            Status = "Sent for verification",
            AccountNumber = "123456789"
        };

        // If user logged in, attach
        var sessionUserId = HttpContext.Session.GetInt32("UserId");
        if (sessionUserId.HasValue)
            loan.UserId = sessionUserId.Value;
        else
        {
            // If user entered Email+Password fields in the application form, create an account
            var email = Request.Form["Email"].ToString();
            var password = Request.Form["Password"].ToString();
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                var existing = await _db.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
                if (existing == null)
                {
                    var newUser = new AppUser
                    {
                        Email = email,
                        FirstName = vm.FirstName,
                        LastName = vm.LastName,
                        Mobile = Request.Form["MobileNumber"].ToString()
                    };
                    newUser.PasswordHash = _pwdHasher.HashPassword(newUser, password);
                    _db.AppUsers.Add(newUser);
                    await _db.SaveChangesAsync();
                    loan.UserId = newUser.Id;
                    // auto-login
                    HttpContext.Session.SetInt32("UserId", newUser.Id);
                    HttpContext.Session.SetString("UserEmail", newUser.Email);
                }
                else
                {
                    // Attach existing user (optionally: verify password)
                    loan.UserId = existing.Id;
                }
            }
        }

        _db.LoanApplications.Add(loan);
        await _db.SaveChangesAsync();

        // Handle file uploads (Request.Form.Files)
        var files = Request.Form.Files;
        if (files != null && files.Count > 0)
        {
            var uploadRoot = Path.Combine(_env.WebRootPath, "uploads", loan.Id.ToString());
            Directory.CreateDirectory(uploadRoot);
            foreach (var f in files)
            {
                if (f.Length <= 0) continue;
                var safeName = Path.GetFileName(f.FileName);
                var savePath = Path.Combine(uploadRoot, safeName);
                using (var fs = new FileStream(savePath, FileMode.Create))
                {
                    await f.CopyToAsync(fs);
                }
                var doc = new Document
                {
                    LoanApplicationId = loan.Id,
                    FileName = safeName,
                    FilePath = $"/uploads/{loan.Id}/{safeName}",
                    DocumentType = f.Name // input name like "AadharFile" used to identify
                };
                _db.Documents.Add(doc);
            }
            await _db.SaveChangesAsync();
        }

        return RedirectToAction("Confirmation", new { id = loan.Id });
    }

    public async Task<IActionResult> Confirmation(int id)
    {
        var loan = await _db.LoanApplications.FirstOrDefaultAsync(l => l.Id == id);
        if (loan == null) return NotFound();
        return View(loan);
    }
}