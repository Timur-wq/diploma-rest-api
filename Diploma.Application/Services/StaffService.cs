using Diploma.Core.Abstractions;
using Diploma.DataAccess.Entities;
using Diploma.DataAccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Diploma.Core.Class1;
using Microsoft.EntityFrameworkCore;
using Diploma.Core.Enums;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace Diploma.Application.Services
{
    public class StaffService : IStaffService
    {
        private readonly DiplomaDbContext _context;
        private readonly IPasswordHasher<UserEntity> _passwordHasher;
        private readonly ILogger<StaffService> _logger;

        public StaffService(
            DiplomaDbContext context,
            IPasswordHasher<UserEntity> passwordHasher,
            ILogger<StaffService> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<StaffDto> CreateAsync(CreateStaffDto dto)
        {
            // 1. Проверка уникальности логина
            if (await _context.Users.AnyAsync(u => u.Login == dto.Login))
                throw new InvalidOperationException("Пользователь с таким логином уже существует");
            var maxId = await _context.Users.MaxAsync(e => (int?)e.UserId) ?? 99;
            var maxId1 = maxId+1;
            // 2. Создание пользователя
            var user = new UserEntity
            {
                UserId = maxId1,
                Login = dto.Login,
                FullName = dto.FullName,
                Password = dto.Password,
                UserType = dto.Role,
                IsActive = true,
                FailedLoginAttempts = 0
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 3. Генерация табельного номера (автоинкремент от 100)
            var maxTab = await _context.Stafs.MaxAsync(e => (int?)e.TabNum) ?? 99;
            var tabNumber = maxTab + 1;

            // 4. Преобразование строки Role в enum UserRole
            if (!Enum.TryParse<UserRole>(dto.Role, ignoreCase: true, out var userRole))
            {
                userRole = UserRole.ServiceEngineer; // значение по умолчанию
            }

            // 5. Создание записи сотрудника
            var employee = new StafEntity
            {
                UserId = user.UserId,
                TabNum = tabNumber,
                DateStartJob = dto.HireDate, // 🔥 Конвертация DateTime -> DateOnly
                Role = userRole // 🔥 Используем enum вместо строки
            };

            _context.Stafs.Add(employee);
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Создан сотрудник: {Login} (Таб. №{TabNumber})", dto.Login, tabNumber);

            return new StaffDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Login = user.Login,
                Role = user.UserType,
                TabNumber = employee.TabNum,
                HireDate = employee.DateStartJob.ToDateTime(TimeOnly.MinValue), // 🔥 Конвертация DateOnly -> DateTime
                IsActive = true
            };
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.StafEntities)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return false;

            // 🔒 Мягкое удаление (деактивация)
            user.IsActive = false;

            var employee = user.StafEntities.FirstOrDefault();

            await _context.SaveChangesAsync();

            _logger.LogInformation("🗑️ Сотрудник деактивирован: UserId={UserId}", userId);
            return true;
        }

        public async Task<List<StaffDto>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.StafEntities)
                .Where(u => u.UserType != "Client" && u.UserType != null)
                .Select(u => new StaffDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Login = u.Login,
                    // 🔥 ИСПРАВЛЕНО: берем роль из StafEntity, а не из UserType
                    Role = u.StafEntities.FirstOrDefault() != null
                        ? u.StafEntities.First().Role.ToString()
                        : string.Empty,
                    TabNumber = u.StafEntities.FirstOrDefault() != null ? u.StafEntities.First().TabNum : 0,
                    HireDate = u.StafEntities.FirstOrDefault() != null ? u.StafEntities.First().DateStartJob.ToDateTime(TimeOnly.MinValue) : DateTime.MinValue,
                    IsActive = u.IsActive
                })
                .ToListAsync();
        }
    }
}