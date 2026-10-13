// Diploma.API/Controllers/Client/ClientWarrantyController.cs

using Diploma.DataAccess;
using Diploma.DataAccess.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Diploma.API.Controllers.Client
{
    [ApiController]
    [Route("api/client/warranties")]
    [Authorize(Policy = "Authenticated")]
    public class ClientWarrantyController : ControllerBase
    {
        private readonly DiplomaDbContext _context;
        private readonly ILogger<ClientWarrantyController> _logger;

        public ClientWarrantyController(DiplomaDbContext context, ILogger<ClientWarrantyController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Получить все гарантийные талоны клиента
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<WarrantyCardDto>>> GetMyWarranties()
        {
            try
            {
                var clientIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(clientIdClaim) || !int.TryParse(clientIdClaim, out var clientId))
                    return Unauthorized(new { message = "Не удалось определить клиента" });

                _logger.LogInformation("🔍 Запрос гарантийных талонов для клиента #{ClientId}", clientId);

                var warranties = await _context.WarrantyCards
                    .Include(w => w.Request)
                        .ThenInclude(r => r!.ClientEntity)
                    .Include(w => w.Act)
                    .Where(w => w.Request!.ClientId == clientId && w.IsVisibleToClient && w.IsActive)
                    .OrderByDescending(w => w.IssuedAt)
                    .Select(w => new WarrantyCardDto
                    {
                        Id = w.Id,
                        RequestId = w.RequestId,
                        DeviceModel = w.Request!.Model,
                        SerialNumber = w.Request.SerialNumber,
                        IssuedAt = w.IssuedAt,
                        ValidUntil = w.ValidUntil,
                        FilePath = w.FilePath,
                        IsActive = w.IsActive,
                        IsExpired = w.ValidUntil < DateTime.UtcNow
                    })
                    .ToListAsync();

                _logger.LogInformation("✅ Найдено {Count} гарантийных талонов", warranties.Count);

                return Ok(warranties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка получения гарантийных талонов");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        /// <summary>
        /// Скачать гарантийный талон
        /// </summary>
        [HttpGet("{warrantyId}/download")]
        public async Task<IActionResult> DownloadWarranty(int warrantyId)
        {
            try
            {
                var clientIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(clientIdClaim) || !int.TryParse(clientIdClaim, out var clientId))
                    return Unauthorized(new { message = "Не удалось определить клиента" });

                var warranty = await _context.WarrantyCards
                    .Include(w => w.Request)
                    .FirstOrDefaultAsync(w => w.Id == warrantyId && w.Request!.ClientId == clientId);

                if (warranty == null || !warranty.IsVisibleToClient)
                    return NotFound(new { message = "Гарантийный талон не найден" });

                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", warranty.FilePath.TrimStart('/'));

                if (!System.IO.File.Exists(fullPath))
                    return NotFound(new { message = "Файл не найден" });

                return PhysicalFile(fullPath, "application/pdf", $"Warranty_{warranty.RequestId}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка скачивания гарантийного талона");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }
    }

    // DTO
    public class WarrantyCardDto
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public string DeviceModel { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public DateTime ValidUntil { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsExpired { get; set; }
    }
}