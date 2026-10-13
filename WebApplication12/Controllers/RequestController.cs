using Diploma.Application.Services;
using Diploma.Core.Abstractions;
using Diploma.Core.Models;
using Diploma.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace WebApplication12.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Client")]
    public class RequestController : ControllerBase
    {
        private readonly DiplomaDbContext _context;
        private readonly IRequestService _requestService;
        private readonly ILogger<RequestController> _logger;

        public RequestController(IRequestService requestService, ILogger<RequestController> logger, DiplomaDbContext dbContext)
        {
            _requestService = requestService;
            _logger = logger;
            _context = dbContext;
        }

        private int GetCurrentClientId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateRequest([FromBody] RepairRequest repairRequest)
        {
            try
            {
                _logger.LogInformation("Получен запрос на создание заявки: {@Request}", repairRequest);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Неверная модель: {@Errors}", ModelState.Values.SelectMany(v => v.Errors));
                    return BadRequest(ModelState);
                }

                var requestId = await _requestService.CreateRequest(repairRequest);

                _logger.LogInformation("Заявка успешно создана. ID: {Id}", requestId);
                return Ok(requestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании заявки: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    message = "Внутренняя ошибка сервера",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Получить все заявки текущего клиента
        /// </summary>
        [HttpGet("my")]
        public async Task<ActionResult<List<RequestDto>>> GetMyRequests()
        {
            var clientId = GetCurrentClientId();
            if (clientId == 0) return Unauthorized();

            var requests = await _context.Requests
                .Where(r => r.ClientId == clientId)
                .OrderByDescending(r => r.DateOnly)
                .Select(r => new RequestDto
                {
                    Id = r.Id,
                    ClientId = r.ClientId,
                    ClientFio = r.ClientEntity.User.FullName,
                    ClientPhone = r.ClientEntity.Phone,
                    Status = r.Status.ToString(),
                    DateOnly = r.DateOnly.ToString(),
                    TimeOnly = r.TimeOnly.ToString(),
                    SvtType = r.SvtType,
                    Model = r.Model,
                    SerialNumber = r.SerialNumber,
                    Description = r.Description,
                    RejectionReason = r.RejectionReason,
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpGet("client-phone/{clientId}")]
        public async Task<ActionResult<string>> GetClientPhone(int clientId)
        {
            var client = await _context.Clients
    .AsNoTracking()
    .FirstOrDefaultAsync(c => c.Id == clientId);

            if (client == null) return NotFound();

            return Ok(client.Phone ?? string.Empty);
        }
    }
    public class CancelRequestDto
    {
        public string? Reason { get; set; }
    }
    //DTO для заявки
    public class RequestDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientFio { get; set; } = string.Empty;
        public string ClientPhone { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string DateOnly { get; set; } = string.Empty;
        public string TimeOnly { get; set; } = string.Empty;
        public string SvtType { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
    }
}