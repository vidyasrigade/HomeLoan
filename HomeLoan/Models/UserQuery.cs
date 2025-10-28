using System.ComponentModel.DataAnnotations;

namespace HomeLoan.Models
{
    public class UserQuery
    {
        public int Id { get; set; }
      
        [Required]
        public string Name { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Message { get; set; }
        public DateTime SubmittedOn { get; set; } = DateTime.UtcNow;
    }
}