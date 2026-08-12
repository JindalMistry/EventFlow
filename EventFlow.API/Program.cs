using EventFlow.API.Middleware;
using EventFlow.Infrastructure.Extensions;
using EventFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

// Configure HTTP Client

builder.Services.AddHttpClient(
    "EventFlowLoadTest",
    client =>
    {
        client.BaseAddress =
            new Uri(
                builder.Configuration["LoadTest:ApiBaseUrl"]!);

        client.Timeout =
            TimeSpan.FromMinutes(5);
    });

// Serilog Configuration
builder.Services.AddSerilog();
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()

        .WriteTo.Console()

        .WriteTo.File(
            Path.Combine(AppContext.BaseDirectory, "Logs", "log-.txt"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            shared: true);
});

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "EventFlow API", Version = "v1" });

    // Define the Bearer Auth Scheme
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token (e.g., 'Bearer {your_token}')"
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});

builder.Services.AddOpenApi();

// Infrastructure Dependency
builder.Services.AddInfrastructure(builder.Configuration);

// Dependency Injection
builder.Services.AddApplicationServices(builder.Configuration);

// Jwt Configuaration
builder.Services.AddJwtAuthentication(builder.Configuration);

//Rabbit MQ
builder.Services.AddRabbitMq(builder.Configuration);

//CORS
var policyName = "CORSPolicy";

//var allowedOrigins = builder.Environment.IsDevelopment()
//    ? new[]
//    {
//        "http://localhost:5173"
//    }
//    : new[]
//    {
//        "https://autohub-app-theta.vercel.app"
//    };

builder.Services.AddCors(options =>
{
    options.AddPolicy(policyName, policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});




var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(); // Access at /swagger/index.html

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor |
    ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();

app.UseCors(policyName);

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<ExceptionMiddleware>();

app.MapControllers();

app.Run();
