using Diploma.Core.Abstractions;
using Diploma.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Diploma.Core.Class1;

namespace Diploma.API.Controllers.Dispatcher
{
    [ApiController]
    [Route("api/dispatcher/[controller]")]
    [Authorize(Roles = "Dispatcher")]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;
        private readonly ILogger<StaffController> _logger;

        public StaffController(IStaffService staffService, ILogger<StaffController> logger)
        {
            _staffService = staffService;
            _logger = logger;
        }

        /// <summary>
        /// Получить список всех сотрудников
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<StaffDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var staff = await _staffService.GetAllAsync();
            return Ok(staff);
        }

        /// <summary>
        /// Добавить нового сотрудника
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(StaffDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateStaffDto dto)
        {
            try
            {
                // Валидация роли (только внутренние сотрудники)
                var allowedRoles = new[] { "Dispatcher", "ServiceEngineer", "Accountant", "OmtEmployee" };
                if (!allowedRoles.Contains(dto.Role))
                    return BadRequest(new { message = "Недопустимая роль сотрудника" });

                var result = await _staffService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetAll), new { id = result.UserId }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания сотрудника");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Удалить (деактивировать) сотрудника
        /// </summary>
        [HttpDelete("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int userId)
        {
            var success = await _staffService.DeleteAsync(userId);
            if (!success) return NotFound(new { message = "Сотрудник не найден" });

            return Ok(new { message = "Сотрудник успешно деактивирован" });
        }
    }
}