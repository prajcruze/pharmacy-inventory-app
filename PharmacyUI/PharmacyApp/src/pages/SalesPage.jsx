import { useState, useEffect } from 'react';
import { getSales } from '../services/saleService';

function SalesPage() {
  const [sales, setSales]     = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError]     = useState(null);

  useEffect(() => {
    getSales()
      .then(setSales)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, []);

  return (
    <div className="container">
      <h2 className="mb-4">Sales History</h2>

      {loading && <div className="text-center py-4"><div className="spinner-border"/></div>}
      {error   && <div className="alert alert-danger">{error}</div>}

      {!loading && !error && (
        sales.length === 0
          ? <p className="text-muted">No sales recorded yet.</p>
          : (
            <div className="table-responsive">
              <table className="table table-bordered table-hover align-middle">
                <thead className="table-dark">
                  <tr>
                    <th>Date</th>
                    <th>Medicine</th>
                    <th className="text-center">Qty Sold</th>
                    <th className="text-end">Unit Price</th>
                    <th className="text-end">Total</th>
                  </tr>
                </thead>
                <tbody>
                  {sales.map((s) => (
                    <tr key={s.id}>
                      <td>{new Date(s.saleDate).toLocaleString()}</td>
                      <td>{s.medicineName}</td>
                      <td className="text-center">{s.quantitySold}</td>
                      <td className="text-end">₹{s.unitPrice.toFixed(2)}</td>
                      <td className="text-end fw-semibold">₹{s.totalPrice.toFixed(2)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )
      )}
    </div>
  );
}

export default SalesPage;