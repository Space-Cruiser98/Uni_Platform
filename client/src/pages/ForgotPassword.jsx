import { useState } from 'react';
import { Link } from 'react-router-dom';
import { auth } from '../api';
import styles from './Auth.module.css';

export default function ForgotPassword() {
  const [email, setEmail] = useState('');
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();

    setMessage('');
    setError('');
    setLoading(true);

    try {
      const res = await auth.forgotPassword(email);

      setMessage(
        res.message ||
        'If an account exists for this email, a password reset link has been sent.'
      );

      setEmail('');
    } catch (err) {
      setError(
        err.message ||
        'Unable to process your request.'
      );
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className={styles.container}>
      <h1>Components Order</h1>
      <h2>Forgot password</h2>

      {message && (
        <div className={styles.success}>
          {message}
        </div>
      )}

      {error && (
        <div className={styles.error}>
          {error}
        </div>
      )}

      {!message && (
        <>
          <p>
            Enter the email address associated with
            your account.
          </p>

          <form
            onSubmit={handleSubmit}
            className={styles.form}
          >
            <input
              type="email"
              placeholder="Email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              autoComplete="email"
            />

            <button
              type="submit"
              disabled={loading}
            >
              {loading
                ? 'Sending…'
                : 'Send reset link'}
            </button>
          </form>
        </>
      )}

      <p>
        <Link to="/login">
          Back to login
        </Link>
      </p>
    </div>
  );
}