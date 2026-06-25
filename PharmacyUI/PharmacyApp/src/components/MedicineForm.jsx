import { useState } from 'react';

const empty = {
  fullName: '', notes: '', expiryDate: '',
  quantity: '', price: '', brand: '',
};

// Returns an object with a message for every field that fails.
// Empty object means everything is valid.
function validate(form) {
  const errors = {};

  if (!form.fullName.trim())
    errors.fullName = 'Full name is required.';
  else if (form.fullName.trim().length < 3)
    errors.fullName = 'Full name must be at least 3 characters.';

  if (!form.brand.trim())
    errors.brand = 'Brand is required.';

  if (!form.expiryDate)
    errors.expiryDate = 'Expiry date is required.';
  else if (new Date(form.expiryDate) <= new Date())
    errors.expiryDate = 'Expiry date must be in the future.';

  if (form.quantity === '')
    errors.quantity = 'Quantity is required.';
  else if (!Number.isInteger(Number(form.quantity)) || Number(form.quantity) < 0)
    errors.quantity = 'Quantity must be a whole number (0 or more).';

  if (form.price === '')
    errors.price = 'Price is required.';
  else if (isNaN(Number(form.price)) || Number(form.price) < 0)
    errors.price = 'Price must be a positive number.';
  else if (!/^\d+(\.\d{1,2})?$/.test(form.price))
    errors.price = 'Price can have at most 2 decimal places.';

  return errors;
}

function MedicineForm({ onSaved, onCancel }) {
  const [form, setForm]       = useState(empty);
  const [errors, setErrors]   = useState({});
  const [saving, setSaving]   = useState(false);
  const [apiError, setApiError] = useState(null);

  function handleChange(e) {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
    // clear the error for this field as soon as the user edits it
    if (errors[name]) setErrors((prev) => ({ ...prev, [name]: undefined }));
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setApiError(null);

    // run validation — if anything failed, show errors and stop
    const found = validate(form);
    if (Object.keys(found).length > 0) {
      setErrors(found);
      return;
    }

    setSaving(true);
    try {
      await onSaved({
        ...form,
        quantity: Number(form.quantity),
        price: parseFloat(form.price),
      });
      setForm(empty);
      setErrors({});
    } catch (err) {
      setApiError(err.message);
    } finally {
      setSaving(false);
    }
  }

  // helper — returns Bootstrap classes + the error message node for a field
  function fieldClass(name) {
    return `form-control${errors[name] ? ' is-invalid' : ''}`;
  }

  return (
    <form onSubmit={handleSubmit} noValidate>

      {/* API-level error (e.g. server 500) shown at the top */}
      {apiError && (
        <div className="alert alert-danger py-2 mb-3">{apiError}</div>
      )}

      {/* Full Name */}
      <div className="mb-3">
        <label className="form-label">
          Full Name <span className="text-danger">*</span>
        </label>
        <input
          className={fieldClass('fullName')}
          name="fullName"
          value={form.fullName}
          onChange={handleChange}
          placeholder="e.g. Paracetamol 500mg Tablets"
        />
        {errors.fullName && (
          <div className="invalid-feedback">{errors.fullName}</div>
        )}
      </div>

      {/* Brand */}
      <div className="mb-3">
        <label className="form-label">
          Brand <span className="text-danger">*</span>
        </label>
        <input
          className={fieldClass('brand')}
          name="brand"
          value={form.brand}
          onChange={handleChange}
          placeholder="e.g. Crocin"
        />
        {errors.brand && (
          <div className="invalid-feedback">{errors.brand}</div>
        )}
      </div>

      {/* Expiry / Quantity / Price — row of three */}
      <div className="row">
        <div className="col-md-4 mb-3">
          <label className="form-label">
            Expiry Date <span className="text-danger">*</span>
          </label>
          <input
            type="date"
            className={fieldClass('expiryDate')}
            name="expiryDate"
            value={form.expiryDate}
            onChange={handleChange}
          />
          {errors.expiryDate && (
            <div className="invalid-feedback">{errors.expiryDate}</div>
          )}
        </div>

        <div className="col-md-4 mb-3">
          <label className="form-label">
            Quantity <span className="text-danger">*</span>
          </label>
          <input
            type="number"
            min="0"
            className={fieldClass('quantity')}
            name="quantity"
            value={form.quantity}
            onChange={handleChange}
            placeholder="0"
          />
          {errors.quantity && (
            <div className="invalid-feedback">{errors.quantity}</div>
          )}
        </div>

        <div className="col-md-4 mb-3">
          <label className="form-label">
            Price (₹) <span className="text-danger">*</span>
          </label>
          <input
            type="number"
            min="0"
            step="0.01"
            className={fieldClass('price')}
            name="price"
            value={form.price}
            onChange={handleChange}
            placeholder="0.00"
          />
          {errors.price && (
            <div className="invalid-feedback">{errors.price}</div>
          )}
        </div>
      </div>

      {/* Notes — optional */}
      <div className="mb-3">
        <label className="form-label">
          Notes <span className="text-muted small">(optional)</span>
        </label>
        <textarea
          className="form-control"
          name="notes"
          rows={2}
          value={form.notes}
          onChange={handleChange}
          placeholder="Any additional notes about this medicine…"
        />
      </div>

      {/* Required field hint */}
      <p className="text-muted small mb-3">
        <span className="text-danger">*</span> Required fields
      </p>

      <div className="d-flex gap-2 justify-content-end">
        <button type="button" className="btn btn-secondary" onClick={onCancel}>
          Cancel
        </button>
        <button type="submit" className="btn btn-primary" disabled={saving}>
          {saving ? 'Saving…' : 'Add Medicine'}
        </button>
      </div>
    </form>
  );
}

export default MedicineForm;