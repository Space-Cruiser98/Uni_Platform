const API_BASE = import.meta.env.DEV ? '' : ''; // use proxy in dev

function getToken() {
  return localStorage.getItem('token');
}

export async function api(url, options = {}) {
  const token = getToken();
  const headers = {
    'Content-Type': 'application/json',
    ...options.headers,
  };
  if (token) headers['Authorization'] = `Bearer ${token}`;
  const res = await fetch(`${API_BASE}${url}`, { ...options, headers });
  const isAuthRequest = url.includes('/api/auth/');
  if (res.status === 401 && !isAuthRequest) {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    window.location.href = '/login';
    throw new Error('Unauthorized');
  }
  if (!res.ok) {
    const text = await res.text();
    let msg = text;
    try {
      const j = JSON.parse(text);
      msg = j.detail || j.message || j.title || text;
    } catch (_) {}
    throw new Error(msg || `Request failed: ${res.status}`);
  }
  if (res.status === 204) return null;
  return res.json();
}

export const auth = {
  register: (email, password, name) =>
    api('/api/auth/register', { method: 'POST', body: JSON.stringify({ email, password, name }) }),
  login: (email, password) =>
    api('/api/auth/login', { method: 'POST', body: JSON.stringify({ email, password }) }),
  me: () => api('/api/auth/me'),
};

export const orders = {
  list: (status) => api(status ? `/api/orders?status=${status}` : '/api/orders'),
  get: (id) => api(`/api/orders/${id}`),
  create: (lines) => api('/api/orders', { method: 'POST', body: JSON.stringify({ lines }) }),
  updateStatus: (id, status, reason) =>
    api(`/api/orders/${id}/status`, {
      method: 'PATCH',
      body: JSON.stringify({ status, reason: reason || null }),
    }),
};
