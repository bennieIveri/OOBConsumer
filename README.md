# OOB Consumer

## Purpose

OOB Consumer is a project to catch, save and manage all OOB messages for Iveri to be managed by client.

## Requirements

### Runtime requirements

- .NET 10 SDK or Runtime
- SQL Server accessible from the application host
  - LocalDB
  - SQL Server Express
  - Full SQL Server

Check your installed SDK version:

`dotnet --version`

### Development requirements

- .NET 10 SDK
- Visual Studio, VS Code, or another .NET-compatible IDE
- Optional: EF Core CLI tools for database migrations

## Project configuration

The application reads its SQL Server connection string from:

`ConnectionStrings:DefaultConnection`

Default configuration file location:

`OOB/appsettings.json`

Do not store production secrets in source control. For development, prefer User Secrets. For hosted environments, prefer environment variables or a secure secret store.

## Development secrets

Initialize User Secrets for the project if needed:

`dotnet user-secrets init --project OOB/OOB.csproj`

Set the connection string:

`dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your-connection-string>" --project OOB/OOB.csproj`

Example environment variable name for hosted environments:

`ConnectionStrings__DefaultConnection`

## Database initialization

You can initialize the database using either Entity Framework Core migrations or the supplied SQL script.

### Option 1: Entity Framework Core migrations

Install or update the EF CLI tool if needed:

`dotnet tool install --global dotnet-ef`

or

`dotnet tool update --global dotnet-ef`

If migrations fail because the design package is missing, add it to the project:

`dotnet add OOB/OOB.csproj package Microsoft.EntityFrameworkCore.Design`

Create the initial migration from the repository root:

`dotnet ef migrations add InitialCreate --project OOB/OOB.csproj --startup-project OOB/OOB.csproj --context OOB.Data.OOBDbContext`

Apply migrations:

`dotnet ef database update --project OOB/OOB.csproj --startup-project OOB/OOB.csproj --context OOB.Data.OOBDbContext`

#### Runtime creation alternative

If you do not want to use CLI migrations, you can initialize the database at startup in `Program.cs` using:

`context.Database.Migrate();`

or

`context.Database.EnsureCreated();`

Use these carefully:

- `Migrate()` applies pending migrations
- `EnsureCreated()` is not compatible with migrations-based lifecycle management

### Option 2: SQL script

Run the supplied SQL script using SSMS, Azure Data Studio, `sqlcmd`, or another SQL Server client:

[Attached Sql Script (init_db.sql)](OOB/init_db.sql)

## Optional database configuration

To add token validation settings, insert an application key and secret into the database.

Example:

```sql
INSERT INTO [dbo].[ApplicationConfig]
           ([AC_ApplicationID]
           ,[AC_KeyName]
           ,[AC_Key]
           ,[AC_Value])
     VALUES
           ('<application GUID>'
           ,'APIKey.Secret'
           ,'<configured API key>'
           ,'<configured API secret>');
```

## Run locally

From the repository root:

`dotnet run --project OOB/OOB.csproj`

By default, the application will use the configured connection string and launch using the project’s ASP.NET Core settings.

## Build

Create a release build from the repository root:

`dotnet build OOB/OOB.csproj -c Release`

Publish deployable output:

`dotnet publish OOB/OOB.csproj -c Release -o ./publish`

## Hosting

### Host on IIS

#### Prerequisites

- Windows Server or Windows with IIS installed
- ASP.NET Core Hosting Bundle matching the application runtime
- SQL Server reachable from the host
- A valid production connection string

#### Publish the application

From the repository root:

`dotnet publish OOB/OOB.csproj -c Release -o ./publish`

Copy the contents of the `publish` folder to the server, for example:

`C:\inetpub\OOBConsumer`

#### Configure the connection string

Use one of the following:

- `appsettings.Production.json`
- environment variables
- IIS-configured environment variables

Environment variable name:

`ConnectionStrings__DefaultConnection`

#### Create the IIS site

1. Open **IIS Manager**
2. Create an **Application Pool**
   - **.NET CLR version**: `No Managed Code`
   - **Managed pipeline mode**: `Integrated`
3. Create a new **Website** or **Application**
4. Point it to the published folder
5. Assign the application pool you created

#### Permissions

Grant the IIS application pool identity the required access to the deployment folder.

Example identity:

`IIS AppPool\<YourAppPoolName>`

Typical permissions:
- Read & execute
- Write only if the application writes local logs or files

Also ensure the SQL Server login or integrated identity has the required database permissions.

#### web.config

The publish step usually generates `web.config` automatically. A typical file looks like:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <aspNetCore processPath="dotnet" arguments=".\OOB.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="inprocess" />
  </system.webServer>
