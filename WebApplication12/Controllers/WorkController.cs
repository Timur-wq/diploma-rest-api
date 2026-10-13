// Diploma.API/Controllers/WorkController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Diploma.DataAccess;
using Diploma.DataAccess.Entities;

namespace Diploma.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ServiceEngineer,Dispatcher")]
    public class WorkController : ControllerBase
    {
        private readonly DiplomaDbContext _context;
        private readonly ILogger<WorkController> _logger;

        public WorkController(DiplomaDbContext context, ILogger<WorkController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Получить список всех работ
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<WorkDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<WorkDto>>> GetAllWorks()
        {
            try
            {
                var works = await _context.WorkEntities
                    .Select(w => new WorkDto
                    {
                        WorkCode = w.WorkCode,
                        WorkName = w.WorkName,
                        EstimatedCost = w.PricePerHour // Или отдельное поле стоимости
                    })
                    .OrderBy(w => w.WorkName)
                    .ToListAsync();

                return Ok(works);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка работ");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        /// <summary>
        /// Поиск работ по названию
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(List<WorkDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<WorkDto>>> SearchWorks([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return await GetAllWorks();

            try
            {
                var works = await _context.WorkEntities
                    .Where(w => w.WorkName.Contains(query))
                    .Select(w => new WorkDto
                    {
                        WorkCode = w.WorkCode,
                        WorkName = w.WorkName,
                        EstimatedCost = w.PricePerHour
                    })
                    .OrderBy(w => w.WorkName)
                    .Take(20)
                    .ToListAsync();

                return Ok(works);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске работ");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }
    }

    // DTO для ответа
    public class WorkDto
    {
        public int WorkCode { get; set; }
        public string WorkName { get; set; } = string.Empty;
        public decimal? EstimatedCost { get; set; }
    }
}