using HomeLoan.Models;

namespace HomeLoan.Services
{
    public interface ILoanService
    {
        Task<LoanApplication> CreateAsync(LoanApplication application);
        Task<LoanApplication?> GetAsync(int id);
    }
}