</configuration>
```

If you enable stdout logging, create the `logs` directory and grant write access to the application pool identity.

#### Verify deployment

- Start the IIS site
- Browse to the configured URL
- Confirm database connectivity
- If startup fails, check:
  - Windows Event Viewer
  - IIS logs
  - application stdout logs if enabled

#### IIS troubleshooting

- **HTTP Error 500.30**: application startup failure
- **HTTP Error 502.5**: process startup failure, often Hosting Bundle related
- **Connection string errors**: verify SQL host, credentials, firewall, and certificate settings
- **403 or file access errors**: verify NTFS permissions

### Host on Linux with Nginx or Apache

#### Prerequisites

- Linux server
- .NET 10 runtime installed
- Nginx or Apache configured as reverse proxy
- `systemd` available
- SQL Server reachable from the host

#### Publish the application

From the repository root:

`dotnet publish OOB/OOB.csproj -c Release -o ./publish`

Copy the published output to the server, for example:

`/var/www/oob-consumer`

#### Configure the connection string

Recommended options:
- `appsettings.Production.json`
- environment variables
- `systemd` service environment values

Environment variable name:

`ConnectionStrings__DefaultConnection`

#### Create a systemd service

Create:

`/etc/systemd/system/oob-consumer.service`

Example:

```ini
[Unit]
Description=OOB Consumer
After=network.target

[Service]
WorkingDirectory=/var/www/oob-consumer
ExecStart=/usr/bin/dotnet /var/www/oob-consumer/OOB.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=oob-consumer
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5000
Environment=ConnectionStrings__DefaultConnection=Server=your-server;Database=OOBResponse;User Id=your-user;Password=your-password;TrustServerCertificate=True;

[Install]
WantedBy=multi-user.target
```

Enable and start the service:

`sudo systemctl daemon-reload`

`sudo systemctl enable oob-consumer`

`sudo systemctl start oob-consumer`

Check service status:

`sudo systemctl status oob-consumer`

View logs:

`journalctl -u oob-consumer -f`

#### Nginx configuration

Create:

`/etc/nginx/sites-available/oob-consumer`

Example:

```nginx
server {
    listen 80;
    server_name your-domain-or-server-ip;

    location / {
        proxy_pass         http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

Enable and reload:

`sudo ln -s /etc/nginx/sites-available/oob-consumer /etc/nginx/sites-enabled/`

`sudo nginx -t`

`sudo systemctl reload nginx`

#### Apache configuration

Enable required modules:

`sudo a2enmod proxy`

`sudo a2enmod proxy_http`

Create:

`/etc/apache2/sites-available/oob-consumer.conf`

Example:

```apache
<VirtualHost *:80>
    ServerName your-domain-or-server-ip

    ProxyPreserveHost On
    ProxyPass / http://127.0.0.1:5000/
    ProxyPassReverse / http://127.0.0.1:5000/

    ErrorLog ${APACHE_LOG_DIR}/oob-consumer-error.log
    CustomLog ${APACHE_LOG_DIR}/oob-consumer-access.log combined
</VirtualHost>
```

Enable and reload:

`sudo a2ensite oob-consumer.conf`

`sudo systemctl reload apache2`

#### Linux troubleshooting

- **502 Bad Gateway**: application is not running or reverse proxy points to the wrong port
- **Service start failure**: inspect `journalctl -u oob-consumer -f`
- **Port conflicts**: change `ASPNETCORE_URLS`
- **Database connectivity issues**: verify DNS, firewall, credentials, and SQL Server accessibility
- **Permission issues**: ensure the service account can read the deployed files
- **SELinux/AppArmor restrictions**: review policy settings if connectivity is blocked

## Troubleshooting

- If EF Core commands fail, verify the DbContext name:
  - `OOB.Data.OOBDbContext`
- If the CLI cannot find the startup project, ensure:
  - the `OOB` project builds successfully
  - commands are run from the repository root
  - or explicit `.csproj` paths are used
- If the app starts but cannot connect to SQL Server:
  - verify the connection string
  - verify SQL Server is reachable from the host
  - verify credentials and permissions

## Security notes

- Do not commit real production connection strings
- Use environment variables, user secrets, or a secrets manager
- Grant only the minimum required SQL permissions
- Protect API keys and secrets stored in `ApplicationConfig`
- Prefer HTTPS in all hosted environments

## Technology notes

- Target framework: .NET 10
- Database provider: SQL Server
- ORM: Entity Framework Core
- Default DbContext: `OOB.Data.OOBDbContext`