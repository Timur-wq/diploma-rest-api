using Diploma.Core.Abstractions;
using Diploma.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.DataAccess.Entities.Repositories
{
    public class RequestRepository : IRequestRepository
    {
        private readonly DiplomaDbContext _dbContext;
        public RequestRepository(DiplomaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateRequest(RepairRequest repairRequest)
        {
            // 🔥 1. Очищаем входные данные в C# (до запроса к БД)
            var cleanInputPhone = new string(repairRequest.Phone.Where(c => char.IsDigit(c)).ToArray());
            var cleanInputFio = repairRequest.Fio.Trim().ToLower();

            // 🔥 2. Загружаем клиентов с пользователями (для небольшой БД — допустимо)
            var allClients = await _dbContext.Clients
                .Include(c => c.User)
                .ToListAsync();

            // 🔥 3. Фильтруем в памяти (C#) — здесь можно использовать Where(char.IsDigit)
            var client = allClients.FirstOrDefault(c =>
                c.User?.FullName?.Trim().ToLower() == cleanInputFio &&
                new string(c.Phone.Where(ch => char.IsDigit(ch)).ToArray()) == cleanInputPhone);

            // 🔥 4. Если клиент не найден — создаём нового
            if (client == null)
            {
                // Создаём запись в Users
                var user = new UserEntity
                {
                    Login = repairRequest.Phone,
                    Password = Guid.NewGuid().ToString(),  // В продакшене: хешировать!
                    FullName = repairRequest.Fio.Trim(),

                    UserType = "client",
                    IsActive = true,

                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();  // Получаем UserId

                // Создаём запись в Clients
                client = new ClientEntity
                {
                    Phone = repairRequest.Phone,
                    UserId = user.UserId,
                    User = user
                };

                _dbContext.Clients.Add(client);
                await _dbContext.SaveChangesAsync();
            }

            // 🔥 5. Создаём заявку
            var requestEntity = new RequestEntity
            {
                DateOnly = repairRequest.DateOnly,
                TimeOnly = repairRequest.TimeOnly,
                SvtType = repairRequest.SvtType,
                Model = repairRequest.Model,
                SerialNumber = repairRequest.SerialNumber,
                Description = repairRequest.Description,  // 🔥 Не забудьте описание!
                ClientId = client.Id  // ✅ Теперь client точно не null
            };

            _dbContext.Requests.Add(requestEntity);
            await _dbContext.SaveChangesAsync();

            return requestEntity.Id;
        }
    }
}
