using PharmacyApi.Models;

namespace PharmacyApi.Services
{
    public class MedicineService : IMedicineService
    {
        private readonly JsonFileStore<Medicine> _store;
        private readonly object _lock = new();

        public MedicineService(JsonFileStore<Medicine> store) => _store = store;

        public IEnumerable<Medicine> GetAll() => _store.ReadAll();

        public Medicine? GetById(int id) =>
            _store.ReadAll().FirstOrDefault(m => m.Id == id);

        public IEnumerable<Medicine> Search(string name) =>
            _store.ReadAll()
                  .Where(m => m.FullName.Contains(name, StringComparison.OrdinalIgnoreCase));

        public Medicine Add(CreateMedicineRequest request)
        {
            lock (_lock)
            {
                var medicines = _store.ReadAll();
                var newId = medicines.Count == 0 ? 1 : medicines.Max(m => m.Id) + 1;

                var medicine = new Medicine
                {
                    Id = newId,
                    FullName = request.FullName,
                    Notes = request.Notes,
                    ExpiryDate = request.ExpiryDate,
                    Quantity = request.Quantity,
                    Price = request.Price,
                    Brand = request.Brand
                };

                medicines.Add(medicine);
                _store.WriteAll(medicines);
                return medicine;
            }
        }

        public bool TryReduceStock(int medicineId, int quantity, out Medicine? medicine, out string? error)
        {
            lock (_lock)
            {
                var medicines = _store.ReadAll();
                medicine = medicines.FirstOrDefault(m => m.Id == medicineId);

                if (medicine is null)
                {
                    error = $"Medicine with id {medicineId} was not found.";
                    return false;
                }

                if (medicine.Quantity < quantity)
                {
                    error = $"Insufficient stock for '{medicine.FullName}'. " +
                            $"Available: {medicine.Quantity}, requested: {quantity}.";
                    return false;
                }

                medicine.Quantity -= quantity;
                _store.WriteAll(medicines);
                error = null;
                return true;
            }
        }
    }
}
