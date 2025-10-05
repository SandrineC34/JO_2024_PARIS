using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using JeuxOlympiques.Data;

var builder = WebApplication.CreateBuilder(args);

// Configuration des services
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/connexion.html";
        options.LogoutPath = "/api/Auth/logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5000", "https://localhost:5001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Enregistrer le DbSeeder comme service
builder.Services.AddScoped<DbSeeder>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// ===========================
// INITIALISATION DE LA BASE DE DONNÉES
// ===========================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Appliquer les migrations automatiquement
        logger.LogInformation("🔄 Application des migrations...");
        context.Database.Migrate();
        logger.LogInformation("✅ Migrations appliquées");
        
        // Charger les données de test depuis le JSON
        // IMPORTANT: Mettre à false en production !
        bool loadTestData = builder.Configuration.GetValue<bool>("LoadTestData", true);
        
        if (loadTestData && app.Environment.IsDevelopment())
        {
            logger.LogInformation("📋 Chargement des données de test depuis seed-data.json...");
            
            var seeder = services.GetRequiredService<DbSeeder>();
            
            // forceReseed = true pour réinitialiser à chaque démarrage
            // forceReseed = false pour garder les données existantes
            bool forceReseed = builder.Configuration.GetValue<bool>("ForceReseed", false);
            
            await seeder.SeedDataAsync(forceReseed);
            
            logger.LogInformation("✅ Données de test chargées !");
        }
        else
        {
            logger.LogInformation("ℹ️  Mode production - Pas de chargement de données de test");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Erreur lors de l'initialisation de la base de données");
        throw;
    }
}

// Configuration du pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API JO 2024 v1");
        options.RoutePrefix = "api-docs";
    });
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowFrontend");
}

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ===========================
// ENDPOINTS POUR GESTION DES DONNÉES DE TEST
// ===========================

// Endpoint pour recharger les données de test (développement uniquement)
app.MapPost("/api/dev/reset-database", async (HttpContext context, DbSeeder seeder) =>
{
    if (!app.Environment.IsDevelopment())
    {
        return Results.Forbid();
    }
    
    await seeder.SeedDataAsync(forceReseed: true);
    return Results.Ok(new { message = "Base de données réinitialisée avec les données de test" });
})
.WithTags("Development")
.WithDescription("Réinitialise la base de données avec les données du fichier JSON");

// Endpoint pour exporter les données actuelles
app.MapGet("/api/dev/export-database", async (HttpContext context, DbSeeder seeder) =>
{
    if (!app.Environment.IsDevelopment())
    {
        return Results.Forbid();
    }
    
    var exportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", $"export-{DateTime.Now:yyyyMMdd-HHmmss}.json");
    await seeder.ExportDataToJsonAsync(exportPath);
    
    return Results.Ok(new { 
        message = "Données exportées avec succès", 
        path = exportPath 
    });
})
.WithTags("Development")
.WithDescription("Exporte les données actuelles de la base vers un fichier JSON");

app.MapGet("/", () => Results.Redirect("/index.html"));
app.MapFallbackToFile("index.html");

Console.WriteLine("🏟️  Démarrage de l'application Jeux Olympiques 2024");
Console.WriteLine($"🌐 Environnement: {app.Environment.EnvironmentName}");
Console.WriteLine($"📍 URLs: {string.Join(", ", app.Urls)}");

if (app.Environment.IsDevelopment())
{
    Console.WriteLine("\n📋 Endpoints de développement disponibles:");
    Console.WriteLine("   POST /api/dev/reset-database - Réinitialiser la base");
    Console.WriteLine("   GET  /api/dev/export-database - Exporter les données");
    Console.WriteLine("   GET  /api-docs - Documentation Swagger\n");
}

app.Run();