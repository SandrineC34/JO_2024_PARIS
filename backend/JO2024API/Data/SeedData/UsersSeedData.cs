using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using JeuxOlympiques.Models;
using JeuxOlympiques.Models.DTOs;

namespace JeuxOlympiques.Services
{
    public class UserData
    {
        // Simulation d'une base de données en mémoire (remplacez par votre contexte de base de données)
        private static List<User> _users = new List<User>();
        private static List<Order> _orders = new List<Order>();
        private static List<Ticket> _tickets = new List<Ticket>();

        // Initialisation avec des données de test
        static UserData()
        {
            InitializeTestData();
        }

        // Méthodes pour les utilisateurs
        public async Task<UserDto> GetUserByIdAsync(int userId)
        {
            var user = _users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                FullName = user.FullName,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<UserDto> GetUserByEmailAsync(string email)
        {
            var user = _users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                FullName = user.FullName,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<ApiResponseDto<UserDto>> UpdateUserAsync(int userId, UpdateUserDto updateDto)
        {
            var user = _users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return new ApiResponseDto<UserDto>
                {
                    Success = false,
                    Message = "Utilisateur non trouvé"
                };
            }

            // Vérifier si l'email n'est pas déjà utilisé par un autre utilisateur
            if (_users.Any(u => u.Email.ToLower() == updateDto.Email.ToLower() && u.Id != userId))
            {
                return new ApiResponseDto<UserDto>
                {
                    Success = false,
                    Message = "Cette adresse email est déjà utilisée"
                };
            }

            // Mettre à jour les informations
            user.FirstName = updateDto.FirstName;
            user.LastName = updateDto.LastName;
            user.Email = updateDto.Email;
            user.UpdatedAt = DateTime.Now;

            var userDto = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                FullName = user.FullName,
                CreatedAt = user.CreatedAt
            };

            return new ApiResponseDto<UserDto>
            {
                Success = true,
                Message = "Informations mises à jour avec succès",
                Data = userDto
            };
        }

        public async Task<ApiResponseDto<bool>> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = _users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Utilisateur non trouvé"
                };
            }

            // Vérifier le mot de passe actuel
            if (!VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Mot de passe actuel incorrect"
                };
            }

            // Mettre à jour le mot de passe
            user.PasswordHash = HashPassword(changePasswordDto.NewPassword);
            user.UpdatedAt = DateTime.Now;

            return new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Mot de passe changé avec succès",
                Data = true
            };
        }

        // Méthodes pour les billets
        public async Task<List<TicketDto>> GetUserTicketsAsync(int userId)
        {
            var tickets = _tickets.Where(t => t.UserId == userId).ToList();
            
            return tickets.Select(t => new TicketDto
            {
                Id = t.Id,
                TicketNumber = t.TicketNumber,
                EventName = t.EventName,
                EventDate = t.EventDate,
                Venue = t.Venue,
                Section = t.Section,
                Row = t.Row,
                Seat = t.Seat,
                Status = t.Status,
                ScannedAt = t.ScannedAt,
                QRCode = t.QRCode
            }).ToList();
        }

        // Méthodes pour les commandes
        public async Task<List<OrderDto>> GetUserOrdersAsync(int userId)
        {
            var orders = _orders.Where(o => o.UserId == userId).ToList();
            
            return orders.Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                Description = o.Description
            }).ToList();
        }

        // Export des données RGPD
        public async Task<UserDataExportDto> ExportUserDataAsync(int userId)
        {
            var user = await GetUserByIdAsync(userId);
            var tickets = await GetUserTicketsAsync(userId);
            var orders = await GetUserOrdersAsync(userId);

            return new UserDataExportDto
            {
                User = user,
                Tickets = tickets,
                Orders = orders,
                ExportDate = DateTime.Now
            };
        }

        // Suppression du compte
        public async Task<ApiResponseDto<bool>> DeleteUserAccountAsync(int userId)
        {
            var user = _users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Utilisateur non trouvé"
                };
            }

            // Supprimer l'utilisateur et ses données associées
            _users.Remove(user);
            _orders.RemoveAll(o => o.UserId == userId);
            _tickets.RemoveAll(t => t.UserId == userId);

            return new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Compte supprimé avec succès",
                Data = true
            };
        }

        // Méthodes utilitaires pour le hachage de mot de passe
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "SALT_JO2024"));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }

        // Initialisation des données de test
        private static void InitializeTestData()
        {
            // Utilisateur de test
            var testUser = new User
            {
                Id = 1,
                FirstName = "Jean",
                LastName = "Dupont",
                Email = "jean.dupont@email.com",
                PasswordHash = "hashed_password", // En réalité, il faudrait hasher le mot de passe
                CreatedAt = DateTime.Now.AddMonths(-6)
            };
            _users.Add(testUser);

            // Commandes de test
            var orders = new List<Order>
            {
                new Order
                {
                    Id = 1,
                    OrderNumber = "CMD-2024-001",
                    UserId = 1,
                    OrderDate = new DateTime(2024, 6, 15, 14, 25, 0),
                    TotalAmount = 75m,
                    Status = "Payée",
                    Description = "• Offre Solo - Natation\n• 1x billet à 75€"
                },
                new Order
                {
                    Id = 2,
                    OrderNumber = "CMD-2024-002",
                    UserId = 1,
                    OrderDate = new DateTime(2024, 6, 20, 9, 15, 0),
                    TotalAmount = 130m,
                    Status = "Utilisée",
                    Description = "• Offre Duo - Basketball\n• 2x billets à 130€"
                },
                new Order
                {
                    Id = 3,
                    OrderNumber = "CMD-2024-003",
                    UserId = 1,
                    OrderDate = new DateTime(2024, 7, 2, 16, 42, 0),
                    TotalAmount = 220m,
                    Status = "Payée",
                    Description = "• Offre Famille - Athlétisme\n• 4x billets à 220€"
                }
            };
            _orders.AddRange(orders);

            // Billets de test
            var tickets = new List<Ticket>
            {
                new Ticket
                {
                    Id = 1,
                    TicketNumber = "JO2024-NAT-001",
                    UserId = 1,
                    OrderId = 1,
                    EventName = "Natation - Finale 100m Nage Libre",
                    EventDate = new DateTime(2024, 7, 25, 20, 30, 0),
                    Venue = "Centre Aquatique - Paris La Défense",
                    Section = "A",
                    Row = "15",
                    Seat = "12",
                    Status = "Actif",
                    QRCode = "QR_CODE_DATA_1"
                },
                new Ticket
                {
                    Id = 2,
                    TicketNumber = "JO2024-BAS-002",
                    UserId = 1,
                    OrderId = 2,
                    EventName = "Basketball - Finale Hommes",
                    EventDate = new DateTime(2024, 8, 10, 21, 0, 0),
                    Venue = "Accor Arena - Bercy",
                    Section = "B",
                    Row = "8",
                    Seat = "25-26",
                    Status = "Scanné",
                    ScannedAt = new DateTime(2024, 8, 10, 19, 45, 0),
                    QRCode = "QR_CODE_DATA_2"
                },
                new Ticket
                {
                    Id = 3,
                    TicketNumber = "JO2024-ATH-003",
                    UserId = 1,
                    OrderId = 3,
                    EventName = "Athlétisme - 100m Hommes",
                    EventDate = new DateTime(2024, 8, 4, 21, 50, 0),
                    Venue = "Stade de France - Saint-Denis",
                    Section = "C",
                    Row = "20",
                    Seat = "1-4",
                    Status = "Actif",
                    QRCode = "QR_CODE_DATA_3"
                }
            };
            _tickets.AddRange(tickets);
        }
    }
}