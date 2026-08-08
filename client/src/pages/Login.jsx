import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import styles from './Auth.module.css';

export default function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const { login } = useAuth();

  async function handleSubmit(e) {
    e.preventDefault();

    setError('');
    setLoading(true);

    try {
      await login(email, password);

      window.location.href = '/';
    } catch (err) {
      const msg = err.message || 'Login failed';

      if (
        msg.includes('Failed to fetch') ||
        msg.includes('NetworkError')
      ) {
        setError(
          'Cannot reach server. Please try again later.'
        );
      } else {
        setError(msg);
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className={styles.container}>
      <img
        src="https://upload.wikimedia.org/wikipedia/commons/thumb/d/df/Logo_ENISo%2C_Tunisie.svg/960px-Logo_ENISo%2C_Tunisie.svg.png"
        alt="ENISO logo"
        className={styles.logo}
      />

      <h1 className={styles.title}>
        ENISO Components Order
      </h1>

      <h2>Sign in</h2>

      <form
        onSubmit={handleSubmit}
        className={styles.form}
      >
        {error && (
          <div className={styles.error}>
            {error}
          </div>
        )}

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
        <Link to="/forgot-password">
          Forgot your password?
        </Link>
      </p>

      <p>
        No account?{' '}
        <Link to="/register">Register</Link>
      </p>
    </div>
  );
}