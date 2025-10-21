// ============================================
// Program.cs
// JO2024_backend/Program.cs
// ============================================
using Microsoft.EntityFrameworkCore;
using JO2024.Infrastructure.Data;
using JO2024.Core.Interfaces;
using JO2024.Core.Services;
using JO2024.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// Configuration des services
// ============================================

// Configuration de la base de données MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    // doker desktop options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)))
    options.UseNpgsql(connectionString) //render
   

);


// Configuration CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Enregistrement des repositories
builder.Services.AddScoped<IOffreRepository, OffreRepository>();
builder.Services.AddScoped<IUtilisateurRepository, UtilisateurRepository>();
builder.Services.AddScoped<ICommandeRepository, CommandeRepository>();
builder.Services.AddScoped<IBilletRepository, BilletRepository>();

// Enregistrement des services
builder.Services.AddScoped<IOffreService, OffreService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IQRCodeService, QRCodeService>();
builder.Services.AddScoped<ICommandeService, CommandeService>();
builder.Services.AddScoped<IBilletService, BilletService>();

// Configuration JWT
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ============================================
// INITIALISATION DE LA BASE DE DONNÉES
// ============================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Démarrage de l'initialisation de la base de données...");
        await DbInitializer.Initialize(context, logger);
        logger.LogInformation("Initialisation de la base de données terminée avec succès");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erreur lors de l'initialisation de la base de données");
        throw; // Arrête l'application si l'init échoue
    }
}

// ============================================
// Configuration du pipeline HTTP
// ============================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Logger.LogInformation("API JO2024 démarrée sur {Urls}", app.Urls);

app.Run();