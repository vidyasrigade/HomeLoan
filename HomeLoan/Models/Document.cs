using System;

namespace HomeLoan.Models
{
    public class Document
    {
        public int Id { get; set; }

        public int LoanApplicationId { get; set; }

        public LoanApplication? LoanApplication { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        // e.g. "Aadhar", "PAN", "SalarySlip"
        public string DocumentType { get; set; } = string.Empty;

        public DateTime UploadedOn { get; set; } = DateTime.UtcNow;
    }
}
