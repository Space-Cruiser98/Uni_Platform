import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import styles from './Auth.module.css';

export default function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  async function handleSubmit(e) {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await login(email, password);
      // Full reload so app loads with token and /me runs; avoids React state timing issues
      window.location.href = '/';
    } catch (err) {
      const msg = err.message || 'Login failed';
      setError(msg.includes('Failed to fetch') || msg.includes('NetworkError') ? 'Cannot reach server. Start the backend (dotnet run in src/ComponentsOrderApi) and refresh.' : msg);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className={styles.container}>
      <h1>Components Order</h1>
      <h2>Sign in</h2>
      <form onSubmit={handleSubmit} className={styles.form}>
        {error && <div className={styles.error}>{error}</div>}
        <input
          type="email"
          placeholder="Email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
          autoComplete="email"
        />
        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
          autoComplete="current-password"
        />
        <button type="submit" disabled={loading}>
          {loading ? 'Signing in…' : 'Sign in'}
        </button>
      </form>
      <p>
        No account? <Link to="/register">Register</Link>
      </p>
      <p className={styles.hint}>
        Demo admin: <strong>admin1@school.edu</strong> / <strong>Admin1!</strong> or <strong>technician@school.edu</strong> / <strong>Technician1!</strong>
      </p>
    </div>
  );
}
