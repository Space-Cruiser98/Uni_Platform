import { useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { auth } from '../api';
import styles from './Auth.module.css';

export default function ResetPassword() {
  const [searchParams] = useSearchParams();

  const token = searchParams.get('token');

  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');

  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();

    setMessage('');
    setError('');

    if (!token) {
      setError('Invalid password reset link.');
      return;
    }

    if (password.length < 6) {
      setError(
        'Password must contain at least 6 characters.'
      );
      return;
    }

    if (password !== confirmPassword) {
      setError('Passwords do not match.');
      return;
    }

    setLoading(true);

    try {
      const res = await auth.resetPassword(
        token,
        password
      );

      setMessage(
        res.message ||
        'Password reset successfully.'
      );

      setPassword('');
      setConfirmPassword('');
    } catch (err) {
      setError(
        err.message ||
        'This reset link is invalid or has expired.'
      );
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className={styles.container}>
      <h1>Components Order</h1>
      <h2>Reset password</h2>

      {message && (
        <>
          <div className={styles.success}>
            {message}
          </div>

          <p>
            <Link to="/login">
              Sign in with your new password
            </Link>
          </p>
        </>
      )}

      {!message && (
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
            type="password"
            placeholder="New password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            minLength={6}
            autoComplete="new-password"
          />

          <input
            type="password"
            placeholder="Confirm new password"
            value={confirmPassword}
            onChange={(e) =>
              setConfirmPassword(e.target.value)
            }
            required
            minLength={6}
            autoComplete="new-password"
          />

          <button
            type="submit"
            disabled={loading}
          >
            {loading
              ? 'Resetting…'
              : 'Reset password'}
          </button>
        </form>
      )}

      {!message && (
        <p>
          <Link to="/login">
            Back to login
          </Link>
        </p>
      )}
    </div>
  );
}