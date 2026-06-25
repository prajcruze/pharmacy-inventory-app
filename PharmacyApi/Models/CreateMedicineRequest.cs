using System.ComponentModel.DataAnnotations;

namespace PharmacyApi.Models
{
    public class CreateMedicineRequest
    {
        [Required, StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        public string? Notes { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative.")]
        public decimal Price { get; set; }

        [Required, StringLength(100)]
        public string Brand { get; set; } = string.Empty;
    }
}
