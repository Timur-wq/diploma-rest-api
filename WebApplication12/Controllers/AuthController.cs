using Diploma.Core.Abstractions;
using Diploma.Core.Models;
using Diploma.DataAccess.Entities.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Diploma.Core.Class1;

namespace Diploma.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Аутентификация пользователя (вход в систему)
        /// </summary>
        /// <response code="200">Успешный вход</response>
        /// <response code="400">Неверные данные</response>
        /// <response code="401">Неверные учётные данные</response>
        /// <response code="423">Аккаунт заблокирован</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status423Locked)]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Неверные данные входа: {@Errors}", ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }

            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var response = await _authService.AuthenticateAsync(request, ipAddress);

                _logger.LogInformation("Пользователь {Login} успешно аутентифицирован", request.Login);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Ошибка аутентификации: {Message}", ex.Message);

                // Возвращаем 423 если аккаунт заблокирован
                if (ex.Message.Contains("заблокирован"))
                    return StatusCode(StatusCodes.Status423Locked, new { message = ex.Message });

                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданная ошибка при аутентификации");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Ошибка сервера" });
            }
        }

        /// <summary>
        /// Обновление access token с помощью refresh token
        /// </summary>
        /// <response code="200">Токены успешно обновлены</response>
        /// <response code="401">Невалидный refresh token</response>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var response = await _authService.RefreshTokenAsync(request.RefreshToken, ipAddress);

                _logger.LogDebug("Токены успешно обновлены для пользователя");
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Ошибка обновления токена: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданная ошибка при обновлении токена");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Ошибка сервера" });
            }
        }

        /// <summary>
        /// Выход из системы (отзыв всех токенов пользователя)
        /// </summary>
        /// <response code="204">Успешный выход</response>
        /// <response code="401">Неавторизованный запрос</response>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogWarning("Не удалось получить ID пользователя из токена");
                return Unauthorized();
            }

            try
            {
                await _authService.RevokeTokensAsync(userId);
                _logger.LogInformation("Пользователь {UserId} вышел из системы", userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при выходе пользователя {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Ошибка при выходе" });
            }
        }

        /// <summary>
        /// Получение информации о текущем авторизованном пользователе
        /// </summary>
        /// <response code="200">Информация получена</response>
        /// <response code="401">Неавторизованный запрос</response>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
        public ActionResult<UserInfoDto> GetCurrentUserInfo()
        {
            var login = User.Identity?.Name;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var fio = User.FindFirst("Fio")?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(login))
            {
                _logger.LogWarning("Не удалось получить имя пользователя из токена");
                return Unauthorized();
            }

            return Ok(new UserInfoDto
            {
                UserId = int.Parse(userId ?? "0"),
                Login = login,
                Fio = fio,
                Role = role,
                IsAuthenticated = true
            });
        }

        /// <summary>
        /// Проверка валидности токена (просто возвращает 200 если токен валиден)
        /// </summary>
        /// <response code="200">Токен валиден</response>
        /// <response code="401">Токен невалиден</response>
        [HttpGet("validate")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult ValidateToken()
        {
            return Ok(new { valid = true, message = "Токен валиден" });
        }

        [HttpPost("register")]
        [AllowAnonymous]  // 🔥 Разрешаем без авторизации
        [ProducesResponseType(typeof(RegistrationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RegistrationResponseDto>> RegisterClient(
    [FromBody] CreateClientRequest request)
        {
            _logger.LogInformation("📥 Запрос на регистрацию клиента: Login={Login}", request.Login);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new RegistrationResponseDto
                {
                    Success = false,
                    Message = string.Join("; ", errors)
                });
            }

            var result = await _authService.RegisterClientAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Проверка уникальности логина (для формы регистрации)
        /// </summary>
        [HttpGet("check-login")]
        public async Task<ActionResult<bool>> CheckLoginUnique([FromQuery] string login)
        {
            if (string.IsNullOrWhiteSpace(login))
                return BadRequest(false);

            var isUnique = await _authService.IsLoginUniqueAsync(login);
            return Ok(isUnique);
        }

        /// <summary>
        /// Проверка уникальности телефона (для формы регистрации)
        /// </summary>
        [HttpGet("check-phone")]
        public async Task<ActionResult<bool>> CheckPhoneUnique([FromQuery] string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return BadRequest(false);

            var isUnique = await _authService.IsPhoneUniqueAsync(phone);
            return Ok(isUnique);
        }
    }
}