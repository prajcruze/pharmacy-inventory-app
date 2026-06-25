using PharmacyApi.Models;

namespace PharmacyApi.Services
{
    public class SaleService : ISaleService
    {
        private readonly JsonFileStore<Sale> _store;
        private readonly IMedicineService _medicineService;
        private readonly object _lock = new();

        public SaleService(JsonFileStore<Sale> store, IMedicineService medicineService)
        {
            _store = store;
            _medicineService = medicineService;
        }

        public IEnumerable<Sale> GetAll() =>
            _store.ReadAll().OrderByDescending(s => s.SaleDate);

        public Sale RecordSale(CreateSaleRequest request)
        {
            if (!_medicineService.TryReduceStock(
                    request.MedicineId, request.QuantitySold, out var medicine, out var error))
            {
                throw new InvalidOperationException(error);
            }

            lock (_lock)
            {
                var sales = _store.ReadAll();
                var newId = sales.Count == 0 ? 1 : sales.Max(s => s.Id) + 1;

                var sale = new Sale
                {
                    Id = newId,
                    MedicineId = medicine!.Id,
                    MedicineName = medicine.FullName,
                    QuantitySold = request.QuantitySold,
                    UnitPrice = medicine.Price,
                    TotalPrice = medicine.Price * request.QuantitySold,
                    SaleDate = DateTime.UtcNow
                };

                sales.Add(sale);
                _store.WriteAll(sales);
                return sale;
            }
        }
    }
}
