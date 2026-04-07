using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudyHub.API.Data;
using StudyHub.API.Services;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=studyhub.db"));

// Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<MaterialsService>();
builder.Services.AddScoped<OrdersService>();

// JWT Auth
var jwtKey = builder.Configuration["Jwt:Key"] ?? "supersecretkey_studyhub_2024_!@#$";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "StudyHub",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "StudyHubUsers"
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// CORS for React dev server
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.WithOrigins("http://localhost:5173")
        .AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Create DB schema automatically on first run
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    try
    {
        db.Database.ExecuteSqlRaw("ALTER TABLE Materials ADD COLUMN IsArchived INTEGER NOT NULL DEFAULT 0;");
    }
    catch
    {
        // Column already exists.
    }
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
