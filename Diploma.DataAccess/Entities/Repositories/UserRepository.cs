// Diploma.DataAccess/Entities.Repositories/UserRepository.cs

using Diploma.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Diploma.Core.Abstractions;

namespace Diploma.DataAccess.Entities.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DiplomaDbContext _dbContext;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(DiplomaDbContext dbContext, ILogger<UserRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        // === Сотрудники ===
        public async Task<StafEntity?> GetStaffByLoginAsync(string login)
        {
            return await _dbContext.Stafs.AsNoTracking().FirstOrDefaultAsync(u => u.User.Login == login);
        }

        public async Task<StafEntity?> GetStaffByIdAsync(int tabNum)
        {
            return await _dbContext.Stafs.AsNoTracking().FirstOrDefaultAsync(u => u.TabNum == tabNum);
        }

        // === Клиенты ===
        public async Task<ClientEntity?> GetClientByLoginAsync(string login)
        {
            return await _dbContext.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.User.Login == login);
        }

        public async Task<ClientEntity?> GetClientByIdAsync(int clientId)
        {
            return await _dbContext.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == clientId);
        }

        // === Общие методы авторизации ===
        public async Task<bool> ResetFailedLoginAttemptsAsync(int userId, bool isStaff)
        {
            //if (isStaff)
            //{
            //    var user = await _dbContext.Users.FindAsync(userId);
            //    if (user == null) return false;
            //    user.FailedLoginAttempts = 0;
            //    user.LockedUntil = null;
            //}
            //else
            //{
            //    var client = await _dbContext.Clients.FindAsync(userId);
            //    if (client == null) return false;
            //    client.FailedLoginAttempts = 0;
            //    client.LockedUntil = null;
            //}

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null) return false;
            user.FailedLoginAttempts = 0;
            user.LockedUntil = null;
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task UpdateLastLoginAsync(int userId, bool isStaff)
        {
            //if (isStaff)
            //{
            //    var user = await _dbContext.Stafs.FindAsync(userId);
            //    if (user != null) { user.LastLoginAt = DateTime.UtcNow; await _dbContext.SaveChangesAsync(); }
            //}
            //else
            //{
            //    var client = await _dbContext.Clients.FindAsync(userId);
            //    if (client != null) { client.LastLoginAt = DateTime.UtcNow; await _dbContext.SaveChangesAsync(); }
            //}
            var user = await _dbContext.Users.FindAsync(userId);
            if (user != null) { user.LastLoginAt = DateTime.UtcNow; await _dbContext.SaveChangesAsync(); }
        }

        public async Task IncrementFailedLoginAttemptsAsync(int userId, bool isStaff)
        {
            //if (isStaff)
            //{
            //    var user = await _dbContext.Stafs.FindAsync(userId);
            //    if (user != null) { user.FailedLoginAttempts++; await _dbContext.SaveChangesAsync(); }
            //}
            //else
            //{
            //    var client = await _dbContext.Clients.FindAsync(userId);
            //    if (client != null) { client.FailedLoginAttempts++; await _dbContext.SaveChangesAsync(); }
            //}
            var user = await _dbContext.Users.FindAsync(userId);
            if (user != null) { user.FailedLoginAttempts++; await _dbContext.SaveChangesAsync(); }
        }

        public async Task LockUserAsync(int userId, int minutes, bool isStaff)
        {
            //if (isStaff)
            //{
            //    var user = await _dbContext.Stafs.FindAsync(userId);
            //    if (user != null) { user.LockedUntil = DateTime.UtcNow.AddMinutes(minutes); await _dbContext.SaveChangesAsync(); }
            //}
            //else
            //{
            //    var client = await _dbContext.Clients.FindAsync(userId);
            //    if (client != null) { client.LockedUntil = DateTime.UtcNow.AddMinutes(minutes); await _dbContext.SaveChangesAsync(); }
            //}

            var user = await _dbContext.Users.FindAsync(userId);
            if (user != null) { user.LockedUntil = DateTime.UtcNow.AddMinutes(minutes); await _dbContext.SaveChangesAsync(); }

        }

        // === Refresh tokens ===
        public async Task<bool> AddRefreshTokenAsync(int userId, string userType, string token, DateTime expiresAt, string? ipAddress)
        {
            try
            {
                await _dbContext.RefreshTokens.AddAsync(new RefreshTokenEntity
                {
                    UserId = userId,
                    Token = token,
                    ExpiresAt = expiresAt,
                    CreatedAt = DateTime.UtcNow,
                    IsUsed = false,
                    IsRevoked = false,
                    CreatedByIp = ipAddress
                });
                return await _dbContext.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении refresh token");
                return false;
            }
        }

        public async Task<RefreshTokenEntity?> GetRefreshTokenAsync(string token)
        {
            return await _dbContext.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<int> RevokeAllRefreshTokensAsync(int userId, string userType)
        {
            var tokens = await _dbContext.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.User.UserType == userType && !rt.IsUsed && !rt.IsRevoked)
                .ToListAsync();

            foreach (var t in tokens) t.IsRevoked = true;
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> CleanupExpiredRefreshTokensAsync()
        {
            var expired = await _dbContext.RefreshTokens
                .Where(rt => rt.ExpiresAt < DateTime.UtcNow || rt.IsUsed || rt.IsRevoked)
                .ToListAsync();

            if (!expired.Any()) return 0;
            _dbContext.RefreshTokens.RemoveRange(expired);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> MarkRefreshTokenAsUsedAsync(int tokenId)
        {
            var rt = await _dbContext.RefreshTokens.FindAsync(tokenId);
            if (rt == null) return false;

            rt.IsUsed = true;
            return await _dbContext.SaveChangesAsync() > 0;
        }

    }
}