# 💊 Pharmacy Inventory App

A full-stack **Single Page Application** built to manage pharmacy medicine stock and sales records. Developed as a professional assignment demonstrating full-stack development skills using a modern .NET backend and a React frontend.

---

## 👨‍💻 Author

**Prajwal HG**

---

## 📋 Project Overview

ABC Pharmacy needed a web application to:
- Track medicine inventory with stock and expiry visibility
- Allow staff to add new medicines to the system
- Record sales and automatically reduce stock
- View a complete sales history

The solution is a **React SPA** talking to an **ASP.NET Core REST API**, with all data persisted as **JSON files on the server** — no database required.

---

## ✨ Features

### Medicines
- 📋 View all medicines in a colour-coded grid
  - 🔴 **Red row** — expiry date is less than 30 days away
  - 🟡 **Yellow row** — stock quantity is less than 10 units
- 🔍 Search medicines by name (live filter)
- ➕ Add a new medicine with full client-side and server-side validation

### Sales
- 🛒 Record a sale directly from the medicines grid
- ✅ Automatic stock validation — prevents overselling with a clear error message
- 📉 Stock quantity updates instantly after a sale
- 📊 View full sales history with date, quantity, unit price and total

---

## 🛠️ Tech Stack

### Backend
| Technology | Purpose |
|---|---|
| ASP.NET Core Web API | REST API framework |
| .NET 10 / C# 14 | Runtime and language |
| Native OpenAPI (`Microsoft.AspNetCore.OpenApi`) | API spec generation |
| Scalar UI | Interactive API documentation |
| JSON file storage | Server-side data persistence |

### Frontend
| Technology | Purpose |
|---|---|
| React 19 | UI component framework |
| Vite | Build tool and dev server |
| React Router | Client-side routing |
| Bootstrap 5 | Styling and responsive layout |
| Fetch API | HTTP communication with the backend |

---

## 🏗️ Architecture

### Backend — Layered (N-Tier)

```
HTTP Request
    │
    ▼
Controller          → HTTP only (routing, status codes, model binding)
    │
    ▼
Service             → Business logic (stock validation, ID assignment, totals)
    │
    ▼
JsonFileStore<T>    → Generic thread-safe JSON file persistence
    │
    ▼
.json file on disk
```

Key design decisions:
- **Interfaces** (`IMedicineService`, `ISaleService`) decouple layers and enable unit testing
- **Singleton DI lifetime** ensures shared locks work correctly across concurrent requests
- **Entity vs DTO pattern** (`Medicine` vs `CreateMedicineRequest`) prevents over-posting
- **`TryReduceStock`** pattern returns `bool` + `out` parameters rather than throwing for expected failures

### Frontend — Component / Service Layers

```
src/
├── pages/          → Route-level screens (MedicinesPage, SalesPage)
├── components/     → Reusable UI (MedicineTable, MedicineForm, SaleForm, Navbar)
└── services/       → API calls isolated from components (medicineService, saleService)
```

Key design decisions:
- **Service layer** mirrors the backend — components never call `fetch` directly
- **`useState` + `useEffect`** for data loading with loading / error / data triad
- **Controlled forms** with client-side validation before any API call is made
- **React Router** for client-side navigation between Medicines and Sales pages

---

## 📁 Project Structure

```
pharmacy-inventory-app/
│
├── PharmacyApi/                        # ASP.NET Core Web API
│   ├── Controllers/
│   │   ├── MedicinesController.cs      # GET /api/medicines, POST /api/medicines
│   │   └── SalesController.cs          # GET /api/sales, POST /api/sales
│   ├── Services/
│   │   ├── JsonFileStore.cs            # Generic thread-safe JSON persistence
│   │   ├── MedicineService.cs          # Medicine business logic
│   │   └── SaleService.cs             # Sale business logic + stock reduction
│   ├── Models/
│   │   ├── Medicine.cs                 # Stored entity
│   │   ├── CreateMedicineRequest.cs    # Input DTO
│   │   ├── Sale.cs                     # Stored entity
│   │   └── CreateSaleRequest.cs        # Input DTO
│   ├── Data/
│   │   ├── medicines.json              # Medicine records
│   │   └── sales.json                  # Sale records
│   └── Program.cs                      # DI registration + middleware pipeline
│
└── PharmacyApp/                        # React SPA (Vite)
    └── src/
        ├── main.jsx                    # Entry point
        ├── App.jsx                     # Router setup
        ├── pages/
        │   ├── MedicinesPage.jsx       # Home screen
        │   └── SalesPage.jsx           # Sales history screen
        ├── components/
        │   ├── Navbar.jsx
        │   ├── MedicineTable.jsx       # Colour-coded grid
        │   ├── MedicineForm.jsx        # Add medicine form
        │   └── SaleForm.jsx            # Record sale form
        └── services/
            ├── medicineService.js      # Medicine API calls
            └── saleService.js          # Sale API calls
```

---

## ⚙️ API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/medicines` | List all medicines |
| `GET` | `/api/medicines?search={name}` | Search medicines by name |
| `GET` | `/api/medicines/{id}` | Get a single medicine |
| `POST` | `/api/medicines` | Add a new medicine |
| `GET` | `/api/sales` | List all sales (newest first) |
| `POST` | `/api/sales` | Record a sale (reduces stock) |

Interactive docs available at `http://localhost:5050/scalar/v1` when the API is running.

---

## 🏃 Running Locally

### Prerequisites

| Tool | Version | Download |
|---|---|---|
| .NET SDK | 10.0+ | https://dotnet.microsoft.com/download |
| Node.js | 18.0+ | https://nodejs.org |
| Git | Any | https://git-scm.com |

### 1. Clone the repository

```bash
git clone https://github.com/PrajwalHG/pharmacy-inventory-app.git
cd pharmacy-inventory-app
```

### 2. Run the API

```bash
cd PharmacyApi
dotnet restore
dotnet run
```

API is now running at **http://localhost:5050**
Scalar UI (interactive docs) at **http://localhost:5050/scalar/v1**

The `Data/` folder is seeded with sample medicines on first run.

### 3. Run the Frontend

Open a **second terminal**:

```bash
cd PharmacyApp
cp .env.example .env
npm install
npm run dev
```

Frontend is now running at **http://localhost:5173**

> Both the API and the frontend must be running at the same time.

---

## 🔍 Key Technical Concepts Demonstrated

- **Layered architecture** with clear separation of concerns
- **Dependency Injection** with correct singleton lifetime for thread-safe shared state
- **Entity vs DTO** pattern to prevent over-posting attacks
- **Thread-safe file I/O** using `lock` in a singleton service
- **RESTful API design** with correct HTTP verbs and status codes (200, 201, 400, 404)
- **Client-side routing** with React Router (no page reloads)
- **Controlled forms** with validation before API calls
- **Error handling** at both service and component layers
- **Environment-based configuration** (`.env` / `appsettings.json`)
- **CORS** configured for cross-origin frontend ↔ API communication

---

## 📄 License

This project was developed as a professional assignment. All rights reserved © Prajwal HG.
