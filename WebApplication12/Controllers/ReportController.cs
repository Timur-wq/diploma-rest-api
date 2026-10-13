using Diploma.Core.Abstractions;
using Diploma.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Diploma.API.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Accountant,Dispatcher")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly IWebHostEnvironment _env;
        private readonly DiplomaDbContext _context;
        private readonly ILogger<ReportController> _logger;

        public ReportController(IReportService reportService, IWebHostEnvironment env, DiplomaDbContext dbContext, ILogger<ReportController> logger)
        {
            _reportService = reportService;
            _env = env;
            _context = dbContext;
            _logger = logger;
        }

        // Генерация отчёта
        // Diploma.API/Controllers/ReportController.cs
        [HttpPost("generate/{requestId}")]
        public async Task<IActionResult> GenerateReport(int requestId)
        {
            try
            {
                _logger.LogInformation(" Запрос генерации отчёта для заявки #{RequestId}", requestId);

                // Получаем ID диспетчера из токена
                var tabNum = GetCurrentTabNumber();
                _logger.LogInformation("👤 Табельный номер диспетчера: {TabNum}", tabNum);

                if (tabNum == 0)
                {
                    _logger.LogWarning("⚠️ Не удалось получить табельный номер диспетчера");
                    return BadRequest(new { message = "Не удалось определить диспетчера" });
                }

                int reportCode = await _reportService.GenerateAccountingReportAsync(requestId, tabNum);

                _logger.LogInformation("✅ Отчёт сформирован с кодом #{ReportCode}", reportCode);

                return Ok(new
                {
                    message = "Отчёт сформирован",
                    reportCode = reportCode
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка генерации отчёта для заявки #{RequestId}", requestId);
                return BadRequest(new { message = ex.Message });
            }
        }

        private int GetCurrentTabNumber()
        {
            var tabClaim = User.FindFirst("tabNumber")
                ?? User.FindFirst("TabNum")
                ?? User.FindFirst("tab_num")  // 🔥 Добавьте этот вариант
                ?? User.FindFirst(ClaimTypes.NameIdentifier);

            if (tabClaim != null && int.TryParse(tabClaim.Value, out var tabNumber))
                return tabNumber;

            return 0;
        }


        // Скачивание отчёта диспетчером
        [HttpGet("{reportCode}/file")]
        public IActionResult GetReportFile(int reportCode)
        {
            var report = _context.ReportEntities.FirstOrDefault(r => r.ReportCode == reportCode);
            if (report == null) return NotFound();

            string fullPath = Path.Combine(_env.WebRootPath, report.FilePath);
            if (!System.IO.File.Exists(fullPath)) return NotFound("Файл не найден");

            var stream = new FileStream(fullPath, FileMode.Open);
            return File(stream, "application/pdf", $"Report_{reportCode}.pdf");
        }

        // Получить все отчёты (для бухгалтера)
        [HttpGet]
        
        public async Task<ActionResult<List<ReportDto>>> GetAllReports()
        {
            try
            {
                var reports = await _context.ReportEntities
                    .Include(r => r.Request)
                    .ThenInclude(req => req.ClientEntity)
                    .ThenInclude(c => c.User)
                    .OrderByDescending(r => r.GeneratedAt)
                    .Select(r => new ReportDto
                    {
                        ReportCode = r.ReportCode,
                        RequestId = r.RequestId,
                        ClientFio = r.Request.ClientEntity.User.FullName,
                        GeneratedAt = r.GeneratedAt,
                        FilePath = r.FilePath
                    })
                    .ToListAsync();

                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка отчётов");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        // DTO для ответа
        public class ReportDto
        {
            public int ReportCode { get; set; }
            public int RequestId { get; set; }
            public string ClientFio { get; set; }
            public DateTime GeneratedAt { get; set; }
            public string FilePath { get; set; }
        }

        // ... helper GetCurrentTabNumber ...
    }
}
