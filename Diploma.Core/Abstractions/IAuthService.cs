using Diploma.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Diploma.Core.Class1;

namespace Diploma.Core.Abstractions
{
    public interface IAuthService
    {
        Task<AuthResponse> AuthenticateAsync(LoginRequest request, string? ipAddress);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken, string? ipAddress);
        Task<bool> RevokeTokensAsync(int userId);

        Task<RegistrationResponseDto> RegisterClientAsync(CreateClientRequest request);
        Task<bool> IsLoginUniqueAsync(string login);
        Task<bool> IsPhoneUniqueAsync(string phone);
    }
}
