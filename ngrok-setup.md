# Ngrok Setup Guide

## Step 1: Sign up for ngrok (Free)
1. Go to https://dashboard.ngrok.com/signup
2. Sign up with your email (free account is sufficient)

## Step 2: Get your authtoken
1. After signing up, go to: https://dashboard.ngrok.com/get-started/your-authtoken
2. Copy your authtoken (it looks like: `2abc123def456ghi789jkl012mno345pqr678stu901vwx234`)

## Step 3: Configure ngrok
Run this command in your terminal (replace with your actual token):
```bash
ngrok config add-authtoken YOUR_AUTHTOKEN_HERE
```

## Step 4: Run ngrok
Once configured, you can run:
```bash
ngrok http 3000
```

This will give you an HTTPS URL like: `https://abc123.ngrok.io`

## Step 5: Update appsettings.json
Put that URL in your `Clean.TelegramBot/appsettings.json`:
```json
"WebApp": {
  "Url": "https://abc123.ngrok.io",
  "Enabled": true
}
```

Note: Free ngrok URLs change each time you restart it. For a stable URL, you need a paid plan, or you can use the URL each time you restart ngrok.

