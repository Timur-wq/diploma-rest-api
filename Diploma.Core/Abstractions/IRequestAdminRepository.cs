using Diploma.Core.Enums;
using Diploma.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.Core.Abstractions
{
    public interface IRequestAdminRepository
    {
        /// <summary>
        /// Получить ВСЕ заявки с данными клиентов и сотрудников
        /// </summary>
        Task<List<RequestEntity>> GetAllWithDetailsAsync();

        /// <summary>
        /// Получить заявку по ID с подробностями
        /// </summary>
        //Task<RequestEntity?> GetByIdWithDetailsAsync(int id);

        /// <summary>
        /// Обновить статус заявки
        /// </summary>
        //Task<bool> UpdateStatusAsync(int requestId, RequestStatus newStatus, string? rejectionReason, int changedByUserId);

        /// <summary>
        /// Получить количество заявок по статусам
        /// </summary>
        //Task<Dictionary<RequestStatus, int>> GetCountByStatusAsync();

        /// <summary>
        /// Получить количество новых заявок
        /// </summary>
        //Task<int> GetNewCountAsync();

        Task<ClientEntity?> GetUserByIdAsync(int id);

        Task<List<ClientEntity?>> GetUsersByIdsAsync(List<int> ids);
    }
}
