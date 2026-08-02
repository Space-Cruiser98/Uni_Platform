const API_BASE = (import.meta.env.VITE_API_BASE_URL || '').replace(/\/$/, '');

function joinApiUrl(path) {
  if (!API_BASE) return path;
  return path.startsWith('/') ? `${API_BASE}${path}` : `${API_BASE}/${path}`;
}

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

  const res = await fetch(joinApiUrl(url), { ...options, headers });
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
  login: (email, password) => api('/api/auth/login', { method: 'POST', body: JSON.stringify({ email, password }) }),
  register: (email, password, name) => api('/api/auth/register', { method: 'POST', body: JSON.stringify({ email, password, name }) }),
  me: () => api('/api/auth/me'),
};

export const orders = {
  list: (status) => api(`/api/orders${status ? `?status=${encodeURIComponent(status)}` : ''}`),
  get: (id) => api(`/api/orders/${id}`),
  create: (payload) => api('/api/orders', { method: 'POST', body: JSON.stringify(payload) }),
  updateStatus: (id, status) => api(`/api/orders/${id}/status`, { method: 'PATCH', body: JSON.stringify({ status }) }),
};
