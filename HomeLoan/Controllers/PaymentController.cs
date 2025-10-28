using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HomeLoan.Controllers
{
    public class PaymentController : Controller
    {
        // GET: /Payment/Pay?amount=1234.56&loanId=42
        [HttpGet]
        public IActionResult Pay(decimal? amount, int? loanId)
        {
            ViewData["Amount"] = amount.HasValue ? amount.Value.ToString("F2") : "";
            ViewData["LoanId"] = loanId?.ToString() ?? "";
            return View();
        }

        // POST: /Payment/Pay
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayPost(
            decimal amount,
            int? loanId,
            string paymentMethod,
            string? cardNumber,
            string? cardExpiry,
            string? cardCvv,
            string? bankName,
            string? accountNumber,
            string? accountHolder,
            string? upiId)
        {
            // Server-side validation matching the client rules
            if (amount <= 0)
            {
                ModelState.AddModelError("", "Amount must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                ModelState.AddModelError("paymentMethod", "Please select a payment method.");
            }
            else
            {
                switch (paymentMethod)
                {
                    case "card":
                        if (string.IsNullOrWhiteSpace(cardNumber)) ModelState.AddModelError("cardNumber", "Card number is required.");
                        if (string.IsNullOrWhiteSpace(cardExpiry)) ModelState.AddModelError("cardExpiry", "Card expiry is required.");
                        if (string.IsNullOrWhiteSpace(cardCvv)) ModelState.AddModelError("cardCvv", "CVV is required.");
                        break;
                    case "netbanking":
                        if (string.IsNullOrWhiteSpace(bankName)) ModelState.AddModelError("bankName", "Bank name is required.");
                        if (string.IsNullOrWhiteSpace(accountNumber)) ModelState.AddModelError("accountNumber", "Account number is required.");
                        if (string.IsNullOrWhiteSpace(accountHolder)) ModelState.AddModelError("accountHolder", "Account holder name is required.");
                        break;
                    case "upi":
                        if (string.IsNullOrWhiteSpace(upiId)) ModelState.AddModelError("upiId", "UPI ID is required.");
                        break;
                    default:
                        ModelState.AddModelError("paymentMethod", "Unknown payment method.");
                        break;
                }
            }

            if (!ModelState.IsValid)
            {
                // Re-populate view data so the form remains filled
                ViewData["Amount"] = amount.ToString("F2");
                ViewData["LoanId"] = loanId?.ToString() ?? "";
                return View("Pay");
            }

            // TODO: integrate payment gateway here. For now mock success.
            TempData["SuccessMessage"] = $"Payment of ₹{amount:N2} received. (LoanId: {loanId?.ToString() ?? "N/A"})";
            return RedirectToAction("Dashboard", "Account");
        }
    }
}