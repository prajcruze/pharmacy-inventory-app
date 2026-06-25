using PharmacyApi.Models;

namespace PharmacyApi.Services
{
    public interface ISaleService
    {
        IEnumerable<Sale> GetAll();
        Sale RecordSale(CreateSaleRequest request);
    }
}
