using HomeLoan.Data;
using HomeLoan.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeLoan.Services
{
    public class LoanService : ILoanService
    {
        private readonly AppDbContext _db;
        public LoanService(AppDbContext db) => _db = db;

        public async Task<LoanApplication> CreateAsync(LoanApplication application)
        {
            _db.LoanApplications.Add(application);
            await _db.SaveChangesAsync();
            return application;
        }

        public async Task<LoanApplication?> GetAsync(int id)
        {
            return await _db.LoanApplications.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}

