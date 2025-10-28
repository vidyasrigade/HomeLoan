using System;
using System.ComponentModel.DataAnnotations;

namespace HomeLoan.Models
{
    public class LoanApplication
    {
        public int Id { get; set; }

        // link to user who created this application (optional)
        public int? UserId { get; set; }

        public AppUser? User { get; set; }

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime DOB { get; set; }

        [MaxLength(12)]
        public string AadharNumber { get; set; } = string.Empty;

        [MaxLength(10)]
        public string PanNumber { get; set; } = string.Empty;

        public string PropertyName { get; set; } = string.Empty;

        public decimal EstimatedPropertyCost { get; set; }

        public decimal LoanAmount { get; set; }

        public int TenureMonths { get; set; }

        public decimal InterestRate { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime AppliedOn { get; set; } = DateTime.UtcNow;

        // Account creation after approval
        public string AccountNumber { get; set; } = string.Empty;

        public DateTime? AccountCreatedOn { get; set; }

        public decimal? AccountBalance { get; set; }
    }
}
