using Diploma.Core.Abstractions;
using Diploma.Core.Enums;
using Diploma.Core.Models;
using Diploma.DataAccess;
using Diploma.DataAccess.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Diploma.Core.Class1;

namespace Diploma.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly DiplomaDbContext _context;
        private readonly IPdfGenerationService _pdfService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ReportService> _logger;

        public ReportService(
            DiplomaDbContext context,
            IPdfGenerationService pdfService,
            IWebHostEnvironment env,
            ILogger<ReportService> logger)
        {
            _context = context;
            _pdfService = pdfService;
            _env = env;
            _logger = logger;
        }

        // Diploma.Application/Services/ReportService.cs
        public async Task<int> GenerateAccountingReportAsync(int requestId, int dispatcherTabNum)
        {
            _logger.LogInformation("📝 Генерация отчёта для заявки #{RequestId}, диспетчер #{DispatcherTabNum}",
                requestId, dispatcherTabNum);

            // 1. Получаем данные заявки
            var request = await _context.Requests
                .Include(r => r.ClientEntity)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(r => r.Id == requestId)
                ?? throw new Exception("Заявка не найдена");

            if (request.Status != RequestStatus.Completed)
                throw new Exception("Отчёт можно сформировать только для завершённой заявки");

            // 2. Получаем ФИО диспетчера
            var dispatcher = await _context.Stafs
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.TabNum == dispatcherTabNum)
                ?? throw new Exception($"Диспетчер с табельным номером {dispatcherTabNum} не найден");

            _logger.LogInformation("✅ Диспетчер: {DispatcherFio}", dispatcher.User?.FullName);

            // 3. Получаем список инженеров
            var engineers = await _context.TaskEntities
                .Include(t => t.StafEntity)
                .ThenInclude(s => s!.User)
                .Where(t => t.RequestId == requestId)
                .Select(t => t.StafEntity!.User!.FullName)
                .Distinct()
                .ToListAsync();

            // 4. Получаем выполненные работы
            var diagnosticAct = await _context.DiagnosticActs
                .Include(a => a.Works)
                .ThenInclude(w => w.Work)
                .FirstOrDefaultAsync(a => a.Task!.RequestId == requestId);

            var works = diagnosticAct?.Works
                .Where(w => w.IsApprovedByClient == true)
                .Select(w => new ReportWorkItem
                {
                    WorkName = w.Work!.WorkName,
                    Cost = w.Work.PricePerHour
                }).ToList() ?? new List<ReportWorkItem>();

            // 5. Получаем списанные ЗИП с ценой реализации
            var writeOffsViaTask = await _context.SparePartsWriteOffEntities
                .Include(w => w.TaskEntity)
                .Include(w => w.SupplierSpare)
                    .ThenInclude(ss => ss.Spare)
                .Where(w => w.TaskEntity != null && w.TaskEntity.RequestId == requestId)
                .ToListAsync();
            writeOffsViaTask = writeOffsViaTask.Take(2).ToList();

            // Загружаем цены реализации из таблицы "Учёт ЗИП"
            //var inventoryItems = await _context.Spares // Учёт ЗИП
            //    .Where(i => writeOffsViaTask.Select(w => w.SupplierSpare!.SpareCode).Contains(i.SpareCode))
            //    .ToDictionaryAsync(i => i.SpareCode, i => DefaultPrices[i.SpareCode]); // SellingPrice - новое поле в БД

            var spares = writeOffsViaTask
                .GroupBy(w => w.SupplierSpare!.SpareCode)
                .Select(g =>
                {
                    var first = g.First();
                    var spareCode = first.SupplierSpare!.SpareCode;
                    var sellingPrice = DefaultPrices[spareCode];

                    return new ReportSpareItem
                    {
                        SpareCode = spareCode,
                        SpareName = first.SupplierSpare!.Spare!.SpareName,
                        Quantity = g.Sum(x => x.Quantity),
                        UnitPrice = first.SupplierSpare.UnitPrice,           // Цена закупки
                        SellingPrice = sellingPrice,                          // Цена реализации
                        TotalPrice = g.Sum(x => x.Quantity * x.SupplierSpare.UnitPrice),
                        TotalSellingPrice = g.Sum(x => x.Quantity * sellingPrice)
                    };
                }).ToList();

            _logger.LogInformation("📦 Получено актов списания ЗИП: {Count}", writeOffsViaTask.Count);

            foreach (var writeOff in writeOffsViaTask)
            {
                _logger.LogInformation("  - ЗИП код: {SpareCode}, Количество: {Quantity}, " +
                                      "Цена закупки: {PurchasePrice}",
                                      writeOff.SupplierSpare?.SpareCode,
                                      writeOff.Quantity,
                                      writeOff.SupplierSpare?.UnitPrice);
            }

                //var spares = writeOffsViaTask
                //    .GroupBy(w => w.SupplierSpare!.SpareCode)
                //    .Select(g => new ReportSpareItem
                //    {
                //        SpareName = g.First().SupplierSpare!.Spare!.SpareName,
                //        Quantity = g.Sum(x => x.Quantity),
                //        UnitPrice = g.First().SupplierSpare.UnitPrice,
                //        TotalPrice = g.Sum(x => x.Quantity * x.SupplierSpare.UnitPrice)
                //    }).ToList();

                // 6. Формируем DTO
                var reportData = new AccountingReportData
            {
                RequestId = requestId,
                GeneratedAt = DateTime.UtcNow,
                DispatcherFio = dispatcher.User!.FullName,
                EngineerFios = engineers,
                Works = works,
                Spares = spares,
                TotalWorksCost = works.Sum(w => w.Cost),
                TotalSparesCost = spares.Sum(s => s.TotalPrice)
            };

            // 7. Генерируем PDF
            string fileName = $"Report_Req{requestId}_{DateTime.UtcNow:yyyyMMdd}.pdf";
            string reportsDir = Path.Combine(_env.WebRootPath, "documents", "reports");
            Directory.CreateDirectory(reportsDir);
            string fullPath = Path.Combine(reportsDir, fileName);
            string relativePath = Path.Combine("documents", "reports", fileName);

            _logger.LogInformation("📄 Генерация PDF: {FullPath}", fullPath);

            await _pdfService.GenerateAccountingReportAsync(reportData, fullPath);

            // 8. Сохраняем в БД
            var reportEntity = new ReportEntity
            {
                RequestId = requestId,
                GeneratedAt = DateTime.UtcNow,
                FilePath = relativePath
            };

            _context.ReportEntities.Add(reportEntity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Отчёт сохранён с кодом #{ReportCode}", reportEntity.ReportCode);

            return reportEntity.ReportCode;
        }

        public static readonly Dictionary<int, decimal> SupplierZipPrices = new()
        {
            { 1, 3500.00m },  // Код ЗИП=1
            { 2, 3500.00m },  // Код ЗИП=1
            { 3, 3500.00m },  // Код ЗИП=1
            { 4, 4500.00m },  // Код ЗИП=2
            { 5, 4500.00m },  // Код ЗИП=2
            { 6, 5000.00m },  // Код ЗИП=3
            { 7, 5000.00m },  // Код ЗИП=3
            { 8, 2800.00m },  // Код ЗИП=4
            { 9, 2800.00m },  // Код ЗИП=4
            { 10, 3800.00m }, // Код ЗИП=5
            { 11, 3800.00m }, // Код ЗИП=5
            { 12, 5500.00m }, // Код ЗИП=6
            { 13, 5500.00m }, // Код ЗИП=6
            { 14, 7000.00m }, // Код ЗИП=7
            { 15, 7000.00m }, // Код ЗИП=7
            { 16, 2200.00m }, // Код ЗИП=8
            { 17, 2200.00m }, // Код ЗИП=8
            { 18, 3200.00m }, // Код ЗИП=9
            { 19, 3200.00m }, // Код ЗИП=9
            { 20, 5000.00m }, // Код ЗИП=10
            { 21, 5000.00m }, // Код ЗИП=10
            { 22, 2000.00m }, // Код ЗИП=11
            { 23, 2000.00m }, // Код ЗИП=11
            { 24, 2800.00m }, // Код ЗИП=12
            { 25, 2800.00m }, // Код ЗИП=12
            { 26, 2500.00m }, // Код ЗИП=13
            { 27, 2500.00m }, // Код ЗИП=13
            { 28, 3000.00m }, // Код ЗИП=14
            { 29, 3000.00m }, // Код ЗИП=14
            { 30, 3500.00m }, // Код ЗИП=15
            { 31, 3500.00m }, // Код ЗИП=15
            { 32, 4500.00m }, // Код ЗИП=16
            { 33, 4500.00m }, // Код ЗИП=16
            { 34, 2800.00m }, // Код ЗИП=17
            { 35, 2800.00m }, // Код ЗИП=17
            { 36, 3200.00m }, // Код ЗИП=18
            { 37, 3200.00m }, // Код ЗИП=18
            { 38, 1800.00m }, // Код ЗИП=19
            { 39, 1800.00m }, // Код ЗИП=19
            { 40, 1800.00m }, // Код ЗИП=20
            { 41, 1800.00m }, // Код ЗИП=20
            { 42, 3500.00m }, // Код ЗИП=21
            { 43, 3500.00m }, // Код ЗИП=21
            { 44, 2200.00m }, // Код ЗИП=22
            { 45, 2200.00m }, // Код ЗИП=22
            { 46, 1500.00m }, // Код ЗИП=23
            { 47, 1500.00m }, // Код ЗИП=23
            { 48, 1350.00m }, // Код ЗИП=24
            { 49, 1350.00m }, // Код ЗИП=24
            { 50, 200.00m },  // Код ЗИП=25
            { 51, 200.00m },  // Код ЗИП=25
            { 52, 600.00m },  // Код ЗИП=26
            { 53, 600.00m },  // Код ЗИП=26
            { 54, 4500.00m }, // Код ЗИП=27
            { 55, 4500.00m }, // Код ЗИП=27
            { 56, 6500.00m }, // Код ЗИП=28
            { 57, 6500.00m }, // Код ЗИП=28
            { 58, 7500.00m }, // Код ЗИП=29
            { 59, 7500.00m }, // Код ЗИП=29
            { 60, 5500.00m }, // Код ЗИП=30
            { 61, 6000.00m }, // Код ЗИП=31
            { 62, 6000.00m }, // Код ЗИП=31
            { 63, 2500.00m }, // Код ЗИП=32
            { 64, 2500.00m }, // Код ЗИП=32
            { 65, 2500.00m }, // Код ЗИП=33
            { 66, 2500.00m }, // Код ЗИП=33
            { 67, 2500.00m }, // Код ЗИП=34
            { 68, 2500.00m }, // Код ЗИП=34
            { 69, 2500.00m }, // Код ЗИП=35
            { 70, 2500.00m }, // Код ЗИП=35
            { 71, 2500.00m }, // Код ЗИП=36
            { 72, 2500.00m }, // Код ЗИП=36
            { 73, 1500.00m }, // Код ЗИП=37
            { 74, 1500.00m }, // Код ЗИП=37
            { 75, 2200.00m }, // Код ЗИП=38
            { 76, 2200.00m }, // Код ЗИП=38
            { 77, 600.00m },  // Код ЗИП=39
            { 78, 600.00m },  // Код ЗИП=39
            { 79, 1000.00m }, // Код ЗИП=40
            { 80, 1000.00m }, // Код ЗИП=40
            { 81, 1900.00m }, // Код ЗИП=41
            { 82, 1900.00m }, // Код ЗИП=41
            { 83, 1400.00m }, // Код ЗИП=42
            { 84, 1400.00m }, // Код ЗИП=42
            { 85, 1400.00m }, // Код ЗИП=43
            { 86, 1400.00m }, // Код ЗИП=43
            { 87, 600.00m },  // Код ЗИП=44
            { 88, 600.00m },  // Код ЗИП=44
            { 89, 800.00m },  // Код ЗИП=45
            { 90, 800.00m },  // Код ЗИП=45
            { 91, 150.00m },  // Код ЗИП=46
            { 92, 150.00m },  // Код ЗИП=46
            { 93, 100.00m },  // Код ЗИП=47
            { 94, 100.00m },  // Код ЗИП=47
            { 95, 300.00m },  // Код ЗИП=48
            { 96, 300.00m },  // Код ЗИП=48
            { 97, 400.00m },  // Код ЗИП=49
            { 98, 400.00m },  // Код ЗИП=49
            { 99, 600.00m },  // Код ЗИП=50
            { 100, 600.00m }, // Код ЗИП=50
            { 101, 300.00m }, // Код ЗИП=51
            { 102, 300.00m }, // Код ЗИП=51
            { 103, 700.00m }, // Код ЗИП=52
            { 104, 700.00m }, // Код ЗИП=52
            { 105, 500.00m }, // Код ЗИП=53
            { 106, 500.00m }, // Код ЗИП=53
            { 107, 200.00m }, // Код ЗИП=54
            { 108, 200.00m }, // Код ЗИП=54
            { 109, 150.00m }, // Код ЗИП=55
            { 110, 150.00m }, // Код ЗИП=55
            { 111, 250.00m }, // Код ЗИП=56
            { 112, 250.00m }, // Код ЗИП=56
            { 113, 400.00m }, // Код ЗИП=57
            { 114, 400.00m }, // Код ЗИП=57
            { 115, 100.00m }, // Код ЗИП=58
            { 116, 100.00m }, // Код ЗИП=58
            { 117, 4500.00m },// Код ЗИП=59
            { 118, 4500.00m },// Код ЗИП=59
            { 119, 12000.00m },// Код ЗИП=60
            { 120, 12000.00m },// Код ЗИП=60
            { 121, 5500.00m },// Код ЗИП=61
            { 122, 5500.00m },// Код ЗИП=61
            { 123, 10000.00m },// Код ЗИП=62
            { 124, 10000.00m },// Код ЗИП=62
            { 125, 18000.00m },// Код ЗИП=63
            { 126, 18000.00m },// Код ЗИП=63
            { 127, 10000.00m },// Код ЗИП=64
            { 128, 10000.00m },// Код ЗИП=64
            { 129, 15000.00m },// Код ЗИП=65
            { 130, 15000.00m },// Код ЗИП=65
            { 131, 8000.00m }, // Код ЗИП=66
            { 132, 8000.00m }, // Код ЗИП=66
            { 133, 10000.00m },// Код ЗИП=67
            { 134, 10000.00m },// Код ЗИП=67
            { 135, 8500.00m }, // Код ЗИП=68
            { 136, 8500.00m }, // Код ЗИП=68
            { 137, 14000.00m },// Код ЗИП=69
            { 138, 14000.00m },// Код ЗИП=69
            { 139, 1000.00m }, // Код ЗИП=70
            { 140, 1000.00m }, // Код ЗИП=70
            { 141, 1000.00m }, // Код ЗИП=71
            { 142, 1000.00m }, // Код ЗИП=71
            { 143, 500.00m },  // Код ЗИП=72
            { 144, 500.00m },  // Код ЗИП=72
            { 145, 1000.00m }, // Код ЗИП=73
            { 146, 1000.00m }, // Код ЗИП=73
            { 147, 1500.00m }, // Код ЗИП=74
            { 148, 1500.00m }, // Код ЗИП=74
            { 149, 800.00m },  // Код ЗИП=75
            { 150, 800.00m },  // Код ЗИП=75
            { 151, 900.00m },  // Код ЗИП=76
            { 152, 900.00m },  // Код ЗИП=76
            { 153, 2000.00m }, // Код ЗИП=77
            { 154, 2000.00m }, // Код ЗИП=77
            { 155, 1000.00m }, // Код ЗИП=78
            { 156, 1000.00m }, // Код ЗИП=78
            { 157, 600.00m },  // Код ЗИП=79
            { 158, 600.00m },  // Код ЗИП=79
            { 159, 500.00m },  // Код ЗИП=80
            { 160, 500.00m },  // Код ЗИП=80
            { 161, 300.00m },  // Код ЗИП=81
            { 162, 300.00m },  // Код ЗИП=81
            { 163, 200.00m },  // Код ЗИП=82
            { 164, 200.00m }   // Код ЗИП=82
        };

        public static readonly Dictionary<int, decimal> DefaultPrices = new()
    {
        { 1, 3500.00m },   // Жёсткий диск HDD 500GB
        { 2, 4500.00m },   // Жёсткий диск HDD 1TB
        { 3, 5000.00m },   // Жёсткий диск HDD 2TB
        { 4, 2800.00m },   // SSD диск 120GB
        { 5, 3800.00m },   // SSD диск 240GB
        { 6, 5500.00m },   // SSD диск 480GB
        { 7, 7000.00m },   // SSD диск 1TB M.2 NVMe
        { 8, 2200.00m },   // Оперативная память DDR4 4GB
        { 9, 3200.00m },   // Оперативная память DDR4 8GB
        { 10, 5000.00m },  // Оперативная память DDR4 16GB
        { 11, 2000.00m },  // Оперативная память DDR3 4GB
        { 12, 2800.00m },  // Оперативная память DDR3 8GB
        { 13, 2500.00m },  // Блок питания 300W
        { 14, 3000.00m },  // Блок питания 400W
        { 15, 3500.00m },  // Блок питания 500W
        { 16, 4500.00m },  // Блок питания 600W
        { 17, 2800.00m },  // Блок питания для ноутбука 65W
        { 18, 3200.00m },  // Блок питания для ноутбука 90W
        { 19, 1800.00m },   // Кулер процессора Intel
        { 20, 1800.00m },   // Кулер процессора AMD
        { 21, 3500.00m },  // Кулер процессора башенный
        { 22, 2200.00m },  // Кулер для ноутбука 15"
        { 23, 1500.00m },  // Кулер для ноутбука 17"
        { 24, 1350.00m },   // Термопаста Arctic MX-4
        { 25, 200.00m },    // Термопаста КПТ-8
        { 26, 600.00m },   // Термопрокладки набор
        { 27, 4500.00m },  // Матрица 15.6" HD LED
        { 28, 6500.00m },  // Матрица 15.6" Full HD IPS
        { 29, 7500.00m },  // Матрица 17.3" Full HD
        { 30, 5500.00m },  // Матрица 13.3" IPS
        { 31, 6000.00m },  // Матрица 14" Full HD
        { 32, 2500.00m },  // Клавиатура для ноутбука Asus
        { 33, 2500.00m },  // Клавиатура для ноутбука Lenovo
        { 34, 2500.00m },  // Клавиатура для ноутбука HP
        { 35, 2500.00m },  // Клавиатура для ноутбука Acer
        { 36, 2500.00m },  // Клавиатура для ноутбука Dell
        { 37, 1500.00m },   // Клавиатура USB проводная
        { 38, 2200.00m },  // Клавиатура беспроводная
        { 39, 600.00m },   // Мышь USB проводная
        { 40, 1000.00m },   // Мышь беспроводная
        { 41, 1900.00m },   // Тачпад для ноутбука
        { 42, 1400.00m },   // Петля для ноутбука левая
        { 43, 400.00m },   // Петля для ноутбука правая
        { 44, 600.00m },   // Корпус для HDD 2.5" USB 3.0
        { 45, 800.00m },   // Корпус для HDD 3.5" USB 3.0
        { 46, 150.00m },   // Кабель SATA 3.0
        { 47, 100.00m },   // Кабель SATA питания
        { 48, 300.00m },   // Кабель IDE-SATA переходник
        { 49, 400.00m },   // Кабель HDMI 1.5m
        { 50, 600.00m },   // Кабель HDMI 3m
        { 51, 300.00m },   // Кабель VGA 1.5m
        { 52, 700.00m },   // Кабель DisplayPort
        { 53, 500.00m },   // USB-хаб 4 порта
        { 54, 200.00m },   // Разъём питания DC для ноутбука
        { 55, 150.00m },   // USB-порт 2.0 для материнской платы
        { 56, 250.00m },   // USB-порт 3.0 для материнской платы
        { 57, 400.00m },   // Разъём HDMI для материнской платы
        { 58, 100.00m },   // Разъём наушников/микрофона 3.5mm
        { 59, 4500.00m },  // Видеокарта GeForce GT 1030 2GB
        { 60, 12000.00m }, // Видеокарта GeForce GTX 1650 4GB
        { 61, 5500.00m },  // Видеокарта Radeon RX 550 2GB
        { 62, 10000.00m },  // Процессор Intel Core i3 10th gen
        { 63, 18000.00m }, // Процессор Intel Core i5 10th gen
        { 64, 10000.00m },  // Процессор AMD Ryzen 3
        { 65, 15000.00m }, // Процессор AMD Ryzen 5
        { 66, 8000.00m },  // Материнская плата Intel H510
        { 67, 10000.00m },  // Материнская плата Intel B560
        { 68, 8500.00m },  // Материнская плата AMD B450
        { 69, 14000.00m }, // Материнская плата AMD B550
        { 70, 1000.00m },   // Динамик для ноутбука левый
        { 71, 1000.00m },   // Динамик для ноутбука правый
        { 72, 500.00m },   // Микрофон для ноутбука
        { 73, 1000.00m },   // Веб-камера встроенная
        { 74, 1500.00m },  // Wi-Fi модуль для ноутбука
        { 75, 800.00m },   // Bluetooth модуль
        { 76, 900.00m },   // Шлейф матрицы универсальный
        { 77, 2000.00m },  // Инвертор для монитора
        { 78, 1000.00m },   // Лампа подсветки для монитора
        { 79, 600.00m },   // Контроллер питания материнской платы
        { 80, 500.00m },   // Микросхема BIOS
        { 81, 300.00m },   // Конденсаторы набор
        { 82, 200.00m }    // Предохранители набор
    };
    }
}