# Deployment notes

This folder contains the modified files needed to prepare the project for free deployment.

Key changes:
- Frontend API base is now configurable with `VITE_API_BASE_URL`.
- Backend CORS origins are configurable with `Cors__AllowedOrigins`.
- JWT validation is enforced before user lookup.
- The `/api/auth/me` endpoint now uses authenticated claims only.
- Production JWT key is no longer stored in `appsettings.json`; it lives in environment variables or `appsettings.Development.json` for local development.

