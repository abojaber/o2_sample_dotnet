using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication1.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public string? Username { get; set; }
        public string? Password { get; set; }

        public string? BillNumber { get; set; }
        public decimal BillAmount { get; set; }
        public string? Message { get; set; }
        public bool BillCreated { get; set; }
        public bool BillPaid { get; set; }
        public string? RumAction { get; set; }
        public string? RumPayload { get; set; }

        private static readonly Dictionary<string, Bill> Bills = new();

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            Username = TempData["Username"] as string;
            Password = TempData["Password"] as string;
            RumAction = TempData["RumAction"] as string;
            RumPayload = TempData["RumPayload"] as string;

            if (TempData.Peek("BillNumber") is string billNumber && Bills.TryGetValue(billNumber, out var bill))
            {
                BillNumber = billNumber;
                BillAmount = bill.Amount;
                BillCreated = true;
                BillPaid = bill.Paid;
            }
        }

        public IActionResult OnPostCreateBill()
        {
            var billNumber = $"BILL-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
            var items = new[]
            {
                new { Name = "Item A - Widget", Price = 29.99m },
                new { Name = "Item B - Gadget", Price = 49.99m }
            };
            var amount = items.Sum(i => i.Price);

            Bills[billNumber] = new Bill { Amount = amount, Paid = false };

            TempData["BillNumber"] = billNumber;

            var payload = new { billNumber, amount, items = items.Select(i => new { i.Name, i.Price }) };
            TempData["RumAction"] = "bill-created";
            TempData["RumPayload"] = System.Text.Json.JsonSerializer.Serialize(payload);

            _logger.LogInformation("PAYMENT_DEMO Bill {Action}: {BillNumber} {ItemCount} items {Amount:C}",
                "CREATED", billNumber, items.Length, amount);

            return RedirectToPage();
        }

        public IActionResult OnPostPayBill()
        {
            if (TempData.Peek("BillNumber") is not string billNumber || !Bills.TryGetValue(billNumber, out var bill))
            {
                return RedirectToPage();
            }

            if (bill.Paid)
            {
                return RedirectToPage();
            }

            bill.Paid = true;

            var payload = new { billNumber, amount = bill.Amount, status = "paid" };
            TempData["RumAction"] = "bill-paid";
            TempData["RumPayload"] = System.Text.Json.JsonSerializer.Serialize(payload);

            _logger.LogInformation("PAYMENT_DEMO Bill {Action}: {BillNumber} amount {Amount:C}",
                "PAID", billNumber, bill.Amount);

            return RedirectToPage();
        }

        public class Bill
        {
            public decimal Amount { get; set; }
            public bool Paid { get; set; }
        }
    }
}