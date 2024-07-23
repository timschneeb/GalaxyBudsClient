Add this to your HTML <head>:

    <link rel="icon" href="/favicon.ico" sizes="any">
    <link rel="apple-touch-icon" href="/apple-touch-icon.png">

Add this to your app's manifest.json:

    ...
    {
      "icons": [
        { "src": "/favicon.ico", "type": "image/x-icon", "sizes": "16x16 32x32" },
        { "src": "/icon-192.png", "type": "image/png", "sizes": "192x192" },
        { "src": "/icon-512.png", "type": "image/png", "sizes": "512x512" },
        { "src": "/icon-192-maskable.png", "type": "image/png", "sizes": "192x192", "purpose": "maskable" },
        { "src": "/icon-512-maskable.png", "type": "image/png", "sizes": "512x512", "purpose": "maskable" }
      ]
    }
    ...
