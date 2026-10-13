// Diploma.Core/Abstractions/IUserRepository.cs

using Diploma.DataAccess.Entities;

namespace Diploma.Core.Abstractions
{
    public interface IUserRepository
    {
        // === Сотрудники (StafEntity) ===
        Task<StafEntity?> GetStaffByLoginAsync(string login);
        Task<StafEntity?> GetStaffByIdAsync(int tabNum);

        // === Клиенты (ClientEntity) ===
        Task<ClientEntity?> GetClientByLoginAsync(string login);
        Task<ClientEntity?> GetClientByIdAsync(int clientId);

        // === Общие методы авторизации ===
        Task<bool> ResetFailedLoginAttemptsAsync(int userId, bool isStaff);
        Task UpdateLastLoginAsync(int userId, bool isStaff);
        Task IncrementFailedLoginAttemptsAsync(int userId, bool isStaff);
        Task LockUserAsync(int userId, int minutes, bool isStaff);

        // === Refresh tokens (универсальные) ===
        Task<bool> AddRefreshTokenAsync(int userId, string userType, string token, DateTime expiresAt, string? ipAddress);
        Task<RefreshTokenEntity?> GetRefreshTokenAsync(string token);
        Task<bool> MarkRefreshTokenAsUsedAsync(int tokenId);
        Task<int> RevokeAllRefreshTokensAsync(int userId, string userType);
        Task<int> CleanupExpiredRefreshTokensAsync();
    }
}