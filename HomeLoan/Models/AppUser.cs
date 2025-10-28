using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HomeLoan.Models
{
    public class AppUser
    {
        public int Id { get; set; }

        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, StringLength(10, MinimumLength = 10, ErrorMessage = "Mobile Number must be exactly 10 digits")]
        [RegularExpression(@"^\d{10}$")]
        public string Mobile { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
    }
}
