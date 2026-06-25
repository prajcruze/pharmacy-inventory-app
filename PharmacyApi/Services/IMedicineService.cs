using PharmacyApi.Models;

namespace PharmacyApi.Services
{
    public interface IMedicineService
    {
        IEnumerable<Medicine> GetAll();
        Medicine? GetById(int id);
        IEnumerable<Medicine> Search(string name);
        Medicine Add(CreateMedicineRequest request);
        bool TryReduceStock(int medicineId, int quantity, out Medicine? medicine, out string? error);
    }
}
