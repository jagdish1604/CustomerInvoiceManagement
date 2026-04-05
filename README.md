# Customer & Invoice Management System

## 🛠 Tech Stack

- .NET Core 8 Web API
- SQL Server
- React JS (Material UI)
- ASP.NET Core Identity + JWT

---

# HOW TO RUN PROJECT (STEP BY STEP)

## 1️⃣ Clone Project

```bash
git clone <your-repo-url>
```

2️⃣ Database Setup (SQL Server)
Open SQL Server
Create new database (example: CIMDatabase)

3️⃣ Run Backend (IMPORTANT)
Go to backend folder:

cd CIM/CIM.API

Update connection string in appsettings.json

"ConnectionStrings": {
"DefaultConnection": "Server=.;Database=CIMDB;Trusted_Connection=True;"
}

Run Migration (VERY IMPORTANT) : dotnet ef database update

dotnet run : dotnet run

API will run on: http://localhost:5115/swagger/index.html

Run Frontend

cd cim-frontend
npm install
npm start

Frontend will run on: http://localhost:3000

How to Use
Open browser → http://localhost:3000
Register new user
Login
Manage Customers & Invoices

Important Notes
Run migration before starting backend
Backend must run before frontend
JWT token is required for API calls

DONE

✔ Backend running
✔ Database created
✔ Frontend running
✔ Application ready to use
