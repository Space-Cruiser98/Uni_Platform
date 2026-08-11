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

  // Approval
  const [approvalScope, setApprovalScope] = useState('');

  // Rejection
  const [rejectionReason, setRejectionReason] = useState('');
  const [rejectNote, setRejectNote] = useState('');

  // Taken
  const [takenNote, setTakenNote] = useState('');

  // Returned
  const [returnCondition, setReturnCondition] = useState('');
  const [returnNote, setReturnNote] = useState('');

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

  async function handleStatus(newStatus, data = {}) {
    setActionLoading(true);
    setError('');

    try {
      const updated = await ordersApi.updateStatus(
        Number(id),
        newStatus,
        data
      );

      setOrder(updated);

      // Reset fields after successful update
      setApprovalScope('');
      setRejectionReason('');
      setRejectNote('');
      setTakenNote('');
      setReturnCondition('');
      setReturnNote('');
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

              {h.approvalScope &&
                ` — Approval: ${formatApprovalScope(
                  h.approvalScope
                )}`}

              {h.rejectionReason &&
                ` — Reason: ${formatRejectionReason(
                  h.rejectionReason
                )}`}

              {h.returnCondition &&
                ` — Return: ${formatReturnCondition(
                  h.returnCondition
                )}`}

              {h.note &&
                ` — ${h.note}`}
            </li>
          ))}
        </ul>
      </section>

      {/* =====================================================
          SUBMITTED → APPROVED / REJECTED
          ===================================================== */}

      {canApproveReject && (
        <>
          {/* APPROVE */}
          <section className={styles.section}>
            <h2>Approve Order</h2>

            <div className={styles.formGroup}>
              <label>
                Components
              </label>

              <select
                value={approvalScope}
                onChange={(e) =>
                  setApprovalScope(e.target.value)
                }
                disabled={actionLoading}
              >
                <option value="">
                  Select an option
                </option>

                <option value="AllComponents">
                  All components
                </option>

                <option value="NotAllComponents">
                  Not all components
                </option>
              </select>
            </div>

            {approvalScope === 'NotAllComponents' && (
              <div className={styles.formGroup}>
                <label>
                  Explanation (optional)
                </label>

                <textarea
                  placeholder="Explain which component(s) are not approved..."
                  value={rejectNote}
                  onChange={(e) =>
                    setRejectNote(e.target.value)
                  }
                  disabled={actionLoading}
                />
              </div>
            )}

            <button
              type="button"
              onClick={() =>
                handleStatus('Approved', {
                  approvalScope,
                  note: rejectNote || null,
                })
              }
              disabled={
                actionLoading || !approvalScope
              }
              className={styles.approveBtn}
            >
              Approve
            </button>
          </section>

          {/* REJECT */}
          <section className={styles.section}>
            <h2>Reject Order</h2>

            <div className={styles.formGroup}>
              <label>
                Reason
              </label>

              <select
                value={rejectionReason}
                onChange={(e) =>
                  setRejectionReason(e.target.value)
                }
                disabled={actionLoading}
              >
                <option value="">
                  Select a reason
                </option>

                <option value="UnavailableComponents">
                  Unavailable Components
                </option>

                <option value="AlreadyLoaned">
                  Already Loaned
                </option>
              </select>
            </div>

            <div className={styles.formGroup}>
              <label>
                Explanation (optional)
              </label>

              <textarea
                placeholder="Add an optional explanation..."
                value={rejectNote}
                onChange={(e) =>
                  setRejectNote(e.target.value)
                }
                disabled={actionLoading}
              />
            </div>

            <button
              type="button"
              onClick={() =>
                handleStatus('Rejected', {
                  rejectionReason,
                  note: rejectNote || null,
                })
              }
              disabled={
                actionLoading || !rejectionReason
              }
              className={styles.rejectBtn}
            >
              Reject
            </button>
          </section>
        </>
      )}

      {/* =====================================================
          APPROVED → TAKEN
          ===================================================== */}

      {canMarkTaken && (
        <section className={styles.section}>
          <h2>Mark as Taken</h2>

          <div className={styles.formGroup}>
            <label>
              Remarks (optional)
            </label>

            <textarea
              placeholder="Add optional remarks..."
              value={takenNote}
              onChange={(e) =>
                setTakenNote(e.target.value)
              }
              disabled={actionLoading}
            />
          </div>

          <button
            type="button"
            onClick={() =>
              handleStatus('Taken', {
                note: takenNote || null,
              })
            }
            disabled={actionLoading}
            className={styles.doneBtn}
          >
            Mark as Taken
          </button>
        </section>
      )}

      {/* =====================================================
          TAKEN → RETURNED
          ===================================================== */}

      {canMarkReturned && (
        <section className={styles.section}>
          <h2>Mark as Returned</h2>

          <div className={styles.formGroup}>
            <label>
              Return condition
            </label>

            <select
              value={returnCondition}
              onChange={(e) =>
                setReturnCondition(e.target.value)
              }
              disabled={actionLoading}
            >
              <option value="">
                Select return condition
              </option>

              <option value="AllComponentsReturned">
                All components returned
              </option>

              <option value="MissingComponents">
                Missing components
              </option>

              <option value="DamagedComponents">
                Damaged components
              </option>
            </select>
          </div>

          {(returnCondition === 'MissingComponents' ||
            returnCondition === 'DamagedComponents') && (
            <div className={styles.formGroup}>
              <label>
                Remarks (optional)
              </label>

              <textarea
                placeholder={
                  returnCondition ===
                  'MissingComponents'
                    ? 'Specify the missing component(s)...'
                    : 'Describe the damaged component(s)...'
                }
                value={returnNote}
                onChange={(e) =>
                  setReturnNote(e.target.value)
                }
                disabled={actionLoading}
              />
            </div>
          )}

          <button
            type="button"
            onClick={() =>
              handleStatus('Returned', {
                returnCondition,
                note: returnNote || null,
              })
            }
            disabled={
              actionLoading || !returnCondition
            }
            className={styles.doneBtn}
          >
            Mark as Returned
          </button>
        </section>
      )}
    </div>
  );
}

/* ============================================================
   Display helpers
   ============================================================ */

function formatApprovalScope(value) {
  switch (value) {
    case 'AllComponents':
      return 'All components';

    case 'NotAllComponents':
      return 'Not all components';

    default:
      return value;
  }
}

function formatRejectionReason(value) {
  switch (value) {
    case 'UnavailableComponents':
      return 'Unavailable Components';

    case 'AlreadyLoaned':
      return 'Already Loaned';

    default:
      return value;
  }
}

function formatReturnCondition(value) {
  switch (value) {
    case 'AllComponentsReturned':
      return 'All components returned';

    case 'MissingComponents':
      return 'Missing components';

    case 'DamagedComponents':
      return 'Damaged components';

    default:
      return value;
  }
}