using Diploma.Core.Abstractions;
using Diploma.Core.Enums;
using Diploma.Core.Models;
using Diploma.DataAccess.Entities;
using Diploma.DataAccess.Entities.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.Application.Services
{
    public class RequestAdminService : IRequestAdminService
    {
        private readonly IRequestAdminRepository _repository;
        private readonly ILogger<RequestAdminService> _logger;

        public RequestAdminService(IRequestAdminRepository repository, ILogger<RequestAdminService> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        public async Task<List<RequestAdmPanel>> GetAllRequestsAsync()
        {
            var entities = await _repository.GetAllWithDetailsAsync();
            var userIds = entities
                .Select(e => e.Id)
                .Distinct()
                .ToList();

            // 3. Загружаем данные пользователей (если есть)
            var users = userIds.Any()
                ? await _repository.GetUsersByIdsAsync(userIds)
                : new List<ClientEntity>();

            // 4. Маппим сущности в DTO
            var result = entities.Select(entity => MapToDto(entity, users)).ToList();

            _logger.LogInformation("Загружено {Count} заявок для админ-панели", result.Count);

            return result;
        }

        private RequestAdmPanel MapToDto(RequestEntity entity, IEnumerable<ClientEntity> users)
        {


            return new RequestAdmPanel
            {
                // Основные поля
                Id = entity.Id,
                ClientId = entity.ClientId,

                // Данные клиента
                ClientFio = entity.ClientEntity?.User?.FullName ?? string.Empty,
                ClientPhone = entity.ClientEntity?.Phone ?? string.Empty,

                // Статус и даты
                Status = entity.Status,
                DateOnly = entity.DateOnly,
                TimeOnly = entity.TimeOnly,

                // Данные устройства
                SvtType = entity.SvtType,
                Model = entity.Model,
                SerialNumber = entity.SerialNumber,

                // Описание и причина отклонения
                Description = entity.Description,
                RejectionReason = entity.RejectionReason,

            };
        }
    }
}
