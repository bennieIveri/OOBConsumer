OOB Consumer - README

Purpose

This README explains how to initialize the database and run the OOB Consumer application.

Prerequisites

- .NET 10 SDK installed (`dotnet --version` should show a 10.x version)
- SQL Server accessible for the application (LocalDB, SQL Express, or full SQL Server)

Configuration

- The app reads the connection string from `OOB/appsettings.json` under the key `ConnectionStrings:DefaultConnection`.
- Example connection string already present in `OOB/appsettings.json`. Replace it with your environment's values or keep secrets out of source control by using User Secrets or environment variables.

Using User Secrets (recommended for development)

From the repository root run:

`dotnet user-secrets init --project OOB/OOB.csproj` (if not already initialized)

`dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your-connection-string>" --project OOB/OOB.csproj`

EF Core tooling (migrations)

1. Install or update the EF CLI tool (if needed):

`dotnet tool install --global dotnet-ef` or `dotnet tool update --global dotnet-ef`

2. Ensure the design package is available in the project. If migrations commands fail, add it to the project:

`dotnet add OOB/OOB.csproj package Microsoft.EntityFrameworkCore.Design`

3. Create an initial migration (run from repository root):

`dotnet ef migrations add InitialCreate --project OOB/OOB.csproj --startup-project OOB/OOB.csproj --context OOB.Data.OOBDbContext`

4. Apply migrations to create/update the database:

`dotnet ef database update --project OOB/OOB.csproj --startup-project OOB/OOB.csproj --context OOB.Data.OOBDbContext`

Alternate: Create database at runtime

If you prefer not to use migrations you can configure the app to create or migrate the database at startup. Typically this is done in `Program.cs` by resolving the `OOBDbContext` and calling:

`context.Database.Migrate();` or `context.Database.EnsureCreated();`

Be sure to understand the implications: `EnsureCreated` is not compatible with migrations, and `Migrate` will apply all pending migrations.

Run the app

Start the web app from the repo root:

`dotnet run --project OOB/OOB.csproj`

Troubleshooting

- If migrations fail due to missing types/namespaces, confirm the `--context` fully-qualified name is correct. The default DbContext lives in `OOB/Data` and is named `OOBDbContext` in this project.
- If the EF CLI cannot find the startup project, ensure the `OOB` project builds and that you run the command from the repository root or pass full paths to the `.csproj` files.

Notes

- Keep sensitive connection strings out of source control. Use environment variables, managed identities, or secrets stores for production.
- This project targets .NET 10 and uses SQL Server via `Microsoft.EntityFrameworkCore.SqlServer`.
