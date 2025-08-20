# SWD-BE

## Prerequisites
- .NET 8 SDK

## Run the API
1. Restore dependencies and run:
   ```bash
   dotnet run --project BookLibwithSub.API --urls=http://0.0.0.0:8080
   ```
2. Visit `http://localhost:8080` to confirm the API is running.
3. Swagger UI is available at `http://localhost:8080/swagger`.

## Entity Framework Migrations (PostgreSQL)
- Create a migration:
  ```bash
  dotnet ef migrations add <MigrationName> -p BookLibwithSub.Repo -s BookLibwithSub.API
  ```
- Apply migrations to the database:
  ```bash
  dotnet ef database update -p BookLibwithSub.Repo -s BookLibwithSub.API
  ```
