import { useState } from 'react';

function validate(quantity, medicine) {
  const qty = Number(quantity);

  if (!quantity && quantity !== 0)
    return 'Quantity is required.';

  if (!Number.isInteger(qty) || qty < 1)
    return 'Quantity must be a whole number of at least 1.';

  if (qty > medicine.quantity)
    return `Insufficient stock for '${medicine.fullName}'. Available: ${medicine.quantity}, requested: ${qty}.`;

  return null; // null means valid
}

function SaleForm({ medicine, onSaved, onCancel }) {
  const [quantity, setQuantity] = useState('');
  const [error, setError]       = useState(null);
  const [saving, setSaving]     = useState(false);

  function handleChange(e) {
    setQuantity(e.target.value);
    // clear the error as soon as the user edits the field
    if (error) setError(null);
  }

  async function handleSubmit(e) {
    e.preventDefault();

    // client-side validation first — no API call if this fails
    const validationError = validate(quantity, medicine);
    if (validationError) {
      setError(validationError);
      return;
    }

    setSaving(true);
    setError(null);
    try {
      await onSaved({ medicineId: medicine.id, quantitySold: Number(quantity) });
    } catch (err) {
      // API-level error (e.g. race condition where stock just changed)
      setError(err.message);
    } finally {
      setSaving(false);
    }
  }

  const qty = Number(quantity);
  const total = qty > 0 ? (medicine.price * qty).toFixed(2) : '0.00';

  return (
    <form onSubmit={handleSubmit} noValidate>

      <div className="mb-3">
        <p className="mb-1">
          <strong>Medicine:</strong> {medicine.fullName}
        </p>
        <p className="mb-0">
          <strong>Brand:</strong> {medicine.brand} &nbsp;|&nbsp;
          <strong>In stock:</strong> {medicine.quantity} &nbsp;|&nbsp;
          <strong>Price:</strong> ₹{medicine.price.toFixed(2)} each
        </p>
      </div>

      <div className="mb-3">
        <label className="form-label">
          Quantity to sell <span className="text-danger">*</span>
        </label>
        <input
          type="number"
          className={`form-control${error ? ' is-invalid' : ''}`}
          value={quantity}
          onChange={handleChange}
          placeholder="Enter quantity"
          autoFocus
        />
        {error && (
          <div className="invalid-feedback">{error}</div>
        )}
      </div>

      <div className={`alert py-2 mb-3 ${qty > medicine.quantity ? 'alert-danger' : 'alert-info'}`}>
        Total: ₹{total}
      </div>

      <div className="d-flex gap-2 justify-content-end">
        <button type="button" className="btn btn-secondary" onClick={onCancel}>
          Cancel
        </button>
        <button type="submit" className="btn btn-success" disabled={saving}>
          {saving ? 'Recording…' : 'Confirm Sale'}
        </button>
      </div>
    </form>
  );
}

export default SaleForm;