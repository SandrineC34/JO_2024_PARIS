using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using JO2024.Infrastructure.Data;
using JO2024.Core.Interfaces;
using JO2024.Core.Services;
using JO2024.Infrastructure.Repositories;

namespace JO2024.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ============================================================
            // 🔧 Configuration des Services
            // ============================================================

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ============================================================
            // 💾 CONFIGURATION BASE DE DONNÉES
            // ============================================================

            var env = builder.Environment.EnvironmentName.ToLower();

            // === 🐋 SECTION DOCKER LOCAL ===
            var connectionStringDocker = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "Server=mysql;Database=jo2024_db;User=jo2024_user;Password=JO2024Pass123!;";
            
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connectionStringDocker, ServerVersion.AutoDetect(connectionStringDocker)));

            // ============================================================
            // 🔐 AUTHENTIFICATION JWT
            // ============================================================
            var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "MaCléParDéfautTrèsSécurisée123!");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            // ============================================================
            // 🧩 DEPENDENCY INJECTION
            // ============================================================
            
            // Repositories
            builder.Services.AddScoped<IUtilisateurRepository, UtilisateurRepository>();
            builder.Services.AddScoped<IOffreRepository, OffreRepository>();
            builder.Services.AddScoped<IBilletRepository, BilletRepository>();
            builder.Services.AddScoped<ICommandeRepository, CommandeRepository>();
            
            // Services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IBilletService, BilletService>();
            builder.Services.AddScoped<ICommandeService, CommandeService>();
            builder.Services.AddScoped<IQRCodeService, QRCodeService>();
            builder.Services.AddScoped<IOffreService, OffreService>();

            // CORS Configuration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // ============================================================
            // 🚀 PIPELINE HTTP
            // ============================================================

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // ============================================================
            // ⚙️ MIGRATIONS AUTOMATIQUES AU DÉMARRAGE
            // ============================================================
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    db.Database.Migrate();
                    Console.WriteLine("✅ Database migrations applied successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error applying migrations: {ex.Message}");
                }
            }

            // Health Check endpoint
            app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

            Console.WriteLine("🚀 JO2024 API is starting...");
            app.Run();
        }
    }
}