using Diploma.Core.Abstractions;
using Diploma.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.DataAccess.Entities.Repositories
{
    public class RequestAdminRepository : IRequestAdminRepository
    {
        private readonly DiplomaDbContext _dbContext;
        private readonly ILogger<RequestAdminRepository> _logger;

        public RequestAdminRepository(DiplomaDbContext dbContext, ILogger<RequestAdminRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<RequestEntity>> GetAllWithDetailsAsync()
        {
            _logger.LogDebug("Загрузка всех заявок с деталями");

            return await _dbContext.Requests
                .Include(r => r.ClientEntity)           // Загружаем клиента
                    .ThenInclude(c => c.User)           // И его пользователя
                .OrderByDescending(r => r.DateOnly)
                .ThenByDescending(r => r.TimeOnly)
                .ToListAsync();
        }

        public async Task<ClientEntity?> GetUserByIdAsync(int id)
        {
            return await _dbContext.Clients
                .Include(c => c.User)                   // Загружаем пользователя
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<ClientEntity?>> GetUsersByIdsAsync(List<int> ids)
        {
            return await _dbContext.Clients
                .Include(c => c.User)                   // Загружаем пользователя
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();
        }
    }
}