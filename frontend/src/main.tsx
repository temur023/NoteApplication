import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.tsx'
import './index.css'
import { initializeTelegramWebApp } from './utils/telegram'

// Initialize Telegram Web App if running inside Telegram
if (typeof window !== 'undefined') {
  initializeTelegramWebApp();
}

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
)


