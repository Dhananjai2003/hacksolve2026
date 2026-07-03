using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.OpenApi;
using Seatgenie.Api.Data;
using Seatgenie.Api.Infrastructure;
using Seatgenie.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Application Insights — telemetry + logs for post-deployment analytics.
// Reads the connection string from ApplicationInsights:ConnectionString or the
// APPLICATIONINSIGHTS_CONNECTION_STRING environment variable (set in Azure App Service).
// Only registered when a connection string is present, so local/dev runs without one
// still start cleanly.
var appInsightsConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"]
    ?? builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
        options.ConnectionString = appInsightsConnectionString);
}

// EF Core (Azure Database for PostgreSQL) + repository layer.
builder.Services.AddPersistence(builder.Configuration);

// Service layer (DTO-based) over the repositories.
builder.Services.AddApplicationServices();

// Controllers. All controller routes are prefixed with "api" to match the
// documented server base path (see the OpenAPI spec's servers entry).
builder.Services
    .AddControllers(options => options.Conventions.Add(new RoutePrefixConvention("api")))
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SeatGenie API",
        Version = "1.0.0",
        Description = "SeatGenie — CGI Desk Reservation System.",
    });

    // NextAuth session cookie (set after Microsoft Entra sign-in).
    options.AddSecurityDefinition("sessionCookie", new OpenApiSecurityScheme
    {
        Name = "next-auth.session-token",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Cookie,
        Description = "NextAuth session cookie (set after Microsoft Entra sign-in).",
    });
    options.AddSecurityRequirement(_ =>
    {
        var sessionCookieRef = new OpenApiSecuritySchemeReference("sessionCookie", null, null);
        return new OpenApiSecurityRequirement { [sessionCookieRef] = new List<string>() };
    });

    // Pull in the triple-slash XML doc comments for richer Swagger output.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Map service-layer domain exceptions (conflict / unauthenticated) to the Error payload.
app.UseMiddleware<ErrorHandlingMiddleware>();

// Serve Swagger UI in every environment for this scaffold.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "SeatGenie API v1");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
