// Diploma.API/Controllers/SpareController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Diploma.DataAccess;
using Diploma.DataAccess.Entities;

namespace Diploma.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ServiceEngineer,Dispatcher,Admin")]
    public class SpareController : ControllerBase
    {
        private readonly DiplomaDbContext _context;
        private readonly ILogger<SpareController> _logger;

        public SpareController(DiplomaDbContext context, ILogger<SpareController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Получить список всех ЗИП
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<SpareDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<SpareDto>>> GetAllSpares()
        {
            try
            {
                var spares = await _context.Spares
                    .Select(s => new SpareDto
                    {
                        SpareCode = s.SpareCode,
                        SpareName = s.SpareName,
                        Amount = s.Amount,
                    })
                    .OrderBy(s => s.SpareName)
                    .ToListAsync();

                return Ok(spares);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка ЗИП");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        /// <summary>
        /// Поиск ЗИП по названию
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(List<SpareDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<SpareDto>>> SearchSpares([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return await GetAllSpares();

            try
            {
                var spares = await _context.Spares
                    .Where(s => s.SpareName.Contains(query))
                    .Select(s => new SpareDto
                    {
                        SpareCode = s.SpareCode,
                        SpareName = s.SpareName,
                        Amount = s.Amount,
                    })
                    .OrderBy(s => s.SpareName)
                    .Take(20)  // Лимит результатов
                    .ToListAsync();

                return Ok(spares);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске ЗИП");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }
    }

    // DTO для ответа
    public class SpareDto
    {
        public int SpareCode { get; set; }
        public string SpareName { get; set; } = string.Empty;
        public int Amount { get; set; }  // Количество на складе
        public string Unit { get; set; } = string.Empty;
    }
}