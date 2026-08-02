import { Outlet, Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import styles from './Layout.module.css';

export default function Layout() {
  const { user, logout, isAdmin } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate('/login');
  }

  return (
    <div className={styles.wrapper}>
      <header className={styles.header}>
        <Link to="/" className={styles.logo}>
          Components Order
        </Link>
        <nav className={styles.nav}>
          <Link to="/">Dashboard</Link>
          {!isAdmin && <Link to="/orders/new">New order</Link>}
          <span className={styles.user}>
            {user?.name} ({user?.role})
          </span>
          <button type="button" onClick={handleLogout} className={styles.logout}>
            Logout
          </button>
        </nav>
      </header>
      <main className={styles.main}>
        <Outlet />
      </main>
    </div>
  );
}
