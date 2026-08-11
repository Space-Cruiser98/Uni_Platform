import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { orders as ordersApi } from '../api';
import styles from './Dashboard.module.css';

export default function Dashboard() {
  const { isAdmin } = useAuth();
  const [orders, setOrders] = useState([]);
  const [statusFilter, setStatusFilter] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    ordersApi
      .list(statusFilter || undefined)
      .then((data) => {
        if (!cancelled) setOrders(data);
      })
      .catch((err) => {
        if (!cancelled) setError(err.message);
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => { cancelled = true; };
  }, [statusFilter, isAdmin]);

  const title = isAdmin ? 'All orders' : 'My orders';

  return (
    <div>
      <div className={styles.top}>
        <h1>{title}</h1>
        {isAdmin && (
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className={styles.filter}
          >
          <option value="">All statuses</option>
          <option value="Submitted">Submitted</option>
          <option value="Approved">Approved</option>
          <option value="Rejected">Rejected</option>
          <option value="Taken">Taken</option>
          <option value="Returned">Returned</option>
          </select>
        )}
        {!isAdmin && (
          <Link to="/orders/new" className={styles.newBtn}>
            New order
          </Link>
        )}
      </div>

      {error && <div className={styles.error}>{error}</div>}
      {loading && <div className={styles.loading}>Loading…</div>}

      {!loading && !error && (
        <div className={styles.tableWrap}>
          <table className={styles.table}>
            <thead>
              <tr>
                <th>Id</th>
                {isAdmin && <th>Student</th>}
                <th>Status</th>
                <th>Created</th>
                <th>Updated</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {orders.length === 0 ? (
                <tr>
                  <td colSpan={isAdmin ? 6 : 5} className={styles.empty}>
                    No orders
                  </td>
                </tr>
              ) : (
                orders.map((o) => (
                  <tr key={o.id}>
                    <td>{o.id}</td>
                    {isAdmin && (
                      <td>
                        {o.studentName} <span className={styles.email}>({o.studentEmail})</span>
                      </td>
                    )}
                    <td>
                      <span className={styles[`status_${o.status.toLowerCase()}`]}>{o.status}</span>
                    </td>
                    <td>{new Date(o.createdAt).toLocaleString()}</td>
                    <td>{new Date(o.updatedAt).toLocaleString()}</td>
                    <td>
                      <Link to={`/orders/${o.id}`} className={styles.link}>
                        View
                      </Link>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
