// Diploma.Application/Services/AuthService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Diploma.Core.Abstractions;
using Diploma.Core.Enums;
using Diploma.Core.Models;
using Diploma.Core.Settings;
using Diploma.DataAccess;
using Diploma.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using static Diploma.Core.Class1;

namespace Diploma.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly DiplomaDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;
        private readonly JwtSettings _jwtSettings;

        public AuthService(DiplomaDbContext context, IConfiguration config, ILogger<AuthService> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
            _jwtSettings = config.GetSection("JwtSettings").Get<JwtSettings>();
        }

        public async Task<AuthResponse> AuthenticateAsync(LoginRequest request, string? ipAddress)
        {
            _logger.LogInformation("🔐 Попытка входа: {Login} с IP: {Ip}", request.Login, ipAddress);

            // 🔥 Ищем пользователя в единой таблице Users
            var user = await _context.Users
                .Include(u => u.StafEntities)
                .Include(u => u.ClientEntities)
                // 🔥 БЕЗ ThenInclude — EF Core сам свяжет навигации
                .FirstOrDefaultAsync(u => u.Login == request.Login);

            if (user == null)
            {
                _logger.LogWarning("❌ Пользователь {Login} не найден", request.Login);
                throw new UnauthorizedAccessException("Неверный логин или пароль");
            }

            // 🔥 Проверяем пароль
            if (request.Password != user.Password)
            {
                _logger.LogWarning("❌ Неверный пароль для {Login}", request.Login);
                await HandleFailedLoginAsync(user);
                throw new UnauthorizedAccessException("Неверный логин или пароль");
            }

            // 🔥 Проверяем активность и блокировку
            if (!user.IsActive)
                throw new UnauthorizedAccessException("Аккаунт деактивирован");

            if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
                throw new UnauthorizedAccessException($"Аккаунт заблокирован до {user.LockedUntil}");

            // ✅ Успешный вход — сбрасываем счётчики
            await ResetFailedLoginAsync(user);
            await UpdateLastLoginAsync(user);

            // 🔥 Определяем тип пользователя и генерируем токен
            AuthResponse response;

            if (user.UserType == "staff" && user.StafEntities.Any())
            {
                var staff = user.StafEntities.First();
                response = GenerateAuthResponse(user, staff.TabNum, user.FullName, staff.Role, "Staff", ipAddress);
            }
            else if (user.UserType == "client" && user.ClientEntities.Any())
            {
                var client = user.ClientEntities.First();
                // 🔥 Берём FullName из user (он уже загружен), с запасным значением
                var fio = user.FullName ?? "Клиент";
                response = GenerateAuthResponse(user, client.Id, fio, UserRole.Client, "Client", ipAddress);
            }
            else
            {
                throw new UnauthorizedAccessException("Пользователь не связан ни с одной ролью");
            }

            _logger.LogInformation("✅ Успешный вход: {Login} ({UserType})", user.Login, user.UserType);
            return response;
        }

        // 🔐 Генерация ответа с токенами
        private AuthResponse GenerateAuthResponse(UserEntity user, int userId, string fio, UserRole role, string userType, string? ipAddress)
        {
            var accessToken = GenerateAccessToken(userId, user.Login, fio, role, userType);
            var refreshToken = GenerateRefreshToken();

            // Сохраняем refresh token
            _context.RefreshTokens.Add(new RefreshTokenEntity
            {
                UserId = user.UserId,  // 🔥 Используем UserId из UserEntity
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenLifetimeDays),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            });

            _context.SaveChanges();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Role = role.ToString(),
                Fio = fio,
                UserId = userId,
                UserType = userType,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenLifetimeMinutes),
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenLifetimeDays)
            };
        }

        // 🔐 Обработка неудачной попытки входа
        private async Task HandleFailedLoginAsync(UserEntity user)
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= 5)
            {
                user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
                _logger.LogWarning("🔒 Аккаунт {Login} заблокирован на 30 минут", user.Login);
            }

            await _context.SaveChangesAsync();
        }

        // ✅ Сброс счётчиков после успешного входа
        private async Task ResetFailedLoginAsync(UserEntity user)
        {
            user.FailedLoginAttempts = 0;
            user.LockedUntil = null;
            await _context.SaveChangesAsync();
        }

        // 🕐 Обновление времени последнего входа
        private async Task UpdateLastLoginAsync(UserEntity user)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // 🔄 Refresh token
        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, string? ipAddress)
        {
            _logger.LogInformation("🔄 Запрос обновления токена");

            // Находим токен
            var storedToken = await _context.RefreshTokens
                .Include(t => t.User)  // 🔥 Загружаем связанного пользователя
                .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.IsUsed && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow);

            if (storedToken == null || storedToken.User == null)
            {
                _logger.LogWarning("❌ Невалидный refresh token");
                throw new UnauthorizedAccessException("Невалидный токен обновления");
            }

            var user = storedToken.User;

            // Проверяем активность пользователя
            if (!user.IsActive)
                throw new UnauthorizedAccessException("Пользователь деактивирован");

            // Помечаем старый токен как использованный
            storedToken.IsUsed = true;
            await _context.SaveChangesAsync();

            // 🔥 Определяем тип пользователя через user.UserType
            string userType = user.UserType;
            int userId;
            string fio;
            UserRole role;

            if (userType == "staff")
            {
                var staff = await _context.Stafs.FirstOrDefaultAsync(s => s.UserId == user.UserId);
                if (staff == null)
                    throw new UnauthorizedAccessException("Сотрудник не найден");

                userId = staff.TabNum;
                fio = user.FullName;
                role = staff.Role;
            }
            else if (userType == "client")
            {
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == user.UserId);
                if (client == null)
                    throw new UnauthorizedAccessException("Клиент не найден");

                userId = client.Id;
                fio = user.FullName ?? "Клиент";  // 🔥 Берём из user, а не из client
                role = UserRole.Client;
            }
            else
            {
                throw new UnauthorizedAccessException("Неизвестный тип пользователя");
            }

            // Генерируем новые токены
            var newAccessToken = GenerateAccessToken(userId, user.Login, fio, role, userType);
            var newRefreshToken = GenerateRefreshToken();

            _context.RefreshTokens.Add(new RefreshTokenEntity
            {
                UserId = user.UserId,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenLifetimeDays),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            });

            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Токен обновлён для {UserType} #{UserId}", userType, userId);

            return new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Role = role.ToString(),
                Fio = fio,
                UserId = userId,
                UserType = userType,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenLifetimeMinutes),
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenLifetimeDays)
            };
        }

        // 🔑 Генерация JWT токена
        private string GenerateAccessToken(int userId, string login, string fio, UserRole role, string userType)
        {
            // 🔥 ВСЕ строковые значения с защитой от null для Claim
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, login ?? string.Empty),
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new("Fio", fio ?? string.Empty),  // 🔥 Claim не принимает null!
                new(ClaimTypes.Role, role.ToString()),
                new("UserType", userType ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenLifetimeMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public async Task<bool> RevokeTokensAsync(int userId, string userType)
        {
            _logger.LogInformation("🔒 Отзыв токенов для {UserType} #{UserId}", userType, userId);

            // Находим все активные токены пользователя
            var tokens = await _context.RefreshTokens
                .Where(t => t.UserId == userId && !t.IsUsed && !t.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();
            return tokens.Count > 0;
        }

        // 📝 Регистрация клиента
        public async Task<RegistrationResponseDto> RegisterClientAsync(CreateClientRequest request)
        {
            _logger.LogInformation("📝 Попытка регистрации клиента: Login={Login}, Phone={Phone}",
                request.Login, request.Phone);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Проверка уникальности логина
                if (await _context.Users.AnyAsync(u => u.Login == request.Login))
                {
                    await transaction.RollbackAsync();
                    return new RegistrationResponseDto
                    {
                        Success = false,
                        Message = "Логин уже занят"
                    };
                }

                // 2. Проверка уникальности телефона
                if (await _context.Clients.AnyAsync(c => c.Phone == request.Phone))
                {
                    await transaction.RollbackAsync();
                    return new RegistrationResponseDto
                    {
                        Success = false,
                        Message = "Телефон уже зарегистрирован"
                    };
                }

                // 3. Получаем следующий ID для клиента
                var nextClientId = await _context.Clients.MaxAsync(c => (int?)c.Id) ?? 0;
                nextClientId += 1;
                _logger.LogInformation($"ID КЛИЕНТА{nextClientId}");

                //получаем следующий ID для пользователя
                var nextUserId = await _context.Users.MaxAsync(c => (int?)c.UserId) ?? 0;
                nextUserId += 1;
                _logger.LogInformation($"ID ПОЛЬЗОВАТЕЛЯ {nextUserId}");

                // 4. Создаём запись в Users
                var user = new UserEntity
                {
                    UserId = nextUserId,
                    Login = request.Login.Trim().ToLower(),
                    Password = request.Password,  // В продакшене: BCrypt.HashPassword()
                    FullName = request.Fio.Trim(),
                    UserType = "client",
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // 5. Создаём запись в Clients
                var client = new ClientEntity
                {
                    Id = nextClientId,
                    Phone = request.Phone.Trim(),
                    UserId = user.UserId  // 🔥 Связь с Users
                };

                _context.Clients.Add(client);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("✅ Клиент зарегистрирован: ClientId={ClientId}, UserId={UserId}",
                    client.Id, user.UserId);

                return new RegistrationResponseDto
                {
                    Success = true,
                    Message = "Регистрация успешна. Теперь вы можете войти.",
                    ClientId = client.Id,
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "❌ Ошибка при регистрации клиента");
                return new RegistrationResponseDto
                {
                    Success = false,
                    Message = "Произошла ошибка при регистрации. Попробуйте позже."
                };
            }
        }

        public async Task<bool> IsLoginUniqueAsync(string login)
        {
            return !await _context.Users.AnyAsync(u => u.Login == login);
        }

        public async Task<bool> IsPhoneUniqueAsync(string phone)
        {
            return !await _context.Clients.AnyAsync(c => c.Phone == phone);
        }

        public Task<bool> RevokeTokensAsync(int userId)
        {
            throw new NotImplementedException();
        }
    }

    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int AccessTokenLifetimeMinutes { get; set; } = 60;
        public int RefreshTokenLifetimeDays { get; set; } = 7;
    }
}