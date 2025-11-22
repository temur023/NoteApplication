# Note Application Frontend

A modern React + TypeScript frontend for the Note Application backend.

## Features

- 🔐 Authentication (Login/Register) with JWT
- 📝 Notes management (Create, Read, Update, Delete)
- ⏰ Reminders management (Create, Read, Update, Delete)
- 🎨 Beautiful, modern UI with Tailwind CSS
- 🔒 Protected routes
- 📱 Responsive design

## Getting Started

### Prerequisites

- Node.js 18+ and npm/yarn

### Installation

1. Install dependencies:
```bash
npm install
```

2. Create a `.env` file in the frontend directory (optional):
```env
VITE_API_URL=http://localhost:5000/api
```

3. Start the development server:
```bash
npm run dev
```

The app will be available at `http://localhost:3000`

### Building for Production

```bash
npm run build
```

The built files will be in the `dist` directory.

## Project Structure

```
frontend/
├── src/
│   ├── api/           # API service layer
│   ├── components/    # Reusable components
│   ├── contexts/      # React contexts (Auth)
│   ├── pages/         # Page components
│   ├── App.tsx        # Main app component
│   └── main.tsx       # Entry point
├── public/            # Static assets
└── package.json       # Dependencies
```

## API Integration

The frontend communicates with the backend API at `http://localhost:5000/api` by default. Make sure your backend is running and CORS is configured if needed.

## Technologies Used

- React 18
- TypeScript
- Vite
- React Router
- Axios
- Tailwind CSS
- date-fns


