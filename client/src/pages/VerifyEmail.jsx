import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { auth } from '../api';
import styles from './Auth.module.css';

export default function VerifyEmail() {
  const [searchParams] = useSearchParams();

  const [status, setStatus] = useState('loading');
  const [message, setMessage] = useState('');

  useEffect(() => {
    const token = searchParams.get('token');

    if (!token) {
      setStatus('error');
      setMessage('Invalid verification link.');
      return;
    }

    auth
      .verifyEmail(token)
      .then((res) => {
        setStatus('success');
        setMessage(
          res.message || 'Your email has been verified successfully.'
        );
      })
      .catch((err) => {
        setStatus('error');
        setMessage(
          err.message ||
          'This verification link is invalid or has expired.'
        );
      });
  }, [searchParams]);

  return (
    <div className={styles.container}>
      <h1>Components Order</h1>
      <h2>Email verification</h2>

      {status === 'loading' && (
        <p>Verifying your email…</p>
      )}

      {status === 'success' && (
        <>
          <div className={styles.success}>
            {message}
          </div>

          <p>
            Your account is now ready.
          </p>

          <p>
            <Link to="/login">
              Sign in to your account
            </Link>
          </p>
        </>
      )}

      {status === 'error' && (
        <>
          <div className={styles.error}>
            {message}
          </div>

          <p>
            <Link to="/login">
              Back to login
            </Link>
          </p>
        </>
      )}
    </div>
  );
}