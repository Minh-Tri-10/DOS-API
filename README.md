
# 🥤 DrinkOrder System (DOS) – PRN232 Microservices Project

![.NET 8](https://img.shields.io/badge/.NET-8.0-blueviolet)
![License](https://img.shields.io/badge/license-MIT-green)
![Status](https://img.shields.io/badge/status-Active-success)
![Architecture](https://img.shields.io/badge/architecture-Microservices-orange)

> A modularized Drink Ordering System built with **.NET 8**, using a **microservices architecture** with **Ocelot API Gateway** and an **ASP.NET MVC frontend**.

---

## 📚 Table of Contents

- [Overview](#-overview)
- [Architecture](#-architecture)
- [Tech Stack](#-tech-stack)
- [System Requirements](#-system-requirements)
- [Environment Configuration](#-environment-configuration)
- [Running the Project](#-running-the-project)
- [API Gateway (Ocelot)](#-api-gateway-ocelot)
- [Swagger Endpoints](#-swagger-endpoints)
- [Utility Tool: ConnKeySwitcher](#-utility-tool-connkeyswitcher)
- [Database Notes](#-database-notes)
- [Contributing](#-contributing)

---

## 🚀 Overview

The **DrinkOrder System (DOS)** consists of multiple independent **.NET 8 Web API microservices**, each responsible for a domain area.

These services communicate via **HTTP**, with an **Ocelot API Gateway** aggregating routes, and an **MVC frontend** that consumes all APIs.

Key Highlights:
- Fully decoupled microservices using EF Core + SQL Server
- Centralized API Gateway (Ocelot)
- MVC front-end for user interaction
- Cloud integration: EmailJS (Account), Cloudinary (Media)
- Scalable and ready for deployment in containerized environments

---

## 🧩 Architecture

### 🔹 Main Projects
| Service | Description |
|----------|--------------|
| **AccountAPI** | User authentication, registration, password reset (via EmailJS), profile management |
| **CategoriesAPI** | Product catalog and categories, integrated with Cloudinary for image hosting |
| **CartAPI** | Shopping cart operations |
| **OrderAPI** | Order creation, reporting; communicates with Account & Categories APIs |
| **PaymentAPI** | Mock payment processing and order status updates |
| **APIGateways** | Central Ocelot Gateway routing client → service |
| **MVCApplication** | ASP.NET MVC frontend, calling APIs and rendering UI |

📁 Each service has its own database and `appsettings.json` configuration.

---

## 🧱 Tech Stack

- **Framework**: .NET 8 (ASP.NET Core Web API + MVC)
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **Mapping**: AutoMapper
- **Security**: BCrypt (Password Hashing)
- **Communication**: HTTP + Ocelot Gateway
- **Cloud Services**:
  - EmailJS (Password Reset Flow)
  - Cloudinary (Media Storage)
- **Caching**: MemoryCache
- **API Docs**: Swagger / Swashbuckle

---

## 🖥️ System Requirements

- [.NET SDK 8.x](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- (Optional) GitHub Codespaces for cloud development

---

## ⚙️ Environment Configuration

### 1️⃣ Connection Strings
Each service contains a `ConnectionStrings` section in `appsettings.json`, e.g.:

```json
"ConnectionStrings": {
  "HuyConnection": "Server=Z14;Database=DOSAccountDb;User=sa;Password=123456;"
}
````

You can:

* Edit directly per service, or
* Use **ConnKeySwitcher** (see below) to apply a single connection key (e.g. `"HuyConnection"`) across all services.

---

### 2️⃣ Frontend Base URL (AccountAPI)

For password reset emails, AccountAPI uses:

```json
"Frontend": {
  "BaseUrl": "https://localhost:7223"
}
```

This must match your MVC application’s HTTPS URL.

---

### 3️⃣ EmailJS (AccountAPI)

In `AccountAPI/appsettings.json`:

```json
"Email": {
  "ServiceId": "",
  "TemplateId": "",
  "PublicKey": "",
  "AccessToken": "",
  "FromName": "DrinkOrder System"
}
```

> ⚠️ Do **not** commit real credentials. Use `UserSecrets` or environment variables.

---

### 4️⃣ Cloudinary (CategoriesAPI)

```json
"CloudinarySettings": {
  "CloudName": "",
  "ApiKey": "",
  "ApiSecret": ""
}
```

Store secrets using `dotnet user-secrets` during development.

---

## ▶️ Running the Project

### 🧭 Option 1: CLI

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build DOS.sln -c Debug
```

Then start services in separate terminals:

```bash
# API Gateway
dotnet run -p APIGateways/APIGateways.csproj

# APIs
dotnet run -p AccountAPI/AccountAPI.csproj
dotnet run -p CategoriesAPI/CategoriesAPI.csproj
dotnet run -p CartAPI/CartAPI.csproj
dotnet run -p OrderAPI/OrderAPI.csproj
dotnet run -p PaymentAPI/PaymentAPI.csproj

# Frontend MVC
dotnet run -p MVCApplication/MVCApplication.csproj
```

### 🧭 Option 2: Visual Studio

1. Open `DOS.sln`
2. Go to Solution Properties → Set **Multiple Startup Projects**
3. Start:

   * `APIGateways`
   * All APIs
   * `MVCApplication`
4. Run in `Debug` mode

---

### 🌐 Default Ports

| Service         | URL                                              |
| --------------- | ------------------------------------------------ |
| AccountAPI      | [https://localhost:7005](https://localhost:7005) |
| CategoriesAPI   | [https://localhost:7021](https://localhost:7021) |
| CartAPI         | [https://localhost:7143](https://localhost:7143) |
| OrderAPI        | [https://localhost:7269](https://localhost:7269) |
| PaymentAPI      | [https://localhost:7011](https://localhost:7011) |
| API Gateway     | [https://localhost:7001](https://localhost:7001) |
| MVC Application | [https://localhost:7223](https://localhost:7223) |

---

## 🔀 API Gateway (Ocelot)

* Config file: `APIGateways/Ocelot.json`
* Default base URL: `http://localhost:7000`

Each route maps from **upstream (client)** → **downstream (microservice)**.

> If ports change, update `Ocelot.json` accordingly.

---

## 📘 Swagger Endpoints

| Service       | Swagger URL                                                      |
| ------------- | ---------------------------------------------------------------- |
| AccountAPI    | [https://localhost:7005/swagger](https://localhost:7005/swagger) |
| CategoriesAPI | [https://localhost:7021/swagger](https://localhost:7021/swagger) |
| CartAPI       | [https://localhost:7143/swagger](https://localhost:7143/swagger) |
| OrderAPI      | [https://localhost:7269/swagger](https://localhost:7269/swagger) |
| PaymentAPI    | [https://localhost:7011/swagger](https://localhost:7011/swagger) |

> API Gateway doesn’t aggregate Swagger by default — test individual services via their URLs.

---

## 🧰 Utility Tool: ConnKeySwitcher

> Quickly sync all connection string keys across services.

**Path:** `Tools/ConnKeySwitcher`

**Usage:**

```bash
dotnet build Tools/ConnKeySwitcher/ConnKeySwitcher.csproj -c Release
```

Then run:

```
Tools/ConnKeySwitcher/bin/Release/net8.0-windows/ConnKeySwitcher.exe
```

Steps:

1. Choose repo folder
2. Select key (e.g. `HuyConnection`)
3. Click **Scan** → preview changes
4. Click **Apply** → update all `Program.cs`
   (Backup `.bak` files will be created automatically)

---

## 🗃️ Database Notes

Each microservice has its own database:

| Service       | Database     |
| ------------- | ------------ |
| AccountAPI    | DOSAccountDb |
| CartAPI       | DOSCartDb    |
| CategoriesAPI | DOSCatalogDb |
| OrderAPI      | DOSOrderDb   |
| PaymentAPI    | DOSPaymentDb |

Reference migration doc:

```
docs/database-separation.md
```

Includes scripts to migrate data from the legacy monolith (`DrinkOrderDB`).

---

## 🤝 Contributing

**Coding Rules**

* Follow existing folder structure & naming conventions
* Use AutoMapper for DTO ↔ Entity
* Keep business logic in Services, not Controllers

**Security**

* Do not commit secrets or credentials
* Use UserSecrets or environment variables

**When adding new services**

* Create new DB & context
* Add to Ocelot routes
* Include Swagger setup
* Register as startup project in solution

---

## 🧾 License

This project is open-source under the [MIT License](LICENSE).

---

### ⭐ Star this repo if you find it helpful!



