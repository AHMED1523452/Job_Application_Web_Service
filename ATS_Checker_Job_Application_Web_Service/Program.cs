
using ATS_Checker_Job_Application_Web_Service.Middleware;
using Hangfire;
using JobApplication.Application;
using JobApplication.Application.Interfaces;
using JobApplication.Infrastructure;
using JobApplication.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

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
    builder.Services.AddSwaggerGen(options =>
    {

        //. Include XML comments if the XML documentation file exists
        var xmlPath = Path.Combine(
            AppContext.BaseDirectory,
            $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml");

        //. Check if the XML documentation file exists before including it
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT bearer token."
        });

        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
    });
}

// ── 1. Clean Architecture layers ─────────────────────────────────────────────
//  AddApplication  : MediatR, FluentValidation, AutoMapper, pipeline behaviors
//  AddInfrastructure: EF Core, JWT, distributed cache, repositories and background services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ICurrentUserService, CurrentUserService>();


//. Rate Limiting configuration 
var rateLimitSettings = builder.Configuration.GetSection("RateLimiting");

//. Get the rate limiting settings from configuration
int permitLimit =Convert.ToInt32(rateLimitSettings.GetValue<int?>("PermitLimit"));
int windowSeconds =Convert.ToInt32(rateLimitSettings.GetValue<int?>("WindowSeconds"));
int queueLimit =Convert.ToInt32(rateLimitSettings.GetValue<int?>("QueueLimit"));

builder.Services.AddRateLimiter(options =>
{
    //. this rateLimiting will be general for all the API endpoints,
    //. but you can also apply rate limiting to specific endpoints using attributes or middleware.
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-client",
            factory: option => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromSeconds(windowSeconds),
                QueueLimit = queueLimit,
                AutoReplenishment = true
            }));
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        //. sending the message in json shape 
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            IsSuccess = false,
            Message = "Too many requests. Please retry later."
        }, cancellationToken);
    };
});


//. Cors --> not best practice 
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()));


var jwtSecretKey = builder.Configuration["JwtSettings:SecretKEY"];
if (string.IsNullOrWhiteSpace(jwtSecretKey))
    throw new InvalidOperationException("Configuration value 'JwtSettings:SecretKEY' is required.");

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
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
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<StopwatchMiddleware>();

app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard();

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
