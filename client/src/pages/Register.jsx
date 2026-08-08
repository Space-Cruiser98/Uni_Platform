import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import styles from './Auth.module.css';

export default function Register() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [name, setName] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);

  const { register } = useAuth();

  async function handleSubmit(e) {
    e.preventDefault();

    setError('');
    setSuccess('');
    setLoading(true);

    try {
      const result = await register(email, password, name);

      setSuccess(
        result.message ||
        'Registration successful. Please check your email to verify your account.'
      );

      setEmail('');
      setPassword('');
      setName('');
    } catch (err) {
      setError(err.message || 'Registration failed');
    } finally {
      setLoading(false);
    }
  }

  if (success) {
    return (
      <div className={styles.container}>
        <h1>Components Order</h1>
        <h2>Check your email</h2>

        <div className={styles.success}>
          {success}
        </div>

        <p>
          We sent a verification link to the email address
          you provided.
        </p>

        <p>
          After verifying your email, you can{' '}
          <Link to="/login">sign in</Link>.
        </p>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      <h1>Components Order</h1>
      <h2>Register (student)</h2>

      <form onSubmit={handleSubmit} className={styles.form}>
        {error && (
          <div className={styles.error}>
            {error}
          </div>
        )}

        <input
          type="text"
          placeholder="Name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          required
          autoComplete="name"
        />

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
          autoComplete="new-password"
          minLength={6}
        />

        <button type="submit" disabled={loading}>
          {loading ? 'Registering…' : 'Register'}
        </button>
      </form>

      <p>
        Already have an account?{' '}
        <Link to="/login">Sign in</Link>
      </p>
    </div>
  );
}