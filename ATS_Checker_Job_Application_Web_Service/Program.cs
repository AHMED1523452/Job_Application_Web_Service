
using Hangfire;
using JobApplication.Application;
using JobApplication.Application.Interfaces;
using JobApplication.Infrastructure;
using JobApplication.Infrastructure.Services;
using ATS_Checker_Job_Application_Web_Service.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


//. (In Development only) 

// Swagger / OpenAPI: register only in Development and when not running under
// the EF CLI. When running 'dotnet ef' the command-line will contain 'ef'
// which can cause design-time host creation to load Swashbuckle types and
// fail if assembly versions don't match. Skip Swagger registration during EF runs.
var argsList = Environment.GetCommandLineArgs();
var runningEf = argsList.Any(a => a.Equals("ef", StringComparison.OrdinalIgnoreCase) || a.IndexOf("ef", StringComparison.OrdinalIgnoreCase) >= 0);
if (!runningEf && builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

// ── 1. Clean Architecture layers ─────────────────────────────────────────────
//  AddApplication  : MediatR, FluentValidation, AutoMapper, pipeline behaviors
//  AddInfrastructure: EF Core, JWT, caching, rate limiting, Stripe, AWS, repositories
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ICurrentUserService, CurrentUserService>();

//. Cors --> not best practice 
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()));


//.Default Scheme 
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKEY"])),
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var adminBootstrapper = scope.ServiceProvider.GetRequiredService<AdminAccountBootstrapper>();
    await adminBootstrapper.SeedAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ATS Checker API v1");
        c.RoutePrefix = string.Empty; // serve Swagger UI at app root
    });
}

app.UseHttpsRedirection();
app.UseMiddleware<ValidationExceptionMiddleware>();

app.UseHangfireDashboard();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Provide a CreateHostBuilder method so EF design-time tools can build a host
// and resolve AppDbContext without requiring a separate design-time factory class.
// Make it a public method on Program so EF tools can discover it.
public static partial class Program
{
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // Register application and infrastructure services so EF design-time can resolve DbContext
                services.AddApplication();
                services.AddInfrastructure(hostContext.Configuration);
                services.AddHttpContextAccessor();
                services.AddTransient<ICurrentUserService, CurrentUserService>();
            });
}
