# Components Order Platform

Platform for students to request electronic components from the engineering school technician. Orders go through: **Submitted → Approved or Rejected → Done** (when the student returns the components).

---

## What you need before starting

Install these once on your computer:

| Tool | What it does | Where to get it |
|------|----------------|------------------|
| **.NET 8 SDK** | Runs the backend API | https://dotnet.microsoft.com/download/dotnet/8.0 — choose “SDK” for your OS |
| **Node.js** (18 or newer) | Runs the frontend and installs packages | https://nodejs.org — use the LTS version |

**Check that they work:**

- Open a terminal (PowerShell or Command Prompt on Windows).
- Run: `dotnet --version` — you should see something like `8.0.x`.
- Run: `node --version` — you should see something like `v18.x` or `v20.x`.

The project uses **SQL Server LocalDB** for the database. On Windows it is usually installed with Visual Studio or the “Database” workload of the .NET SDK. If the backend later fails with a database error, see “Troubleshooting” at the bottom.

---

## How to run the project (step by step)

You will open **two terminals**: one for the backend (API), one for the frontend (website). Keep both running while you use the app.

---

### Terminal 1 — Start the backend (API)

1. Open a terminal.
2. Go to the project folder and then into the API folder:

   **Windows (PowerShell or CMD):**
   ```text
   cd C:\Users\Safwen\ComponentsOrderPlatform\src\ComponentsOrderApi
   ```

   **macOS / Linux:**
   ```text
   cd /path/to/ComponentsOrderPlatform/src/ComponentsOrderApi
   ```
   (Replace `/path/to/` with the real path to `ComponentsOrderPlatform`.)

3. Start the API:
   ```text
   dotnet run
   ```

4. Wait until you see something like:
   ```text
   Now listening on: http://localhost:5257
   ```

5. Leave this terminal open. Do not close it.

**Optional check:** In your browser, open: **http://localhost:5257/swagger**  
You should see the Swagger API documentation page.

---

### Terminal 2 — Start the frontend (website)

1. Open a **second** terminal (leave the first one running).
2. Go to the project folder and then into the client folder:

   **Windows:**
   ```text
   cd C:\Users\Safwen\ComponentsOrderPlatform\client
   ```

   **macOS / Linux:**
   ```text
   cd /path/to/ComponentsOrderPlatform/client
   ```

3. Install dependencies (only needed the first time or after pulling changes):
   ```text
   npm install
   ```

4. Start the frontend:
   ```text
   npm run dev
   ```

5. You should see something like:
   ```text
   Local:   http://localhost:5173/
   ```

6. Leave this terminal open as well.

**Open the app:** In your browser go to: **http://localhost:5173**  
You should see the login page of the Components Order app.

---

## Using the app

- **As a student:** Click “Register”, create an account, then sign in. You can create orders (component name, quantity, description) and see “My orders” and their status.
- **As a technician (admin):** There is no registration for admins. Use one of these accounts to sign in:
  - Email: **admin1@school.edu** — Password: **Admin1!**
  - Email: **technician@school.edu** — Password: **Technician1!**  

  Then you can see all orders, filter by status, and for each order: **Approve**, **Reject** (with optional reason), or **Mark as Done** when the student has returned the components.

---

## Stopping the app

- In each terminal, press **Ctrl+C** to stop the backend or the frontend.
- You can close the terminal windows after that.

---

## Troubleshooting

**“dotnet” or “node” not found**  
Install the .NET 8 SDK and Node.js as in “What you need before starting” and restart the terminal.

**Backend fails with a database or connection error**  
The app uses SQL Server LocalDB. If you don’t have it:

- **Option A:** Install “SQL Server Express LocalDB” or the “Database” workload when installing .NET/Visual Studio.
- **Option B:** Use a different database (e.g. change the connection string in `src/ComponentsOrderApi/appsettings.json` to a SQL Server instance you have installed).

**Port already in use (e.g. 5257 or 5173)**  
Another program may be using that port. Close other apps using it, or change the port in:
- Backend: `src/ComponentsOrderApi/Properties/launchSettings.json` (e.g. `applicationUrl`).
- Frontend: `client/vite.config.js` (proxy target) and Vite’s dev server port if you change it.

**Frontend shows “Loading…” or login doesn’t work**  
Make sure the **backend is running** in the first terminal and that you see “Now listening on: http://localhost:5257”. The frontend talks to the API at that address.

**I changed the project path**  
Update the `cd` paths in “Terminal 1” and “Terminal 2” to your actual `ComponentsOrderPlatform` folder (e.g. `C:\Users\YourName\ComponentsOrderPlatform` on Windows).

---

## Project layout (for reference)

```text
ComponentsOrderPlatform/
├── src/ComponentsOrderApi/   ← Backend (dotnet run)
├── client/                   ← Frontend (npm run dev)
└── README.md                 ← This file
```
