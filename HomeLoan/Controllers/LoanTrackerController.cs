using HomeLoan.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeLoan.Controllers
{
    public class LoanTrackerController : Controller
    {
        private readonly AppDbContext _db;
        public LoanTrackerController(AppDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> Index(int id, DateTime dob)
        {
            var loan = await _db.LoanApplications.FirstOrDefaultAsync(x => x.Id == id && x.DOB == dob);
            if (loan == null)
            {
                ViewBag.Error = "No matching record found.";
                return View();
            }
            return View("Status", loan);
        }
    }
}
