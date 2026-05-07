using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.EntityFrameworkCore;
using OOB.Data;
using OOB.Logger;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using System.IO;
using Microsoft.AspNetCore.DataProtection;


var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddFilter((category, level) =>
            category == DbLoggerCategory.Database.Command.Name
            && level == LogLevel.Information)
        .AddConsole();
});

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddXmlSerializerFormatters();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<OOBDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Set Cookie properties using CookieBuilder properties†.
builder.Services.AddAntiforgery(options =>
{
    options.FormFieldName = "__RequestVerificationToken";
    options.HeaderName = "X-XSRF-TOKEN";
    options.SuppressXFrameOptionsHeader = false;
});

var OOBCorsPolicy = "OOBCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: OOBCorsPolicy,
        policy =>
        {
            policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

// add logging
builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        );

Serilog.Debugging.SelfLog.Enable(msg => Console.Error.WriteLine(msg));
Log.Information("Serilog initialized. BaseDir: {BaseDir}, CWD: {CWD}, LogPath: {LogPath}",
    AppContext.BaseDirectory, Environment.CurrentDirectory, Path.GetFullPath("./logs/log-.txt"));

// Ensure a persistent key folder exists and configure Data Protection to use it.
//var keyFolder = Path.Combine(AppContext.BaseDirectory, "keys");
//Directory.CreateDirectory(keyFolder);
//builder.Services.AddDataProtection()
//    .PersistKeysToFileSystem(new DirectoryInfo(keyFolder))
//    .SetApplicationName("OOB");

var app = builder.Build();

 app.UseSerilogRequestLogging();

// global cors policy
app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

// add middleware
app.UseMiddleware<ErrorHandler>();

CookiePolicyOptions cookiePolicyOptions = new();
cookiePolicyOptions.HttpOnly = HttpOnlyPolicy.Always;
cookiePolicyOptions.MinimumSameSitePolicy = SameSiteMode.Lax;
cookiePolicyOptions.Secure = CookieSecurePolicy.Always;
cookiePolicyOptions.CheckConsentNeeded = context => false;
app.UseCookiePolicy(cookiePolicyOptions);

app.UseRouting();

app.UseCors(OOBCorsPolicy);

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
