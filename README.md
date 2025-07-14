# RFID Receiver API

A .NET 9 Web API for managing RFID-tagged items, tracking their presence and location, and providing real-time updates via SignalR.

## Features

- **RFID Item Management:** Create, update, delete, and list RFID-tagged items and their locations.
- **RFID Readings:** Accepts RFID scan data and updates item presence/location accordingly.
- **Real-Time Updates:** Uses SignalR to broadcast item status and scan events to connected clients.
- **API Key & JWT Authentication:** Supports API key protection for sensitive endpoints and JWT-based authentication for secure access.
- **EF Core + PostgreSQL:** Uses Entity Framework Core for data access and PostgreSQL as the backing database.
- **Graceful Presence Detection:** Items are marked present/absent based on scan activity and configurable grace/cleanup periods.

## Project Structure

- `Controllers/` — API endpoints for items and RFID readings.
- `Models/` — Entity models and EF Core `AppDbContext`.
- `Services/` — Business logic for items and RFID processing.
- `Hubs/` — SignalR hub for real-time communication.
- `DataTransferObjects/` — DTOs for API requests.
- `Middleware/` — Custom middleware (e.g., API key attribute).
- `Migrations/` — EF Core database migrations.
- `signalRTest.html` — Example HTML dashboard for real-time updates.

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL database

### Configuration

1. **Database Connection:**  
   Set your PostgreSQL connection string in `appsettings.Development.json` or `appsettings.json` under `ConnectionStrings:DefaultConnection`.

2. **Auth0 (Optional):**  
   Configure Auth0 domain and audience for JWT authentication.

3. **RFID Settings:**  
   Adjust grace, cleanup, and check intervals in the `Rfid` section of your config files.

### Database Setup

Run EF Core migrations to set up the database schema:

```sh
dotnet ef database update
```

### Running the API

```sh
dotnet run
```

The API will be available, if ran locally, at `http://localhost:5202`.

### API Endpoints

- `POST /api/RfidReading/addreading` — Add a new RFID scan (requires API key).
- `GET /api/Item/getallitems` — List all items with their status and location.
- `POST /api/Item/create` — Create a new item.
- `PUT /api/Item/update` — Update an item.
- `DELETE /api/Item/delete/{hexId}` — Delete an item.
- `GET /api/Arsch/arsch` — Test endpoint.
- `GET /api/Arsch/securearsch` — Authenticated test endpoint.

### Real-Time Dashboard

Open `signalRTest.html` in your browser to see live updates of item status and scan events.

## Development Notes

- **API Key:**  
  The default API key for protected endpoints is hardcoded as `"arsch"` (see `ApiKeyAttribute.cs`). Change this for production use.
- **CORS:**  
  CORS is enabled for `localhost:5500` for local dashboard testing.
- **Seeding:**  
  Default locations are seeded on startup.