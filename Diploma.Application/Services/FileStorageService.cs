using Diploma.Core.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.Application.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(IWebHostEnvironment env, ILogger<FileStorageService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            // Создаём папку: wwwroot/documents/{folder}
            var uploadsFolder = Path.Combine(_env.WebRootPath, "documents", folder);
            Directory.CreateDirectory(uploadsFolder);

            // Уникальное имя файла
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Сохраняем
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Возвращаем относительный путь для БД
            var relativePath = Path.Combine("documents", folder, fileName);
            _logger.LogInformation("✅ Файл сохранён: {Path}", relativePath);

            return relativePath;
        }

        public Task<byte[]> GetFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_env.WebRootPath, filePath);
            return File.ReadAllBytesAsync(fullPath);
        }

        public Task<bool> DeleteFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_env.WebRootPath, filePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}
