import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5143/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: false, // CORS credentials handled via Authorization header
});

// Add token to requests
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
      console.log('Request with token to:', config.url);
    } else {
      console.warn('No token found for request to:', config.url);
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Handle 401 errors (unauthorized)
api.interceptors.response.use(
  (response) => response,
  (error) => {
    // Only log out if it's not a login/register endpoint and we get a 401
    const isAuthEndpoint = error.config?.url?.includes('/auth/login') || 
                          error.config?.url?.includes('/auth/register');
    
    if (error.response?.status === 401 && !isAuthEndpoint) {
      // Check if we have a token - if not, don't try to log out
      const token = localStorage.getItem('token');
      console.error('401 Unauthorized error:', {
        url: error.config?.url,
        status: error.response?.status,
        hasToken: !!token,
        response: error.response?.data
      });
      
      if (token) {
        console.error('Authentication failed. Token may be invalid or expired.');
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        // Only redirect if we're not already on the login page
        if (window.location.pathname !== '/login') {
          window.location.href = '/login';
        }
      }
    } else if (error.response) {
      console.error('API Error:', {
        url: error.config?.url,
        status: error.response?.status,
        data: error.response?.data
      });
    }
    return Promise.reject(error);
  }
);

export default api;

