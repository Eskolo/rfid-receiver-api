# Docker Setup for RFID Receiver API

This repository contains a dockerized ASP.NET Core API with PostgreSQL database support.

## Prerequisites

- Docker and Docker Compose installed on your system
- Auth0 domain and audience (for authentication)
- RFID API key (for RFID endpoint security)

## Quick Start

1. **Clone the repository and navigate to the project directory**

2. **Set up environment variables**
   ```bash
   cp .env.example .env
   ```
   Edit the `.env` file and add your configuration:
   ```
   AUTH0_DOMAIN=your-auth0-domain.auth0.com
   AUTH0_AUDIENCE=your-auth0-audience
   RFID_API_KEY=your-rfid-api-key
   ```

3. **Build and run the containers**
   ```bash
   docker-compose up --build
   ```

4. **The API will be available at:**
   - API: http://localhost:8080
   - PostgreSQL: localhost:5432

## Services

### PostgreSQL Database
- **Container Name**: `rfid-postgres`
- **Port**: 5432
- **Database**: `rfiddb`
- **Username**: `rfiduser`
- **Password**: `rfidpassword`
- **Data Persistence**: Uses Docker volume `postgres_data`

### Database Migration Service
- **Container Name**: `rfid-migration`
- **Purpose**: Runs Entity Framework migrations to initialize database schema
- **Runs**: Once on startup, then exits
- **Dependencies**: PostgreSQL must be healthy

### RFID API
- **Container Name**: `rfid-api`
- **Port**: 8080
- **Environment**: Production
- **Database**: Connects to PostgreSQL container
- **Dependencies**: PostgreSQL healthy + Migrations completed

## Useful Commands

### Start services in background
```bash
docker-compose up -d
```

### Stop services
```bash
docker-compose down
```

### View logs
```bash
# All services
docker-compose logs

# Specific service
docker-compose logs rfid-api
docker-compose logs postgres
```

### Rebuild and restart
```bash
docker-compose down
docker-compose up --build
```

### Access PostgreSQL database
```bash
docker exec -it rfid-postgres psql -U rfiduser -d rfiddb
```

### Run database migrations
Database migrations are automatically handled by the `rfid-migration` service on startup. If you need to run migrations manually:

```bash
# Check migration service logs
docker-compose logs rfid-migration

# Or run migrations manually in a new container
docker-compose run --rm rfid-migration
```

## Development

For development, you may want to:

1. **Use a different Docker Compose file for development**
2. **Mount source code as volumes for hot reloading**
3. **Use Development environment settings**

## Environment Variables

The following environment variables can be configured:

- `ASPNETCORE_ENVIRONMENT`: Application environment (Development/Production)
- `ASPNETCORE_URLS`: URLs the application listens on
- `ConnectionStrings__DefaultConnection`: Database connection string
- `Auth0__Domain`: Auth0 domain for authentication
- `Auth0__Audience`: Auth0 audience for authentication
- `ApiKeys__RFID`: Accepted API key for RFID endpoints
- `Rfid__GraceSeconds`: Grace period before toggling item presence status (default: 3)
  - When an RFID tag stops being detected, the system waits this many seconds before marking the item as absent/present
  - Prevents rapid state changes due to temporary signal loss or reader interference
  - Used by RfidMovementMonitor to determine when a tag has "disappeared" long enough to trigger a status change
- `Rfid__CleanupMinutes`: Memory cleanup interval for stale tag tracking data (default: 5)
  - Tags that haven't been seen for this duration are removed from memory to prevent memory leaks
  - Only affects internal tracking, does not change database item status
  - Balances memory usage with the ability to track intermittent tag readings
- `Rfid__CheckMs`: Background monitoring interval in milliseconds (default: 1000)
  - How often the RfidMovementMonitor checks for tags that should have their status toggled
  - Lower values = faster response to tag disappearance, but higher CPU usage
  - Higher values = slower response but better performance

## Troubleshooting

### Database Connection Issues
- Ensure PostgreSQL container is healthy: `docker-compose ps`
- Check database logs: `docker-compose logs postgres`

### Migration Issues
- Check migration logs: `docker-compose logs rfid-migration`
- If migrations fail, you can retry: `docker-compose up rfid-migration`
- For clean database start: `docker-compose down -v` (WARNING: This deletes all data)

### API Not Starting
- Check API logs: `docker-compose logs rfid-api`
- Verify environment variables are set correctly
- Ensure migrations completed successfully

### Port Conflicts
- If ports 8080 or 5432 are already in use, modify the port mappings in `docker-compose.yml`

### Startup Order Issues
The services start in this order:
1. PostgreSQL starts and becomes healthy
2. Migration service runs EF migrations
3. API starts only after migrations complete successfully
