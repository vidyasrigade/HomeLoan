using Microsoft.AspNetCore.Mvc;
using HomeLoan.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace HomeLoan.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        public AdminController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Login(string username, string password)
        {
            if (username == "admin@hotmail.com" && password == "Test@123")
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                return RedirectToAction("Dashboard", "admin");
            }
            ViewBag.Error = "Invalid Username or Password";
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have logged out.";
            return RedirectToAction("Index", "Home");
        }
        //-----DashBoard-------
        public async Task<IActionResult> Dashboard()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");

            var apps = await _db.LoanApplications
                                .OrderByDescending(x => x.AppliedOn)
                                .ToListAsync();

            // Populate view counts and aggregates so the view shows correct values
            ViewBag.TotalApplications = apps.Count;
            ViewBag.UsersCount = await _db.AppUsers.CountAsync();
            ViewBag.MessagesCount = await _db.ContactMessages.CountAsync();
            ViewBag.Revenue = await _db.LoanApplications.SumAsync(a => (decimal?)a.LoanAmount) ?? 0m;

            return View(apps);
        }

        // Lightweight counts endpoint for dashboard polling (JSON)
        [HttpGet]
        public async Task<IActionResult> Counts()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            var totalApplications = await _db.LoanApplications.CountAsync();
            var usersCount = await _db.AppUsers.CountAsync();
            var messagesCount = await _db.ContactMessages.CountAsync();
            var revenue = await _db.LoanApplications.SumAsync(a => (decimal?)a.LoanAmount) ?? 0m;

            var approved = await _db.LoanApplications.CountAsync(a => a.Status == "Approved");
            var rejected = await _db.LoanApplications.CountAsync(a => a.Status == "Rejected");
            var pending = await _db.LoanApplications.CountAsync(a => !(a.Status == "Approved" || a.Status == "Rejected"));

            return Json(new
            {
                totalApplications,
                usersCount,
                messagesCount,
                revenue,
                approved,
                rejected,
                pending
            });
        }

        // Export applications as CSV and trigger file download
        [HttpGet]
        public async Task<IActionResult> ExportCsv()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");

            var apps = await _db.LoanApplications
                                .OrderByDescending(x => x.AppliedOn)
                                .ToListAsync();

            var sb = new StringBuilder();
            // header
            sb.AppendLine("Id,FirstName,LastName,PropertyName,LoanAmount,Status,AppliedOn");

            string Escape(string? s)
            {
                if (string.IsNullOrEmpty(s)) return "";
                // wrap in quotes and escape existing quotes
                return "\"" + s.Replace("\"", "\"\"") + "\"";
            }

            foreach (var a in apps)
            {
                // Guard against uninitialized / MinValue dates (Excel shows "######" for invalid/negative dates).
                string applied;
                if (a.AppliedOn == default || a.AppliedOn.Year < 1900)
                {
                    applied = ""; // leave blank if date is invalid for Excel
                }
                else
                {
                    applied = a.AppliedOn.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                }

                var row = new[]
                {
                    a.Id.ToString(),
                    Escape(a.FirstName),
                    Escape(a.LastName),
                    Escape(a.PropertyName),
                    a.LoanAmount.ToString("F2"),
                    Escape(a.Status),
                    Escape(applied),
                };
                sb.AppendLine(string.Join(",", row));
            }

            // Add UTF-8 BOM so Excel/Windows recognizes UTF-8 reliably
            var preamble = Encoding.UTF8.GetPreamble();
            var payload = Encoding.UTF8.GetBytes(sb.ToString());
            var bytes = new byte[preamble.Length + payload.Length];
            Buffer.BlockCopy(preamble, 0, bytes, 0, preamble.Length);
            Buffer.BlockCopy(payload, 0, bytes, preamble.Length, payload.Length);

            var filename = $"applications_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv; charset=utf-8", filename);
        }

        // New: Admin can view contact messages
        public async Task<IActionResult> Messages()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");

            var msgs = await _db.ContactMessages
                                .OrderByDescending(m => m.CreatedAt)
                                .ToListAsync();
            return View(msgs);
        }
        // List applications filtered by status
        [HttpGet]
        public async Task<IActionResult> Applications(string filter = "all")
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");

            var q = _db.LoanApplications.AsQueryable();

            ViewBag.Filter = filter ?? "all";
            switch ((filter ?? "all").ToLowerInvariant())
            {
                case "pending":
                    q = q.Where(x => x.Status.Contains("verification") || x.Status == "Sent for verification" || x.Status == "Verified and sent for final approval");
                    ViewBag.Title = "Pending Applications";
                    break;
                case "approved":
                    q = q.Where(x => x.Status == "Approved");
                    ViewBag.Title = "Approved Applications";
                    break;
                case "rejected":
                    q = q.Where(x => x.Status == "Rejected");
                    ViewBag.Title = "Rejected Applications";
                    break;
                default:
                    ViewBag.Title = "All Applications";
                    break;
            }

            var list = await q.OrderByDescending(x => x.AppliedOn).ToListAsync();
            return View(list);
        }

        // Show single application details
        [HttpGet]
        public async Task<IActionResult> ApplicationDetails(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");

            var app = await _db.LoanApplications.FindAsync(id);
            if (app == null) return NotFound();

            return View(app);
        }
        // Users list (registrations)
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");

            var users = await _db.AppUsers.OrderByDescending(u => u.CreatedAt).ToListAsync();
            return View(users);
        }

        // Single user details
        [HttpGet]
        public async Task<IActionResult> UserDetails(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");

            var user = await _db.AppUsers.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var loan = await _db.LoanApplications.FindAsync(id);
            if (loan != null)
            {
                loan.Status = status;

                // When approved -> create account if not present (per doc)
                if (status == "Approved" && string.IsNullOrEmpty(loan.AccountNumber))
                {
                    loan.AccountNumber = GenerateAccountNumber();
                    loan.AccountCreatedOn = DateTime.UtcNow;
                    loan.AccountBalance = (decimal?)loan.LoanAmount; // mock disbursal
                }

                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> ViewDocuments(int loanId)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");

            var docs = await _db.Documents.Where(d => d.LoanApplicationId == loanId).ToListAsync();
            ViewBag.LoanId = loanId;
            return View(docs);
        }

        // helper
        private string GenerateAccountNumber()
        {
            // Example account number generator: HL + yyyyMMdd + 6 random digits
            var rnd = new Random();
            var suffix = rnd.Next(100000, 999999).ToString();
            return "HL" + DateTime.UtcNow.ToString("yyyyMMdd") + suffix;
        }

    }
}