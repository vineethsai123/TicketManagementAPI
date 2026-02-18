using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TicketManagementApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,    //tells swagger where to put the token in the request header
        Description = "Enter ONLY the JWT token (without 'Bearer ' prefix)"
    });
// We enforce this security requirement globally, so all endpoints will require a valid JWT token in the Authorization header
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register JWT Service
builder.Services.AddSingleton<IJwtService, JwtService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Register TicketService as a singleton with CSV file path
// Look for CSV file in bin directory
var csvFilePath = Path.Combine(AppContext.BaseDirectory, "tickets.csv");

// If tickets.csv doesn't exist, search for any CSV file in the bin directory
if (!File.Exists(csvFilePath))
{
    var csvFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.csv");
    if (csvFiles.Length > 0)
    {
        csvFilePath = csvFiles[0];
        Console.WriteLine($"Found CSV file: {Path.GetFileName(csvFilePath)}");
    }
}

builder.Services.AddSingleton<ITicketService>(new TicketService(csvFilePath));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
