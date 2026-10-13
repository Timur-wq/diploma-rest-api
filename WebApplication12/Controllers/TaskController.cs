using Diploma.Core.Abstractions;
using Diploma.DataAccess;
using Diploma.DataAccess.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Diploma.Core.Class1;

namespace Diploma.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Dispatcher,Admin")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskAssignmentService _assignmentService;
        private readonly ILogger<TaskController> _logger;
        private readonly DiplomaDbContext _context;

        public TaskController(
            ITaskAssignmentService assignmentService,
            DiplomaDbContext dbContext,
            ILogger<TaskController> logger)
        {
            _assignmentService = assignmentService;
            _logger = logger;
            _context = dbContext;
        }

        /// <summary>
        /// Назначить наряд автоматически по алгоритму
        /// </summary>
        [HttpPost("assign")]
        [ProducesResponseType(typeof(AssignWorkOrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AssignWorkOrderResponse>> AssignWorkOrder(
            [FromBody] AssignWorkOrderRequest request)
        {
            if (request.WorkId <= 0 || request.RequestId <= 0)
            {
                return BadRequest(new { message = "Неверные параметры запроса" });
            }

            var result = await _assignmentService.AssignWorkOrderAsync(request);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }

        /// <summary>
        /// Получить список доступных кандидатов для ручной проверки
        /// </summary>
        [HttpGet("candidates/{workId}")]
        [ProducesResponseType(typeof(List<CandidateDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CandidateDto>>> GetCandidates(
            int workId,
            [FromQuery] string svtModel)
        {
            if (string.IsNullOrWhiteSpace(svtModel))
            {
                return BadRequest(new { message = "Укажите модель СВТ" });
            }

            var candidates = await _assignmentService.GetEligibleCandidatesAsync(workId, svtModel);
            return Ok(candidates);
        }

        /// <summary>
        /// Начать работу по наряду
        /// </summary>
        [HttpPost("{taskId}/start")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> StartWork(int taskId)
        {
            var success = await _assignmentService.StartWorkAsync(taskId);

            if (!success)
                return BadRequest(new { message = "Не удалось начать работу" });

            return Ok(new { message = "Работа начата" });
        }

        /// <summary>
        /// Завершить работу по наряду
        /// </summary>
        [HttpPost("{taskId}/complete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CompleteWork(int taskId)
        {
            var success = await _assignmentService.CompleteWorkAsync(taskId);

            if (!success)
                return BadRequest(new { message = "Не удалось завершить работу" });

            return Ok(new { message = "Работа завершена" });
        }

        /// <summary>
        /// Отменить наряд
        /// </summary>
        [HttpPost("{taskId}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelWork(
            int taskId,
            [FromQuery] string? reason = null)
        {
            var success = await _assignmentService.CancelWorkAsync(taskId, reason ?? "Отменено");

            if (!success)
                return BadRequest(new { message = "Не удалось отменить наряд" });

            return Ok(new { message = "Наряд отменён" });
        }

        /// <summary>
        /// Получить все наряды
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<TaskEntity>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<TaskEntity>>> GetAllTasks()
        {
            var tasks = await _assignmentService.GetAllTasksAsync();
            return Ok(tasks);
        }

        // ============================================
        // 🔥 ДЛЯ РЕМОНТА (НОВОЕ)
        // ============================================
        [HttpPost("assign-repair")]
        public async Task<ActionResult<AssignRepairWorkOrdersResponse>> AssignRepairWorkOrders(
            [FromBody] AssignRepairWorkOrdersRequest request)
        {
            if (request.RequestId <= 0)
            {
                return BadRequest(new { message = "Укажите RequestId" });
            }

            var result = await _assignmentService.AssignRepairWorkOrdersAsync(request);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(result);
        }

        // В TaskController
        [HttpGet("request/{requestId}/exists")]
        public async Task<ActionResult<bool>> CheckWorkOrderExists(int requestId)
        {
            var exists = await _context.TaskEntities
                .AnyAsync(t => t.RequestId == requestId &&
                              (t.Status == "assigned" || t.Status == "in_progress" || t.Status == "completed") && t.WorkId != 1);
            return Ok(exists);
        }
    }
}
