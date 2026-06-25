using System.ComponentModel.DataAnnotations;

namespace PharmacyApi.Models
{
    public class CreateSaleRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "A valid MedicineId is required.")]
        public int MedicineId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "QuantitySold must be at least 1.")]
        public int QuantitySold { get; set; }
    }
}
