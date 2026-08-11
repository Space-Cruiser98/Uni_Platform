import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { orders as ordersApi } from '../api';
import styles from './OrderDetail.module.css';

export default function OrderDetail() {
  const { id } = useParams();
  const { isAdmin } = useAuth();

  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [actionLoading, setActionLoading] = useState(false);
  const [rejectReason, setRejectReason] = useState('');

  useEffect(() => {
    let cancelled = false;

    ordersApi
      .get(Number(id))
      .then((data) => {
        if (!cancelled) setOrder(data);
      })
      .catch((err) => {
        if (!cancelled) setError(err.message);
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [id]);

  async function handleStatus(newStatus, reason) {
    setActionLoading(true);
    setError('');

    try {
      const updated = await ordersApi.updateStatus(
        Number(id),
        newStatus,
        reason
      );

      setOrder(updated);
      setRejectReason('');
    } catch (err) {
      setError(err.message);
    } finally {
      setActionLoading(false);
    }
  }

  if (loading) {
    return <div className={styles.loading}>Loading…</div>;
  }

  if (error && !order) {
    return <div className={styles.error}>{error}</div>;
  }

  if (!order) {
    return null;
  }

  // Technician/Admin actions
  const canApproveReject =
    isAdmin && order.status === 'Submitted';

  const canMarkTaken =
    isAdmin && order.status === 'Approved';

  const canMarkReturned =
    isAdmin && order.status === 'Taken';

  return (
    <div>
      <div className={styles.header}>
        <Link to="/" className={styles.back}>
          ← Dashboard
        </Link>

        <h1>Order #{order.id}</h1>
      </div>

      {error && (
        <div className={styles.error}>
          {error}
        </div>
      )}

      <section className={styles.section}>
        <h2>Details</h2>

        <p>
          <strong>Student:</strong>{' '}
          {order.studentName} ({order.studentEmail})
        </p>

        <p>
          <strong>Status:</strong>{' '}
          <span
            className={
              styles[
                `status_${order.status.toLowerCase()}`
              ]
            }
          >
            {order.status}
          </span>
        </p>

        <p>
          <strong>Created:</strong>{' '}
          {new Date(order.createdAt).toLocaleString()}
        </p>

        <p>
          <strong>Updated:</strong>{' '}
          {new Date(order.updatedAt).toLocaleString()}
        </p>

        {order.rejectionReason && (
          <p>
            <strong>Rejection reason:</strong>{' '}
            {order.rejectionReason}
          </p>
        )}
      </section>

      <section className={styles.section}>
        <h2>Items</h2>

        <table className={styles.table}>
          <thead>
            <tr>
              <th>Component</th>
              <th>Quantity</th>
              <th>Description</th>
            </tr>
          </thead>

          <tbody>
            {order.lines.map((l) => (
              <tr key={l.id}>
                <td>{l.componentName}</td>
                <td>{l.quantity}</td>
                <td>{l.description || '—'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>

      <section className={styles.section}>
        <h2>Traceability</h2>

        <ul className={styles.timeline}>
          <li>
            <strong>Submitted</strong>
            {' — '}
            {new Date(
              order.createdAt
            ).toLocaleString()}
          </li>

          {order.statusHistory?.map((h) => (
            <li key={h.id}>
              <strong>
                {h.fromStatus} → {h.toStatus}
              </strong>

              {' — '}

              {new Date(
                h.changedAt
              ).toLocaleString()}

              {h.changedByUserName &&
                ` by ${h.changedByUserName}`}

              {h.note &&
                ` (${h.note})`}
            </li>
          ))}
        </ul>
      </section>

      {/* SUBMITTED → APPROVED / REJECTED */}
      {canApproveReject && (
        <section className={styles.section}>
          <h2>Actions</h2>

          <div className={styles.actions}>
            <button
              type="button"
              onClick={() =>
                handleStatus('Approved')
              }
              disabled={actionLoading}
              className={styles.approveBtn}
            >
              Approve
            </button>

            <div className={styles.rejectRow}>
              <input
                type="text"
                placeholder="Rejection reason (optional)"
                value={rejectReason}
                onChange={(e) =>
                  setRejectReason(e.target.value)
                }
                className={styles.rejectInput}
              />

              <button
                type="button"
                onClick={() =>
                  handleStatus(
                    'Rejected',
                    rejectReason
                  )
                }
                disabled={actionLoading}
                className={styles.rejectBtn}
              >
                Reject
              </button>
            </div>
          </div>
        </section>
      )}

      {/* APPROVED → TAKEN */}
      {canMarkTaken && (
        <section className={styles.section}>
          <h2>Actions</h2>

          <p>
            The order has been approved. Mark it as
            taken when the student receives the
            components.
          </p>

          <button
            type="button"
            onClick={() =>
              handleStatus('Taken')
            }
            disabled={actionLoading}
            className={styles.doneBtn}
          >
            Mark as Taken
          </button>
        </section>
      )}

      {/* TAKEN → RETURNED */}
      {canMarkReturned && (
        <section className={styles.section}>
          <h2>Actions</h2>

          <p>
            Mark the order as returned when the
            student brings the components back.
          </p>

          <button
            type="button"
            onClick={() =>
              handleStatus('Returned')
            }
            disabled={actionLoading}
            className={styles.doneBtn}
          >
            Mark as Returned
          </button>
        </section>
      )}
    </div>
  );
}