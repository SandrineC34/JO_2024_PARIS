// Program.cs
using Microsoft.EntityFrameworkCore;
using JO2024API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuration de la base de données
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Utilisation de PostgreSQL (recommandé pour Render)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Alternative SQLite pour développement local
// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseSqlite(connectionString));

// Configuration de l'authentification JWT (si nécessaire)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration.GetSection("AppSettings:Token").Value)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// Ajout des services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration CORS pour le frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

var app = builder.Build();

// Initialisation de la base de données
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Appliquer les migrations automatiquement
    try
    {
        context.Database.Migrate();
        Console.WriteLine("Migrations appliquées avec succès.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erreur lors des migrations : {ex.Message}");
    }
}

// Configuration du pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Servir les fichiers statiques (HTML, CSS, JS)
app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Route par défaut pour servir index.html
app.MapFallbackToFile("index.html");

app.Run();