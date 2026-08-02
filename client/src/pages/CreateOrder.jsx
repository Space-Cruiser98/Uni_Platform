import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { orders as ordersApi } from '../api';
import styles from './CreateOrder.module.css';

const emptyLine = () => ({ componentName: '', quantity: 1, description: '' });

export default function CreateOrder() {
  const [lines, setLines] = useState([emptyLine()]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  function addLine() {
    setLines((prev) => [...prev, emptyLine()]);
  }

  function removeLine(i) {
    setLines((prev) => (prev.length <= 1 ? prev : prev.filter((_, idx) => idx !== i)));
  }

  function updateLine(i, field, value) {
    setLines((prev) => {
      const next = [...prev];
      next[i] = { ...next[i], [field]: value };
      if (field === 'quantity') next[i].quantity = Math.max(1, parseInt(value, 10) || 1);
      return next;
    });
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setError('');
    const payload = lines
      .filter((l) => l.componentName.trim())
      .map((l) => ({
        componentName: l.componentName.trim(),
        quantity: l.quantity,
        description: l.description?.trim() || null,
      }));
    if (payload.length === 0) {
      setError('Add at least one component with a name and quantity.');
      return;
    }
    setLoading(true);
    try {
      const order = await ordersApi.create(payload);
      navigate(`/orders/${order.id}`, { replace: true });
    } catch (err) {
      setError(err.message || 'Failed to create order');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div>
      <h1>New order</h1>
      <form onSubmit={handleSubmit} className={styles.form}>
        {error && <div className={styles.error}>{error}</div>}
        <div className={styles.lines}>
          {lines.map((line, i) => (
            <div key={i} className={styles.line}>
              <input
                placeholder="Component name"
                value={line.componentName}
                onChange={(e) => updateLine(i, 'componentName', e.target.value)}
                required
              />
              <input
                type="number"
                min={1}
                value={line.quantity}
                onChange={(e) => updateLine(i, 'quantity', e.target.value)}
              />
              <input
                placeholder="Description (optional)"
                value={line.description || ''}
                onChange={(e) => updateLine(i, 'description', e.target.value)}
              />
              <button type="button" onClick={() => removeLine(i)} className={styles.removeBtn}>
                Remove
              </button>
            </div>
          ))}
        </div>
        <button type="button" onClick={addLine} className={styles.addBtn}>
          Add line
        </button>
        <div className={styles.actions}>
          <button type="submit" disabled={loading}>
            {loading ? 'Submitting…' : 'Submit order'}
          </button>
          <button type="button" onClick={() => navigate(-1)} className={styles.cancel}>
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
}
