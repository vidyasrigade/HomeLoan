using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HomeLoan.ViewModels
{
    public class LoanApplicationViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(50), Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required, StringLength(50), Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime? DOB { get; set; }

        [Required(ErrorMessage = "Aadhaar number is required")]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "Aadhaar must be 12 digits.")]
        [Display(Name = "Aadhaar Number")]
        public string AadharNumber { get; set; }

        [Required(ErrorMessage = "PAN is required")]
        [MaxLength(10, ErrorMessage = "PAN Number must be at most 10 characters.")]
        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]$", ErrorMessage = "PAN format is invalid.")]
        public string PanNumber { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile must be 10 digits.")]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; }

        [Required(ErrorMessage = "Property name or location is required")]
        [Display(Name = "Property Name / Location")]
        public string PropertyName { get; set; }

        [Required(ErrorMessage = "Estimated property cost is required")]
        [Range(1, 999999999, ErrorMessage = "Estimated cost must be greater than 0.")]
        public decimal? EstimatedPropertyCost { get; set; }

        [Display(Name = "Loan Amount")]
        [Required(ErrorMessage = "Loan amount is required")]
        [Range(1, 9999999999, ErrorMessage = "Loan amount must be greater than 0.")]
        public decimal? LoanAmount { get; set; }

        [Display(Name = "Tenure (Years)")]
        [Required(ErrorMessage = "Tenure is required")]
        [Range(1, 50, ErrorMessage = "Tenure must be between 1 and 50 years.")]
        public int? TenureYears { get; set; } = 5;

        [Display(Name = "Interest Rate (%)")]
        [Required(ErrorMessage = "Interest rate is required")]
        [Range(0.1, 100, ErrorMessage = "Interest rate must be a positive value.")]
        public decimal? InterestRate { get; set; } = 8.5M;

        // File uploads
        [Display(Name = "Aadhaar (scan)")]
        public IFormFile AadharFile { get; set; }

        [Display(Name = "PAN (scan)")]
        public IFormFile PanFile { get; set; }

        [Display(Name = "Salary Slip / Income Proof")]
        public IFormFile SalarySlip { get; set; }
    }
}
