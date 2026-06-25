function rowClass(medicine) {
  const today = new Date();
  const expiry = new Date(medicine.expiryDate);
  const daysLeft = Math.ceil((expiry - today) / (1000 * 60 * 60 * 24));

  if (daysLeft < 30) return 'table-danger';   // red  — expiry < 30 days
  if (medicine.quantity < 10) return 'table-warning'; // yellow — stock < 10
  return '';
}

function MedicineTable({ medicines, onSell }) {
  if (medicines.length === 0) {
    return <p className="text-muted">No medicines found.</p>;
  }

  return (
    <div className="table-responsive">
      <table className="table table-bordered table-hover align-middle">
        <thead className="table-dark">
          <tr>
            <th>Full Name</th>
            <th>Brand</th>
            <th>Expiry Date</th>
            <th className="text-center">Quantity</th>
            <th className="text-end">Price (₹)</th>
            <th className="text-center">Action</th>
          </tr>
        </thead>
        <tbody>
          {medicines.map((m) => (
            <tr key={m.id} className={rowClass(m)}>
              <td>{m.fullName}</td>
              <td>{m.brand}</td>
              <td>{new Date(m.expiryDate).toLocaleDateString()}</td>
              <td className="text-center">{m.quantity}</td>
              <td className="text-end">{m.price.toFixed(2)}</td>
              <td className="text-center">
                <button
                  className="btn btn-sm btn-outline-dark"
                  onClick={() => onSell(m)}
                >
                  Sell
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default MedicineTable;