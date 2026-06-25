import { useState, useEffect } from 'react';
import { getMedicines, addMedicine } from '../services/medicineService';
import { recordSale } from '../services/saleService';
import MedicineTable from '../components/MedicineTable';
import MedicineForm from '../components/MedicineForm';
import SaleForm from '../components/SaleForm';

function MedicinesPage() {
  const [medicines, setMedicines]   = useState([]);
  const [loading, setLoading]       = useState(true);
  const [error, setError]           = useState(null);
  const [search, setSearch]         = useState('');
  const [showAdd, setShowAdd]       = useState(false);
  const [selling, setSelling]       = useState(null); // the medicine being sold

  // load medicines on first render, and again whenever search changes
  useEffect(() => {
    setLoading(true);
    setError(null);
    getMedicines(search)
      .then(setMedicines)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, [search]);

  async function handleAdd(data) {
    const created = await addMedicine(data);   // throws on error → caught in MedicineForm
    setMedicines((prev) => [...prev, created]);// add to list without re-fetching
    setShowAdd(false);
  }

  async function handleSell(saleData) {
    await recordSale(saleData);                // throws on error → caught in SaleForm
    // refresh the list so the quantity updates immediately
    const updated = await getMedicines(search);
    setMedicines(updated);
    setSelling(null);
  }

  return (
    <div className="container">
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h2 className="mb-0">Medicines</h2>
        <button className="btn btn-primary" onClick={() => setShowAdd(true)}>
          + Add Medicine
        </button>
      </div>

      {/* Color-code legend */}
      <div className="d-flex gap-3 mb-3 small">
        <span><span className="badge bg-danger me-1">&nbsp;</span>Expiry &lt; 30 days</span>
        <span><span className="badge bg-warning text-dark me-1">&nbsp;</span>Stock &lt; 10</span>
      </div>

      {/* Search bar */}
      <div className="mb-3">
        <input
          className="form-control"
          placeholder="Search by medicine name…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>

      {/* Add medicine modal */}
      {showAdd && (
        <div className="modal show d-block" style={{ background: 'rgba(0,0,0,.4)' }}>
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Add Medicine</h5>
              </div>
              <div className="modal-body">
                <MedicineForm
                  onSaved={handleAdd}
                  onCancel={() => setShowAdd(false)}
                />
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Sell modal */}
      {selling && (
        <div className="modal show d-block" style={{ background: 'rgba(0,0,0,.4)' }}>
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Record Sale</h5>
              </div>
              <div className="modal-body">
                <SaleForm
                  medicine={selling}
                  onSaved={handleSell}
                  onCancel={() => setSelling(null)}
                />
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Main content */}
      {loading && <div className="text-center py-4"><div className="spinner-border"/></div>}
      {error   && <div className="alert alert-danger">{error}</div>}
      {!loading && !error && (
        <MedicineTable medicines={medicines} onSell={setSelling} />
      )}
    </div>
  );
}

export default MedicinesPage;