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
            // 👉 Active si tu déploies via Docker Desktop (MySQL)
            // <docker>
            var connectionStringDocker = builder.Configuration.GetConnectionString("MySQLConnection")
                ?? "server=mysql;port=3306;database=jo2024db;user=root;password=example;";
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(connectionStringDocker, ServerVersion.AutoDetect(connectionStringDocker)));
            // </docker>

            // === ☁️ SECTION RENDER DEPLOYMENT ===
            // 👉 Décommente si tu déploies sur Render (PostgreSQL)
            /*
            // <render>
            var connectionStringRender = Environment.GetEnvironmentVariable("DATABASE_URL");
            if (!string.IsNullOrEmpty(connectionStringRender))
            {
                var databaseUri = new Uri(connectionStringRender);
                var userInfo = databaseUri.UserInfo.Split(':');
                var connStr = $"Host={databaseUri.Host};Port={databaseUri.Port};Database={databaseUri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true;";
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(connStr));
            }
            // </render>
            */

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
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IEmailService, EmailService>();

            var app = builder.Build();

            // ============================================================
            // 🚀 PIPELINE HTTP
            // ============================================================

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // ============================================================
            // ⚙️ MIGRATIONS AUTOMATIQUES AU DÉMARRAGE
            // ============================================================
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }

            app.Run();
        }
    }
}
