// Diploma.Application/Services/PdfGenerationService.cs

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.Extensions.Logging;
using Diploma.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Diploma.Core.Abstractions;
using Microsoft.AspNetCore.Hosting;
using static Diploma.Core.Class1;

namespace Diploma.Application.Services
{

    public class PdfGenerationService : IPdfGenerationService
    {
        private readonly DiplomaDbContext _context;
        private readonly ILogger<PdfGenerationService> _logger;
        private readonly IWebHostEnvironment _env;  
        public PdfGenerationService(DiplomaDbContext context, ILogger<PdfGenerationService> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;
        }
        public async Task<string> GenerateContractAsync(int requestId, string outputPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            var requestData = await LoadContractDataAsync(requestId);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // HEADER
                    page.Header()
                        .Element(e => e.Column(col =>
                        {
                            col.Item().Text("ДОГОВОР ОКАЗАНИЯ УСЛУГ ПО ТЕХНИЧЕСКОМУ ОБСЛУЖИВАНИЮ И РЕМОНТУ СРЕДСТВ ВЫЧИСЛИТЕЛЬНОЙ ТЕХНИКИ").FontSize(14).Bold().AlignCenter();
                            col.Item().Text($"№ {requestId}/2026").FontSize(12).AlignCenter();
                            col.Item().Text($"Дата: {DateTime.Now:dd.MM.yyyy}").FontSize(11).AlignCenter();
                            col.Item().PaddingTop(10);
                        }));

                    // CONTENT
                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(col =>
                        {
                            // 1. ПРЕАМБУЛА
                            SectionTitle(col.Item(), "1. ПРЕАМБУЛА");
                            col.Item().Text($"г. {requestData.city}, {DateTime.Now:dd.MM.yyyy} г.");
                            col.Item().Text("");
                            col.Item().Text($"Гражданин(ка) {requestData.clientFio}, именуемый(ая) в дальнейшем «Заказчик», с одной стороны, и Индивидуальный предприниматель / Общество с ограниченной ответственностью \"{requestData.companyName}\", именуемый(ое) в дальнейшем «Исполнитель», с другой стороны, заключили настоящий договор о нижеследующем:");
                            col.Item().PaddingTop(10);

                            // 2. ПРЕДМЕТ ДОГОВОРА
                            SectionTitle(col.Item(), "2. ПРЕДМЕТ ДОГОВОРА");
                            col.Item().Text("2.1. Исполнитель обязуется оказать услуги по техническому обслуживанию и ремонту средств вычислительной техники (далее - СВТ), принадлежащих Заказчику, а Заказчик обязуется принять и оплатить эти услуги.");
                            col.Item().Text("2.2. Наименование СВТ: " + requestData.svtType);
                            col.Item().Text("2.3. Модель: " + requestData.model);
                            col.Item().Text("2.4. Серийный номер: " + requestData.serialNumber);
                            col.Item().Text("2.5. Описание неисправности: " + requestData.description);
                            col.Item().PaddingTop(10);

                            // 3. ПРАВА И ОБЯЗАННОСТИ СТОРОН
                            SectionTitle(col.Item(), "3. ПРАВА И ОБЯЗАННОСТИ СТОРОН");
                            col.Item().Text("3.1. Исполнитель обязуется:");
                            col.Item().Text("   - оказать услуги качественно и в срок;");
                            col.Item().Text("   - использовать качественные запасные части и материалы;");
                            col.Item().Text("   - предоставить гарантию на выполненные работы и установленные запчасти.");
                            col.Item().Text("3.2. Заказчик обязуется:");
                            col.Item().Text("   - предоставить СВТ в состоянии, позволяющем выполнить работы;");
                            col.Item().Text("   - оплатить услуги в порядке и сроки, установленные настоящим договором;");
                            col.Item().Text("   - принять выполненные работы по акту сдачи-приемки.");
                            col.Item().PaddingTop(10);

                            // 4. СТОИМОСТЬ УСЛУГ И ПОРЯДОК ОПЛАТЫ
                            SectionTitle(col.Item(), "4. СТОИМОСТЬ УСЛУГ И ПОРЯДОК ОПЛАТЫ");
                            col.Item().Text($"4.1. Стоимость согласованных работ составляет: {requestData.worksCost:F2} ({NumberToWords(requestData.worksCost)}) рублей.");
                            col.Item().Text($"4.2. Стоимость согласованных запасных частей: {requestData.sparesCost:F2} ({NumberToWords(requestData.sparesCost)}) рублей.");
                            col.Item().Text($"4.3. ОБЩАЯ СТОИМОСТЬ: {requestData.totalCost:F2} ({NumberToWords(requestData.totalCost)}) рублей.");
                            col.Item().Text("4.4. Порядок оплаты:");
                            col.Item().Text("   - Предоплата за запасные части (ЗИП) в размере 100% от их стоимости.");
                            col.Item().Text("   - Полная оплата работ и запчастей производится после выполнения работ.");
                            col.Item().PaddingTop(10);

                            // 5. ПЕРЕЧЕНЬ РАБОТ
                            SectionTitle(col.Item(), "5. ПЕРЕЧЕНЬ ВЫПОЛНЯЕМЫХ РАБОТ");
                            if (requestData.works.Any())
                            {
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(5);
                                        columns.RelativeColumn(2);
                                    });

                                    // 🔥 ИСПРАВЛЕНО: Заголовок таблицы
                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("№").Bold();
                                        header.Cell().Element(CellStyle).Text("Наименование работы").Bold();
                                        header.Cell().Element(CellStyle).Text("Стоимость, ₽").Bold();
                                    });

                                    // 🔥 ИСПРАВЛЕНО: Данные таблицы (используем table.Cell, а не header.Cell)
                                    int workNum = 1;
                                    foreach (var work in requestData.works)
                                    {
                                        table.Cell().Element(CellStyle).Text(workNum.ToString());
                                        table.Cell().Element(CellStyle).Text(work.workName);
                                        table.Cell().Element(CellStyle).AlignRight().Text($"{work.estimatedCost:F2}");
                                        workNum++;
                                    }

                                    // 🔥 ИСПРАВЛЕНО: Итого (используем ColumnSpan на ячейке)
                                    table.Cell().ColumnSpan(2).Element(CellStyle).Text("Итого за работы:").Bold();
                                    table.Cell().Element(CellStyle).AlignRight().Text($"{requestData.worksCost:F2}").Bold();
                                });
                            }
                            else
                            {
                                col.Item().Text("Работы не предусмотрены.");
                            }
                            col.Item().PaddingTop(10);

                            // 6. ПЕРЕЧЕНЬ ЗИП
                            SectionTitle(col.Item(), "6. ПЕРЕЧЕНЬ ЗАПАСНЫХ ЧАСТЕЙ (ЗИП)");
                            if (requestData.spares.Any())
                            {
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    // 🔥 ИСПРАВЛЕНО: Заголовок таблицы
                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("№").Bold();
                                        header.Cell().Element(CellStyle).Text("Наименование").Bold();
                                        header.Cell().Element(CellStyle).Text("Кол-во").Bold();
                                        header.Cell().Element(CellStyle).Text("Ед. изм.").Bold();
                                        header.Cell().Element(CellStyle).Text("Стоимость, ₽").Bold();
                                    });

                                    // 🔥 ИСПРАВЛЕНО: Данные таблицы
                                    int spareNum = 1;
                                    foreach (var spare in requestData.spares)
                                    {
                                        table.Cell().Element(CellStyle).Text(spareNum.ToString());
                                        table.Cell().Element(CellStyle).Text(spare.spareName);
                                        table.Cell().Element(CellStyle).Text(spare.quantity.ToString());
                                        table.Cell().Element(CellStyle).Text(spare.unit);
                                        table.Cell().Element(CellStyle).AlignRight().Text($"{spare.totalPrice:F2}");
                                        spareNum++;
                                    }

                                    // 🔥 ИСПРАВЛЕНО: Итого
                                    table.Cell().ColumnSpan(4).Element(CellStyle).Text("Итого за запчасти:").Bold();
                                    table.Cell().Element(CellStyle).AlignRight().Text($"{requestData.sparesCost:F2}").Bold();
                                });
                            }
                            else
                            {
                                col.Item().Text("Запасные части не требуются.");
                            }
                            col.Item().PaddingTop(10);

                            // 7. СРОКИ ВЫПОЛНЕНИЯ РАБОТ
                            SectionTitle(col.Item(), "7. СРОКИ ВЫПОЛНЕНИЯ РАБОТ");
                            col.Item().Text($"7.1. Срок выполнения работ: {requestData.estimatedTime ?? "не установлен"}.");
                            col.Item().Text("7.2. Срок исчисляется с момента поступления предоплаты на расчетный счет Исполнителя.");
                            col.Item().PaddingTop(10);

                            // 8. ГАРАНТИЙНЫЕ ОБЯЗАТЕЛЬСТВА
                            SectionTitle(col.Item(), "8. ГАРАНТИЙНЫЕ ОБЯЗАТЕЛЬСТВА");
                            col.Item().Text("8.1. Исполнитель предоставляет гарантию на выполненные работы сроком на 3 (три) месяца.");
                            col.Item().Text("8.2. Гарантия на установленные запасные части предоставляется в соответствии с гарантийными обязательствами производителя.");
                            col.Item().PaddingTop(10);

                            // 9. ОТВЕТСТВЕННОСТЬ СТОРОН
                            SectionTitle(col.Item(), "9. ОТВЕТСТВЕННОСТЬ СТОРОН");
                            col.Item().Text("9.1. За неисполнение или ненадлежащее исполнение обязательств по настоящему договору Стороны несут ответственность в соответствии с действующим законодательством Российской Федерации.");
                            col.Item().PaddingTop(10);

                            // 10. РЕКВИЗИТЫ И ПОДПИСИ СТОРОН
                            SectionTitle(col.Item(), "10. РЕКВИЗИТЫ И ПОДПИСИ СТОРОН");
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                // Заказчик
                                table.Cell().Element(CellStyle).Column(column =>
                                {
                                    column.Item().Text("ЗАКАЗЧИК:").Bold();
                                    column.Item().Text($"ФИО: {requestData.clientFio}");
                                    column.Item().Text($"Телефон: {requestData.clientPhone}");
                                    column.Item().Text($"Адрес: {requestData.clientAddress ?? "___________________"}");
                                    column.Item().PaddingTop(20);
                                    column.Item().Text("Подпись: _________________");
                                });

                                // Исполнитель
                                table.Cell().Element(CellStyle).Column(column =>
                                {
                                    column.Item().Text("ИСПОЛНИТЕЛЬ:").Bold();
                                    column.Item().Text($"Наименование: {requestData.companyName}");
                                    column.Item().Text($"ИНН: {requestData.companyInn ?? "___________________"}");
                                    column.Item().Text($"ОГРН: {requestData.companyOgrn ?? "___________________"}");
                                    column.Item().Text($"Адрес: {requestData.companyAddress ?? "___________________"}");
                                    column.Item().Text($"Телефон: {requestData.companyPhone ?? "___________________"}");
                                    column.Item().PaddingTop(20);
                                    column.Item().Text("Подпись: _________________");
                                    column.Item().Text("М.П.");
                                });
                            });
                        });

                    // FOOTER
                    // FOOTER
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Страница ").FontSize(9).FontColor(Colors.Grey.Medium);
                            x.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Medium);
                            x.Span(" из ").FontSize(9).FontColor(Colors.Grey.Medium);
                            x.TotalPages().FontSize(9).FontColor(Colors.Grey.Medium);
                        });
                });
            }).GeneratePdf(outputPath);

            _logger.LogInformation("✅ Договор сформирован: {Path}", outputPath);
            return outputPath;
        }

        private static void SectionTitle(IContainer container, string title)
        {
            container
                .PaddingTop(10)
                .PaddingBottom(5)
                .Text(text =>
                {
                    text.DefaultTextStyle(t => t.FontSize(12).Bold());
                    text.Span(title);
                });
        }

        private static IContainer CellStyle(IContainer container) =>
            container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);

        private string NumberToWords(decimal number)
        {
            // Простая реализация - можно улучшить
            return number.ToString("F2");
        }

        private async Task<ContractData> LoadContractDataAsync(int requestId)
        {
            var request = await _context.Requests
                .Include(r => r.ClientEntity)
                    .ThenInclude(c => c.User)  // 🔥 Добавлено: загружаем пользователя клиента
                .FirstOrDefaultAsync(r => r.Id == requestId);

            var act = await _context.DiagnosticActs
                .Include(a => a.Task)
                .Include(a => a.Works)
                    .ThenInclude(w => w.Work)
                .Include(a => a.SpareParts)
                    .ThenInclude(s => s.Spare)
                .FirstOrDefaultAsync(a => a.Task!.RequestId == requestId);

            // 🔥 ИСПРАВЛЕНО: Фильтруем только согласованные работы
            var works = act?.Works
                .Where(w => w.IsApprovedByClient == true)  // ✅ Только согласованные
                .Select(w => new WorkInfo
                {
                    workName = w.Work?.WorkName ?? $"Работа #{w.WorkCode}",
                    estimatedCost = w.Work?.PricePerHour ?? 0
                }).ToList() ?? new List<WorkInfo>();

            // 🔥 ИСПРАВЛЕНО: Фильтруем только согласованные ЗИП
            var spares = act?.SpareParts
                .Where(s => s.IsApprovedByClient == true)  // ✅ Только согласованные
                .Select(s =>
                {
                    var supplierSpare = _context.SupplierSpares
                        .FirstOrDefault(ss => ss.SpareCode == s.SpareCode);

                    return new SpareInfo
                    {
                        spareName = s.Spare?.SpareName ?? $"ЗИП #{s.SpareCode}",
                        quantity = s.Amount,
                        //totalPrice = (supplierSpare?.UnitPrice ?? 0) * s.Amount
                        totalPrice = SparePartsService.DefaultPrices[s.Spare.SpareCode] * s.Amount
                    };
                }).ToList() ?? new List<SpareInfo>();

            var worksCost = works.Sum(w => w.estimatedCost);
            var sparesCost = spares.Sum(s => s.totalPrice);

            // Получаем ФИО из разных источников
            string clientFio = request?.ClientEntity?.User?.FullName;
            string clientPhone = request?.ClientEntity?.Phone ?? "+7 (___) ___-__-__";

            // Если ФИО пустое, пробуем получить из других источников
            if (string.IsNullOrWhiteSpace(clientFio))
            {
                // Пытаемся получить из заявки (если там есть поле для ФИО)
                clientFio = "Клиент";  // Дефолтное значение
            }


            return new ContractData
            {
                contractNumber = $"{requestId}/2026",  // 🔥 Добавьте
                city = "Казань",
                clientFio = clientFio,  
                clientPhone = clientPhone,
                clientAddress = null,  
                svtType = request?.SvtType ?? "___________________",
                model = request?.Model ?? "___________________",
                serialNumber = request?.SerialNumber ?? "___________________",
                description = request?.Description ?? "___________________",
                works = works,
                spares = spares,
                worksCost = worksCost,
                sparesCost = sparesCost,
                totalCost = worksCost + sparesCost,
                estimatedTime = act?.EstimatedTime,
                companyName = "ООО \"Мастерская по ремонту СВТ\"",
                companyInn = "7701234567",
                companyOgrn = "1157746123456",
                companyAddress = "г. Казань, ул. Примерная, д. 1",
                companyPhone = "+7 (495) 123-45-67"
            };
        }

        public async Task<string> GenerateInvoiceAsync(int requestId, decimal amount, string outputPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);

                    page.Header()
                        .Row(row =>
                        {
                            row.RelativeItem().Text("СЧЁТ НА ОПЛАТУ").FontSize(16).Bold();
                            row.RelativeItem().AlignRight().Text($"№ {requestId}-INV");
                        });

                    page.Content()
                        .Column(col =>
                        {
                            col.Item().PaddingTop(20).Text($"Заказчик: Заявка #{requestId}");
                            col.Item().Text($"Дата выставления: {DateTime.Now:dd.MM.yyyy}");

                            col.Item().PaddingTop(20).Text("НАИМЕНОВАНИЕ УСЛУГ:").Bold();
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Услуга").Bold();
                                    header.Cell().Element(CellStyle).Text("Кол-во").Bold();
                                    header.Cell().Element(CellStyle).AlignRight().Text("Сумма").Bold();
                                });

                                table.Cell().Element(CellStyle).Text("Техническое обслуживание и ремонт");
                                table.Cell().Element(CellStyle).Text("1");
                                table.Cell().Element(CellStyle).AlignRight().Text($"{amount:F2} ₽");
                            });

                            col.Item().PaddingTop(20).AlignRight()
                                .Text($"ИТОГО К ОПЛАТЕ: {amount:F2} ₽").FontSize(14).Bold();

                            col.Item().PaddingTop(30).Text("Реквизиты для оплаты:");
                            col.Item().Text("ООО «СервисЦентр», ИНН 0000000000, р/с 00000000000000000000");
                        });
                });
            }).GeneratePdf(outputPath);

            _logger.LogInformation("✅ Счёт сформирован: {Path}", outputPath);
            return outputPath;
        }

        public async Task<string> GeneratePrepaymentInvoiceAsync(int requestId, string outputPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            var invoiceData = await LoadInvoiceDataAsync(requestId);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // HEADER
                    page.Header()
                        .Element(e => e.Column(col =>
                        {
                            col.Item().Text("СЧЁТ НА ПРЕДОПЛАТУ").FontSize(16).Bold().AlignCenter();
                            col.Item().Text($"№ INV-{requestId}/2026").FontSize(12).AlignCenter();
                            col.Item().Text($"Дата: {DateTime.Now:dd.MM.yyyy}").FontSize(11).AlignCenter();
                            col.Item().PaddingTop(10);
                        }));

                    // CONTENT
                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(col =>
                        {
                            // Реквизиты
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn();
                                    cols.RelativeColumn();
                                });

                                table.Cell().Element(CellStyle).Column(c =>
                                {
                                    c.Item().Text("ПОСТАВЩИК:").Bold();
                                    c.Item().Text(invoiceData.companyName);
                                    c.Item().Text($"ИНН: {invoiceData.companyInn}");
                                    c.Item().Text($"Адрес: {invoiceData.companyAddress}");
                                    c.Item().Text($"Тел: {invoiceData.companyPhone}");
                                });

                                table.Cell().Element(CellStyle).Column(c =>
                                {
                                    c.Item().Text("ЗАКАЗЧИК:").Bold();
                                    c.Item().Text(invoiceData.clientFio);
                                    c.Item().Text($"Тел: {invoiceData.clientPhone}");
                                    c.Item().Text($"Заявка №: {requestId}");
                                });
                            });

                            col.Item().PaddingTop(20);

                            // 🔥 Только ЗИП для предоплаты
                            col.Item().Text("ПРЕДМЕТ ОПЛАТЫ: Запасные части (ЗИП)").Bold().FontSize(12);
                            col.Item().PaddingTop(10);

                            if (invoiceData.spares.Any())
                            {
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(cols =>
                                    {
                                        cols.RelativeColumn(1);
                                        cols.RelativeColumn(4);
                                        cols.RelativeColumn(2);
                                        cols.RelativeColumn(2);
                                        cols.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("№").Bold();
                                        header.Cell().Element(CellStyle).Text("Наименование").Bold();
                                        header.Cell().Element(CellStyle).Text("Кол-во").Bold();
                                        header.Cell().Element(CellStyle).Text("Ед.").Bold();
                                        header.Cell().Element(CellStyle).Text("Сумма, ₽").Bold();
                                    });

                                    int num = 1;
                                    foreach (var spare in invoiceData.spares)
                                    {
                                        table.Cell().Element(CellStyle).Text(num.ToString());
                                        table.Cell().Element(CellStyle).Text(spare.spareName);
                                        table.Cell().Element(CellStyle).Text(spare.quantity.ToString());
                                        table.Cell().Element(CellStyle).Text(spare.unit);
                                        table.Cell().Element(CellStyle).AlignRight().Text($"{spare.totalPrice:F2}");
                                        num++;
                                    }

                                    // Итого
                                    table.Cell().ColumnSpan(4).Element(CellStyle).Text("ИТОГО К ОПЛАТЕ:").Bold().FontSize(12);
                                    table.Cell().Element(CellStyle).AlignRight().Text($"{invoiceData.totalAmount:F2} ₽").Bold().FontSize(12);
                                });
                            }
                            else
                            {
                                col.Item().Text("Запасные части не требуются.");
                            }

                            col.Item().PaddingTop(20);

                            // Условия оплаты
                            col.Item().Text("УСЛОВИЯ ОПЛАТЫ:").Bold();
                            col.Item().Text("• Предоплата 100% за запасные части");
                            col.Item().Text("• Оплата работ производится после их выполнения");
                            col.Item().Text("• Срок оплаты: 3 банковских дня");

                            col.Item().PaddingTop(20);

                            // Реквизиты для оплаты
                            col.Item().Text("РЕКВИЗИТЫ ДЛЯ ОПЛАТЫ:").Bold();
                            col.Item().Text($"Получатель: {invoiceData.companyName}");
                            col.Item().Text($"ИНН: {invoiceData.companyInn}");
                            col.Item().Text($"Р/с: {invoiceData.bankAccount}");
                            col.Item().Text($"Банк: {invoiceData.bankName}");
                            col.Item().Text($"К/с: {invoiceData.bankCorrespondent}");
                            col.Item().Text($"БИК: {invoiceData.bankBik}");
                            col.Item().Text($"Назначение: Предоплата за ЗИП по заявке №{requestId}");

                            col.Item().PaddingTop(30);
                            col.Item().Text("Счёт действителен в течение 10 дней с даты выставления.").FontSize(9).FontColor(Colors.Grey.Medium);
                        });

                    // FOOTER
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.DefaultTextStyle(t => t.FontSize(9).FontColor(Colors.Grey.Medium));
                            x.Span("Страница ");
                            x.CurrentPageNumber();
                            x.Span(" из ");
                            x.TotalPages();
                        });
                });
            }).GeneratePdf(outputPath);

            _logger.LogInformation("✅ Счёт на предоплату сформирован: {Path}", outputPath);
            return outputPath;
        }

        // 🔥 Вспомогательный метод для загрузки данных счёта
        private async Task<InvoiceData> LoadInvoiceDataAsync(int requestId)
        {
            var request = await _context.Requests
                .Include(r => r.ClientEntity)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            var act = await _context.DiagnosticActs
                .Include(a => a.SpareParts)
                    .ThenInclude(s => s.Spare)
                .FirstOrDefaultAsync(a => a.Task!.RequestId == requestId);

            // 🔥 Только согласованные ЗИП для предоплаты
            var spares = act?.SpareParts
                .Where(s => s.IsApprovedByClient == true)
                .Select(s =>
                {
                    var supplierSpare = _context.SupplierSpares
                        .FirstOrDefault(ss => ss.SpareCode == s.SpareCode);

                    return new SpareInfo
                    {
                        spareName = s.Spare?.SpareName ?? $"ЗИП #{s.SpareCode}",
                        quantity = s.Amount,
                        //totalPrice = (supplierSpare?.UnitPrice ?? 0) * s.Amount
                        totalPrice = SparePartsService.DefaultPrices[s.Spare.SpareCode] * s.Amount
                    };
                }).ToList() ?? new List<SpareInfo>();

            return new InvoiceData
            {
                clientFio = request?.ClientEntity?.User?.FullName ?? "___________________",
                clientPhone = request?.ClientEntity?.Phone ?? "___________________",
                spares = spares,
                totalAmount = spares.Sum(s => s.totalPrice),
                // Реквизиты компании (вынести в настройки)
                companyName = "ООО \"Мастерская по ремонту СВТ\"",
                companyInn = "7701234567",
                companyAddress = "г. Казань, ул. Примерная, д. 1",
                companyPhone = "+7 (495) 123-45-67",
                bankAccount = "40702810100000012345",
                bankName = "ПАО \"Банк\"",
                bankCorrespondent = "30101810400000000225",
                bankBik = "044525225"
            };
        }

        public async Task<string> GenerateRepairActAsync(RepairActData data)
        {
            var docsPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "documents", "repair-acts");
            Directory.CreateDirectory(docsPath);

            var fileName = $"ActRepair_{data.TaskId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(docsPath, fileName);
            var relativePath = Path.Combine("documents", "repair-acts", fileName).Replace("\\", "/");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // HEADER
                    page.Header()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.DefaultTextStyle(x => x.FontSize(14).Bold());
                            text.Span("АКТ ВЫПОЛНЕННЫХ РАБОТ\n");
                            text.Span($"№ {data.TaskId} от {data.CompletedAt:dd.MM.yyyy}").FontSize(12);
                        });

                    // CONTENT
                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(col =>
                        {
                            // Исполнитель
                            col.Item().PaddingBottom(5).Text("ИСПОЛНИТЕЛЬ:").Bold();
                            col.Item().PaddingBottom(10).Text("ООО \"Мастерская по ремонту СВТ\"\nИНН: 0000000000");

                            // Заказчик
                            col.Item().PaddingBottom(5).Text("ЗАКАЗЧИК:").Bold();
                            col.Item().PaddingBottom(10).Text(data.ClientFio);

                            // Работы
                            col.Item().PaddingBottom(5).Text("ВЫПОЛНЕНЫ РАБОТЫ:").Bold();
                            col.Item().Text($"Устройство: {data.DeviceModel}");
                            col.Item().Text($"Серийный номер: {data.SerialNumber}");
                            col.Item().Text($"Работа: {data.WorkName}");
                            col.Item().PaddingBottom(10).Text($"Исполнитель: {data.EngineerFio} (таб. №{data.EngineerTabNumber})");

                            // Подписи сторон
                            col.Item().PaddingTop(20).Text("ПОДПИСИ СТОРОН:").Bold().FontSize(12);
                            col.Item().PaddingTop(10);

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                // Исполнитель
                                table.Cell().Border(1).Padding(10).Column(c =>
                                {
                                    c.Item().Text("ИСПОЛНИТЕЛЬ:").Bold();
                                    c.Item().PaddingTop(30).Text("_________________");
                                    c.Item().Text("(подпись)");
                                    c.Item().PaddingTop(5).Text(data.EngineerFio);
                                    c.Item().Text("(расшифровка подписи)");
                                });

                                // Заказчик
                                table.Cell().Border(1).Padding(10).Column(c =>
                                {
                                    c.Item().Text("ЗАКАЗЧИК:").Bold();
                                    c.Item().PaddingTop(30).Text("_________________");
                                    c.Item().Text("(подпись)");
                                    c.Item().PaddingTop(5).Text(data.ClientFio);
                                    c.Item().Text("(расшифровка подписи)");
                                });
                            });

                            // 🔥 БЛОК С ЭЛЕКТРОННЫМИ ПОДПИСЯМИ (как на изображении)
                            if (data.ClientSignedAt.HasValue || data.DispatcherSignedAt.HasValue)
                            {
                                col.Item().PaddingTop(20).Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4).Padding(10).Column(c =>
                                {
                                    // Заголовок блока
                                    c.Item().AlignCenter().Text(text =>
                                    {
                                        text.DefaultTextStyle(x => x.FontSize(10).Bold().FontColor(Colors.Grey.Darken2));
                                        text.Span("ДОКУМЕНТ ПОДПИСАН\n");
                                        text.Span("ПРОСТОЙ ЭЛЕКТРОННОЙ ПОДПИСЬЮ");
                                    });

                                    c.Item().PaddingTop(10);

                                    // Информация о подписавших
                                    if (data.ClientSignedAt.HasValue)
                                    {
                                        c.Item().Text(text =>
                                        {
                                            text.DefaultTextStyle(x => x.FontSize(9));
                                            text.Span("Подписал (клиент): ").Bold();
                                            text.Span($"{data.ClientFio}\n");
                                            text.Span($"Дата подписания: {data.ClientSignedAt:dd.MM.yyyy HH:mm:ss}\n");

                                            text.Span($"Метод: {data.ClientSignatureMethod ?? "электронная подпись"}");
                                        });
                                    }

                                    if (data.DispatcherSignedAt.HasValue)
                                    {
                                        c.Item().PaddingTop(10).Text(text =>
                                        {
                                            text.DefaultTextStyle(x => x.FontSize(9));
                                            text.Span("Подписал (диспетчер): ").Bold();
                                            text.Span($"{data.DispatcherFio}\n");
                                            text.Span($"(Диспетчер, таб. №{data.DispatcherTabNumber})\n");
                                            text.Span($"Дата подписания: {data.DispatcherSignedAt:dd.MM.yyyy HH:mm:ss}\n");
                                            text.Span("Метод: электронная подпись диспетчера");
                                        });
                                    }

                                    // Footer блока
                                    c.Item().PaddingTop(10).Text(text =>
                                    {
                                        text.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium));
                                        text.Span("Документ, подписанный простой электронной подписью, ");
                                        text.Span("имеет юридическую силу в соответствии с ФЗ-63 ");
                                        text.Span("\"Об электронной подписи\"");
                                    });
                                });
                            }
                        });

                    // FOOTER
                    page.Footer()
                        .AlignCenter()
                        .PaddingVertical(10)
                        .Text(text =>
                        {
                            text.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Medium));
                            text.Span("Страница ");
                            text.CurrentPageNumber();
                            text.Span(" из ");
                            text.TotalPages();
                        });
                });
            }).GeneratePdf(filePath);

            _logger.LogInformation("✅ Акт выполненных работ сформирован: {Path}", filePath);
            return relativePath;
        }

        // 🔥 ИСПРАВЛЕНО: Генерация Акта списания ЗИП
        // 🔥 ИСПРАВЛЕНО: Генерация Акта списания ЗИП
        public async Task<string> GenerateSpareWriteOffAsync(SpareWriteOffData data)
        {
            var docsPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "documents", "spare-writeoffs");
            Directory.CreateDirectory(docsPath);

            var fileName = $"WriteOff_{data.TaskId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(docsPath, fileName);
            var relativePath = Path.Combine("documents", "spare-writeoffs", fileName).Replace("\\", "/");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // HEADER
                    page.Header()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.DefaultTextStyle(x => x.FontSize(14).Bold());
                            text.Span("АКТ СПИСАНИЯ ЗАПАСНЫХ ЧАСТЕЙ (ЗИП)\n");
                            text.Span($"№ {data.TaskId} от {data.CompletedAt:dd.MM.yyyy}").FontSize(12);
                        });

                    // CONTENT
                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(col =>
                        {
                            // Заказчик
                            col.Item().PaddingBottom(5).Text("Заказчик:").Bold();
                            col.Item().PaddingBottom(10).Text(data.ClientFio);

                            // Устройство
                            col.Item().PaddingBottom(5).Text("Устройство:").Bold();
                            col.Item().PaddingBottom(10).Text($"{data.DeviceModel} (S/N: {data.SerialNumber})");

                            // Таблица запчастей
                            col.Item().PaddingBottom(5).Text("СПИСАННЫЕ ЗАПЧАСТИ:").Bold().FontSize(12);
                            col.Item().PaddingBottom(10);

                            if (data.Spares.Any())
                            {
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    // Header
                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("№").Bold();
                                        header.Cell().Element(CellStyle).Text("Наименование").Bold();
                                        header.Cell().Element(CellStyle).Text("Кол-во").Bold();
                                        header.Cell().Element(CellStyle).Text("Цена").Bold();
                                        header.Cell().Element(CellStyle).Text("Сумма").Bold();
                                    });

                                    // Data
                                    int num = 1;
                                    foreach (var spare in data.Spares)
                                    {
                                        table.Cell().Element(CellStyle).Text(num.ToString());
                                        table.Cell().Element(CellStyle).Text(spare.SpareName);
                                        table.Cell().Element(CellStyle).Text(spare.Quantity.ToString());
                                        table.Cell().Element(CellStyle).Text($"{spare.UnitPrice:F2} ₽");
                                        table.Cell().Element(CellStyle).Text($"{spare.TotalPrice:F2} ₽");
                                        num++;
                                    }

                                    // Total
                                    table.Cell().ColumnSpan(4).Element(CellStyle).Text("ИТОГО:").Bold();
                                    table.Cell().Element(CellStyle).Text($"{data.Spares.Sum(s => s.TotalPrice):F2} ₽").Bold();
                                });
                            }
                            else
                            {
                                col.Item().PaddingBottom(10).Text("Запасные части не списаны.");
                            }

                            // Подписи
                            col.Item().PaddingTop(20).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("ИСПОЛНИТЕЛЬ:").Bold();
                                    c.Item().PaddingTop(20).Text("_________________ / Подпись");
                                });
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("ЗАКАЗЧИК:").Bold();
                                    c.Item().PaddingTop(20).Text("_________________ / Подпись");
                                });
                            });
                        });

                    // FOOTER
                    page.Footer()
                        .AlignCenter()
                        .PaddingVertical(10)
                        .Text(text =>
                        {
                            text.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Medium));
                            text.Span("Страница ");
                            text.CurrentPageNumber();
                            text.Span(" из ");
                            text.TotalPages();
                        });
                });
            }).GeneratePdf(filePath);

            _logger.LogInformation("✅ Акт списания ЗИП сформирован: {Path}", filePath);
            return relativePath;
        }

        public async Task<string> GenerateWarrantyCardAsync(WarrantyData data)
        {
            var docsPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "documents", "warranties");
            Directory.CreateDirectory(docsPath);

            var fileName = $"Warranty_{data.RequestId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(docsPath, fileName);
            var relativePath = Path.Combine("documents", "warranties", fileName).Replace("\\", "/");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Content().Column(col =>
                    {
                        // Заголовок
                        col.Item().PaddingBottom(5).AlignCenter().Text(text =>
                        {
                            text.DefaultTextStyle(x => x.FontSize(14).Bold());
                            text.Span("ГАРАНТИЙНЫЙ ТАЛОН");
                        });

                        col.Item().PaddingBottom(10).AlignCenter().Text($"№ {data.RequestId} от {data.RepairDate:dd.MM.yyyy}");

                        // Информация о клиенте и устройстве
                        col.Item().PaddingBottom(5).Text(text =>
                        {
                            text.DefaultTextStyle(x => x.Bold());
                            text.Span("Клиент:");
                        });

                        col.Item().PaddingBottom(5).Text(data.ClientFio);

                        col.Item().PaddingBottom(5).Text(text =>
                        {
                            text.DefaultTextStyle(x => x.Bold());
                            text.Span("Устройство:");
                        });

                        col.Item().PaddingBottom(5).Text($"{data.DeviceModel}");

                        col.Item().PaddingBottom(5).Text(text =>
                        {
                            text.DefaultTextStyle(x => x.Bold());
                            text.Span("Серийный номер:");
                        });

                        col.Item().PaddingBottom(10).Text(data.SerialNumber);

                        col.Item().PaddingBottom(5).Text(text =>
                        {
                            text.DefaultTextStyle(x => x.Bold());
                            text.Span("Дата ремонта:");
                        });

                        col.Item().PaddingBottom(10).Text($"{data.RepairDate:dd.MM.yyyy}");

                        // Гарантийный срок
                        col.Item().PaddingBottom(5).Text(text =>
                        {
                            text.DefaultTextStyle(x => x.FontSize(12).Bold());
                            text.Span($"Гарантийный срок: {data.WarrantyMonths} мес.");
                        });

                        col.Item().PaddingBottom(5).Text(text =>
                        {
                            text.DefaultTextStyle(x => x.FontSize(9));
                            text.Span("Гарантия распространяется на выполненные работы и установленные запчасти.");
                        });

                        col.Item().PaddingBottom(15).Text(text =>
                        {
                            text.DefaultTextStyle(x => x.FontSize(9));
                            text.Span("Гарантия не распространяется на механические повреждения и нарушения условий эксплуатации.");
                        });

                        // Подписи
                        // Подписи
                        col.Item().PaddingTop(15).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(); // одна колонка на всю ширину
                            });

                            table.Cell().AlignRight().Text("Печать сервиса");
                        });
                        // Валидно до
                        col.Item().PaddingTop(15).Text(text =>
                        {
                            text.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Medium));
                            text.Span($"Действителен до: {data.RepairDate.AddMonths(data.WarrantyMonths):dd.MM.yyyy}");
                        });
                    });                         

                    // Footer
                    page.Footer().AlignCenter().PaddingVertical(5).Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium));
                        text.Span("ООО \"Мастерская по ремонту СВТ\" | ");
                        text.Span("www.service-center.ru | ");
                        text.Span("+7 (495) 123-45-67");
                    });
                });
            }).GeneratePdf(filePath);

            _logger.LogInformation("✅ Гарантийный талон сформирован: {Path}", filePath);
            return relativePath;
        }

        // 🔥 Вспомогательный метод для стилей ячеек

        public async Task<string> GenerateInvoiceAsync2(int requestId, decimal amount, string outputPath)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

                // Получаем данные заявки
                var request = await _context.Requests
                    .Include(r => r.ClientEntity)
                    .FirstOrDefaultAsync(r => r.Id == requestId);

                if (request == null)
                    throw new Exception("Заявка не найдена");

                // Получаем акт диагностики для работ
                var diagnosticAct = await _context.DiagnosticActs
                    .Include(a => a.Task)
                    .FirstOrDefaultAsync(a => a.Task!.RequestId == requestId);

                // Получаем согласованные работы
                // Получаем согласованные работы
                var works = await _context.WorkDiagnosticActs
                    .Include(w => w.Work)
                    .Where(w => w.ActCode == diagnosticAct!.ActCode && w.IsApprovedByClient == true)
                    .ToListAsync();

                // Получаем ЗИП для примечания
                var spares = await _context.SparePartsDiagnosticActs
                    .Include(s => s.Spare)
                    .Where(s => s.ActCode == diagnosticAct.ActCode && s.IsApprovedByClient == true)
                    .ToListAsync();

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        // HEADER
                        page.Header()
                            .Element(e => e.Column(col =>
                            {
                                col.Item().Text("СЧЁТ НА ОПЛАТУ").FontSize(16).Bold().AlignCenter();
                                col.Item().Text($"№ INV-{requestId}/{DateTime.UtcNow:yyyy}").FontSize(12).AlignCenter();
                                col.Item().Text($"Дата: {DateTime.UtcNow:dd.MM.yyyy}").FontSize(11).AlignCenter();
                                col.Item().PaddingTop(10);
                            }));

                        // CONTENT
                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(col =>
                            {
                                // Реквизиты поставщика и заказчика
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(cols =>
                                    {
                                        cols.RelativeColumn();
                                        cols.RelativeColumn();
                                    });

                                    table.Cell().Element(CellStyle).Column(c =>
                                    {
                                        c.Item().Text("ПОСТАВЩИК:").Bold();
                                        c.Item().Text("ООО \"Мастерская по ремонту СВТ\"");
                                        c.Item().Text("ИНН: 7701234567");
                                        c.Item().Text("Адрес: г. Казань, ул. Примерная, д. 1");
                                        c.Item().Text("Тел: +7 (495) 123-45-67");
                                    });

                                    table.Cell().Element(CellStyle).Column(c =>
                                    {
                                        c.Item().Text("ЗАКАЗЧИК:").Bold();
                                        c.Item().Text(request.ClientEntity.User.FullName);
                                        c.Item().Text($"Тел: {request.ClientEntity.Phone}");
                                        c.Item().Text($"Заявка №: {requestId}");
                                    });
                                });

                                col.Item().PaddingTop(20);

                                // Предмет оплаты
                                col.Item().Text("ПРЕДМЕТ ОПЛАТЫ: Выполненные работы").Bold().FontSize(12);
                                col.Item().PaddingTop(10);

                                // Таблица работ
                                // Таблица работ
                                if (works.Any())
                                {
                                    col.Item().Table(table =>
                                    {
                                        table.ColumnsDefinition(cols =>
                                        {
                                            cols.RelativeColumn(0.5f);
                                            cols.RelativeColumn(3);
                                            cols.RelativeColumn(1);
                                            cols.RelativeColumn(1);
                                            cols.RelativeColumn(1.5f);
                                        });

                                        table.Header(header =>
                                        {
                                            header.Cell().Element(CellStyle).Text("№").Bold();
                                            header.Cell().Element(CellStyle).Text("Наименование").Bold();
                                            header.Cell().Element(CellStyle).Text("Кол-во").Bold();
                                            header.Cell().Element(CellStyle).Text("Ед.").Bold();
                                            header.Cell().Element(CellStyle).Text("Сумма, ₽").Bold();
                                        });

                                        int num = 1;
                                        foreach (var work in works)
                                        {
                                            table.Cell().Element(CellStyle).Text(num.ToString());
                                            table.Cell().Element(CellStyle).Text(work.Work?.WorkName ?? $"Работа #{work.WorkCode}");
                                            table.Cell().Element(CellStyle).Text("1");
                                            table.Cell().Element(CellStyle).Text("час");
                                            table.Cell().Element(CellStyle).AlignRight().Text($"{(work.Work?.PricePerHour ?? 0):F2}");
                                            num++;
                                        }

                                        // Итого
                                        table.Cell().ColumnSpan(4).Element(CellStyle).Text("ИТОГО К ОПЛАТЕ:").Bold().FontSize(12);
                                        table.Cell().Element(CellStyle).AlignRight().Text($"{amount:F2} ₽").Bold().FontSize(12);
                                    });
                                }
                                else
                                {
                                    col.Item().Text("Работы не найдены.");
                                }

                                col.Item().PaddingTop(20);

                                // Условия оплаты
                                col.Item().Text("УСЛОВИЯ ОПЛАТЫ:").Bold();
                                col.Item().Text("• Оплата работ производится после их выполнения");
                                col.Item().Text("• Срок оплаты: 7 банковских дней");

                                col.Item().PaddingTop(20);

                                // Реквизиты для оплаты
                                col.Item().Text("РЕКВИЗИТЫ ДЛЯ ОПЛАТЫ:").Bold();
                                col.Item().Text("Получатель: ООО \"Мастерская по ремонту СВТ\"");
                                col.Item().Text("ИНН: 7701234567");
                                col.Item().Text("Р/с: 40702810100000012345");
                                col.Item().Text("Банк: ПАО \"Банк\"");
                                col.Item().Text("К/с: 30101810400000000225");
                                col.Item().Text("БИК: 044525225");

                                var spareNote = spares.Any()
                                    ? $" (ЗИП оплачен отдельно)"
                                    : "";
                                col.Item().Text($"Назначение: Оплата за выполненные работы по заявке №{requestId}{spareNote}");

                                col.Item().PaddingTop(30);
                                col.Item().Text("Счёт действителен в течение 7 дней с даты выставления.").FontSize(9).FontColor(Colors.Grey.Medium);
                            });

                        // FOOTER
                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.DefaultTextStyle(t => t.FontSize(9).FontColor(Colors.Grey.Medium));
                                x.Span("Страница ");
                                x.CurrentPageNumber();
                                x.Span(" из ");
                                x.TotalPages();
                            });
                    });
                }).GeneratePdf(outputPath);

                _logger.LogInformation("✅ Счёт на оплату работ сформирован: {Path}", outputPath);
                return outputPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка генерации счёта на оплату работ");
                throw;
            }
        }

        public async Task<string> GenerateContractWithSignaturesAsync(ContractPdfData data)
        {
            var docsPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "documents", "contracts");
            Directory.CreateDirectory(docsPath);

            var fileName = $"Contract_{data.RequestId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}_signed.pdf";
            var filePath = Path.Combine(docsPath, fileName);
            var relativePath = Path.Combine("documents", "contracts", fileName).Replace("\\", "/");

            // 🔥 ЗАГРУЖАЕМ ВСЕ ДАННЫЕ ИЗ БД
            var requestData = await LoadContractDataAsync(data.RequestId);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // HEADER
                    page.Header()
                        .Element(e => e.Column(col =>
                        {
                            col.Item().Text("ДОГОВОР ОКАЗАНИЯ УСЛУГ ПО ТЕХНИЧЕСКОМУ ОБСЛУЖИВАНИЮ И РЕМОНТУ СРЕДСТВ ВЫЧИСЛИТЕЛЬНОЙ ТЕХНИКИ").FontSize(14).Bold().AlignCenter();
                            col.Item().Text($"№ {requestData.contractNumber ?? data.ContractNumber}").FontSize(12).AlignCenter();
                            col.Item().Text($"Дата: {requestData.createdAt:dd.MM.yyyy}").FontSize(11).AlignCenter();
                            col.Item().PaddingTop(10);
                        }));

                    // CONTENT - 🔥 ПОЛНЫЙ КОНТЕНТ КАК В GenerateContractAsync
                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(col =>
                        {
                            // 1. ПРЕАМБУЛА
                            SectionTitle(col.Item(), "1. ПРЕАМБУЛА");
                            col.Item().Text($"г. {requestData.city}, {requestData.createdAt:dd.MM.yyyy} г.");
                            col.Item().Text("");
                            col.Item().Text($"Гражданин(ка) {requestData.clientFio}, именуемый(ая) в дальнейшем «Заказчик», с одной стороны, и Индивидуальный предприниматель / Общество с ограниченной ответственностью \"{requestData.companyName}\", именуемый(ое) в дальнейшем «Исполнитель», с другой стороны, заключили настоящий договор о нижеследующем:");
                            col.Item().PaddingTop(10);

                            // 2. ПРЕДМЕТ ДОГОВОРА
                            SectionTitle(col.Item(), "2. ПРЕДМЕТ ДОГОВОРА");
                            col.Item().Text("2.1. Исполнитель обязуется оказать услуги по техническому обслуживанию и ремонту средств вычислительной техники (далее - СВТ), принадлежащих Заказчику, а Заказчик обязуется принять и оплатить эти услуги.");
                            col.Item().Text("2.2. Наименование СВТ: " + requestData.svtType);
                            col.Item().Text("2.3. Модель: " + requestData.model);
                            col.Item().Text("2.4. Серийный номер: " + requestData.serialNumber);
                            col.Item().Text("2.5. Описание неисправности: " + requestData.description);
                            col.Item().PaddingTop(10);

                            // 3. ПРАВА И ОБЯЗАННОСТИ СТОРОН
                            SectionTitle(col.Item(), "3. ПРАВА И ОБЯЗАННОСТИ СТОРОН");
                            col.Item().Text("3.1. Исполнитель обязуется:");
                            col.Item().Text("   - оказать услуги качественно и в срок;");
                            col.Item().Text("   - использовать качественные запасные части и материалы;");
                            col.Item().Text("   - предоставить гарантию на выполненные работы и установленные запчасти.");
                            col.Item().Text("3.2. Заказчик обязуется:");
                            col.Item().Text("   - предоставить СВТ в состоянии, позволяющем выполнить работы;");
                            col.Item().Text("   - оплатить услуги в порядке и сроки, установленные настоящим договором;");
                            col.Item().Text("   - принять выполненные работы по акту сдачи-приемки.");
                            col.Item().PaddingTop(10);

                            // 4. СТОИМОСТЬ УСЛУГ И ПОРЯДОК ОПЛАТЫ
                            SectionTitle(col.Item(), "4. СТОИМОСТЬ УСЛУГ И ПОРЯДОК ОПЛАТЫ");
                            col.Item().Text($"4.1. Стоимость согласованных работ составляет: {requestData.worksCost:F2} ({NumberToWords(requestData.worksCost)}) рублей.");
                            col.Item().Text($"4.2. Стоимость согласованных запасных частей: {requestData.sparesCost:F2} ({NumberToWords(requestData.sparesCost)}) рублей.");
                            col.Item().Text($"4.3. ОБЩАЯ СТОИМОСТЬ: {requestData.totalCost:F2} ({NumberToWords(requestData.totalCost)}) рублей.");
                            col.Item().Text("4.4. Порядок оплаты:");
                            col.Item().Text("   - Предоплата за запасные части (ЗИП) в размере 100% от их стоимости.");
                            col.Item().Text("   - Полная оплата работ и запчастей производится после выполнения работ.");
                            col.Item().PaddingTop(10);

                            // 5. ПЕРЕЧЕНЬ РАБОТ
                            SectionTitle(col.Item(), "5. ПЕРЕЧЕНЬ ВЫПОЛНЯЕМЫХ РАБОТ");
                            if (requestData.works.Any())
                            {
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(5);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("№").Bold();
                                        header.Cell().Element(CellStyle).Text("Наименование работы").Bold();
                                        header.Cell().Element(CellStyle).Text("Стоимость, ₽").Bold();
                                    });

                                    int workNum = 1;
                                    foreach (var work in requestData.works)
                                    {
                                        table.Cell().Element(CellStyle).Text(workNum.ToString());
                                        table.Cell().Element(CellStyle).Text(work.workName);
                                        table.Cell().Element(CellStyle).AlignRight().Text($"{work.estimatedCost:F2}");
                                        workNum++;
                                    }

                                    table.Cell().ColumnSpan(2).Element(CellStyle).Text("Итого за работы:").Bold();
                                    table.Cell().Element(CellStyle).AlignRight().Text($"{requestData.worksCost:F2}").Bold();
                                });
                            }
                            else
                            {
                                col.Item().Text("Работы не предусмотрены.");
                            }
                            col.Item().PaddingTop(10);

                            // 6. ПЕРЕЧЕНЬ ЗИП
                            SectionTitle(col.Item(), "6. ПЕРЕЧЕНЬ ЗАПАСНЫХ ЧАСТЕЙ (ЗИП)");
                            if (requestData.spares.Any())
                            {
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("№").Bold();
                                        header.Cell().Element(CellStyle).Text("Наименование").Bold();
                                        header.Cell().Element(CellStyle).Text("Кол-во").Bold();
                                        header.Cell().Element(CellStyle).Text("Ед. изм.").Bold();
                                        header.Cell().Element(CellStyle).Text("Стоимость, ₽").Bold();
                                    });

                                    int spareNum = 1;
                                    foreach (var spare in requestData.spares)
                                    {
                                        table.Cell().Element(CellStyle).Text(spareNum.ToString());
                                        table.Cell().Element(CellStyle).Text(spare.spareName);
                                        table.Cell().Element(CellStyle).Text(spare.quantity.ToString());
                                        table.Cell().Element(CellStyle).Text(spare.unit);
                                        table.Cell().Element(CellStyle).AlignRight().Text($"{spare.totalPrice:F2}");
                                        spareNum++;
                                    }

                                    table.Cell().ColumnSpan(4).Element(CellStyle).Text("Итого за запчасти:").Bold();
                                    table.Cell().Element(CellStyle).AlignRight().Text($"{requestData.sparesCost:F2}").Bold();
                                });
                            }
                            else
                            {
                                col.Item().Text("Запасные части не требуются.");
                            }
                            col.Item().PaddingTop(10);

                            // 7. СРОКИ ВЫПОЛНЕНИЯ РАБОТ
                            SectionTitle(col.Item(), "7. СРОКИ ВЫПОЛНЕНИЯ РАБОТ");
                            col.Item().Text($"7.1. Срок выполнения работ: {requestData.estimatedTime ?? "не установлен"}.");
                            col.Item().Text("7.2. Срок исчисляется с момента поступления предоплаты на расчетный счет Исполнителя.");
                            col.Item().PaddingTop(10);

                            // 8. ГАРАНТИЙНЫЕ ОБЯЗАТЕЛЬСТВА
                            SectionTitle(col.Item(), "8. ГАРАНТИЙНЫЕ ОБЯЗАТЕЛЬСТВА");
                            col.Item().Text("8.1. Исполнитель предоставляет гарантию на выполненные работы сроком на 3 (три) месяца.");
                            col.Item().Text("8.2. Гарантия на установленные запасные части предоставляется в соответствии с гарантийными обязательствами производителя.");
                            col.Item().PaddingTop(10);

                            // 9. ОТВЕТСТВЕННОСТЬ СТОРОН
                            SectionTitle(col.Item(), "9. ОТВЕТСТВЕННОСТЬ СТОРОН");
                            col.Item().Text("9.1. За неисполнение или ненадлежащее исполнение обязательств по настоящему договору Стороны несут ответственность в соответствии с действующим законодательством Российской Федерации.");
                            col.Item().PaddingTop(120);

                            // 10. ПОДПИСИ СТОРОН С ВИЗУАЛЬНЫМИ ОТМЕТКАМИ
                            SectionTitle(col.Item(), "10. ПОДПИСИ СТОРОН");
                            col.Item().PaddingTop(10);

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                // Клиент
                                table.Cell().Border(1).Padding(10).Column(c =>
                                {
                                    c.Item().Text("ЗАКАЗЧИК:").Bold();
                                    c.Item().PaddingTop(30);

                                    if (data.ClientSignedAt.HasValue)
                                    {
                                        c.Item().Text("✓ Подписано электронной подписью").FontSize(9).FontColor(Colors.Green.Medium);
                                        c.Item().Text($"Дата: {data.ClientSignedAt:dd.MM.yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
                                        if (!string.IsNullOrEmpty(data.ClientSignatureMethod))
                                            c.Item().Text($"Метод: {data.ClientSignatureMethod}").FontSize(8).FontColor(Colors.Grey.Medium);
                                    }
                                    else
                                    {
                                        c.Item().Text("_________________").FontSize(11);
                                        c.Item().Text("(подпись)").FontSize(9);
                                    }

                                    c.Item().PaddingTop(5).Text(requestData.clientFio).FontSize(10);
                                    c.Item().Text("(расшифровка)").FontSize(9);
                                });

                                // Диспетчер
                                table.Cell().Border(1).Padding(10).Column(c =>
                                {
                                    c.Item().Text("ИСПОЛНИТЕЛЬ:").Bold();
                                    c.Item().PaddingTop(30);

                                    if (data.DispatcherSignedAt.HasValue)
                                    {
                                        c.Item().Text("✓ Подписано электронной подписью").FontSize(9).FontColor(Colors.Green.Medium);
                                        c.Item().Text($"Дата: {data.DispatcherSignedAt:dd.MM.yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);

                                        // 🔥 ДОБАВЛЕНО: Отображение метода подписания для диспетчера
                                        if (!string.IsNullOrEmpty(data.DispatcherSignatureMethod))
                                            c.Item().Text($"Метод: {data.DispatcherSignatureMethod}").FontSize(8).FontColor(Colors.Grey.Medium);

                                        c.Item().Text($"Диспетчер: {data.DispatcherFio}").FontSize(9);
                                        if (data.DispatcherTabNumber.HasValue)
                                            c.Item().Text($"Таб. №: {data.DispatcherTabNumber}").FontSize(8);
                                    }
                                    else
                                    {
                                        c.Item().Text("_________________").FontSize(11);
                                        c.Item().Text("(подпись)").FontSize(9);
                                    }

                                    c.Item().PaddingTop(5).Text(data.DispatcherFio).FontSize(10);
                                    c.Item().Text("(расшифровка)").FontSize(9);
                                });
                            });

                            // 🔥 БЛОК С ИНФОРМАЦИЕЙ ОБ ЭЛЕКТРОННЫХ ПОДПИСЯХ
                            if (data.ClientSignedAt.HasValue || data.DispatcherSignedAt.HasValue)
                            {
                                col.Item().PaddingTop(20).Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4).Padding(10).Column(c =>
                                {
                                    c.Item().AlignCenter().Text(text =>
                                    {
                                        text.DefaultTextStyle(x => x.FontSize(10).Bold().FontColor(Colors.Grey.Darken2));
                                        text.Span("ДОКУМЕНТ ПОДПИСАН\n");
                                        text.Span("ПРОСТОЙ ЭЛЕКТРОННОЙ ПОДПИСЬЮ");
                                    });

                                    c.Item().PaddingTop(10);

                                    if (data.ClientSignedAt.HasValue)
                                    {
                                        c.Item().Text(text =>
                                        {
                                            text.DefaultTextStyle(x => x.FontSize(9));
                                            text.Span("Подписал (клиент): ").Bold();
                                            text.Span($"{requestData.clientFio}\n");
                                            text.Span($"Дата: {data.ClientSignedAt:dd.MM.yyyy HH:mm:ss}\n");
                                            text.Span($"Метод: {data.ClientSignatureMethod ?? "click"}");
                                            if (!string.IsNullOrEmpty(data.ClientIp))
                                                text.Span($"\nIP: {data.ClientIp}");
                                        });
                                    }

                                    if (data.DispatcherSignedAt.HasValue)
                                    {
                                        c.Item().PaddingTop(10).Text(text =>
                                        {
                                            text.DefaultTextStyle(x => x.FontSize(9));
                                            text.Span("Подписал (диспетчер): ").Bold();
                                            text.Span($"{data.DispatcherFio}\n");
                                            if (data.DispatcherTabNumber.HasValue)
                                                text.Span($"Таб. №: {data.DispatcherTabNumber}\n");
                                            text.Span($"Дата: {data.DispatcherSignedAt:dd.MM.yyyy HH:mm:ss}\n");

                                            // 🔥 ДОБАВЛЕНО: Метод подписания диспетчера
                                            if (!string.IsNullOrEmpty(data.DispatcherSignatureMethod))
                                                text.Span($"Метод: {data.DispatcherSignatureMethod}\n");
                                        });
                                    }

                                    c.Item().PaddingTop(10).Text(text =>
                                    {
                                        text.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium));
                                        text.Span("Документ имеет юридическую силу в соответствии с ФЗ-63 \"Об электронной подписи\"");
                                    });
                                });
                            }
                        });

                    // FOOTER
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.DefaultTextStyle(t => t.FontSize(9).FontColor(Colors.Grey.Medium));
                            x.Span("Страница ");
                            x.CurrentPageNumber();
                            x.Span(" из ");
                            x.TotalPages();
                        });
                });
            }).GeneratePdf(filePath);

            _logger.LogInformation("✅ Договор с подписями сформирован: {Path}", filePath);
            return relativePath;
        }

        public async Task GenerateAccountingReportAsync(AccountingReportData data, string outputPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(TextStyle.Default.FontSize(12));

                    // HEADER - объединяем все элементы в один вызов через Column
                    page.Header()
                        .Element(header => header.Column(col =>
                        {
                            col.Item().AlignCenter().Text("Отчёт для бухгалтерии").Bold().FontSize(18);
                            col.Item().AlignCenter().Text($"Заявка №{data.RequestId} от {data.GeneratedAt:dd.MM.yyyy}");
                            col.Item().PaddingTop(10).LineHorizontal(1);
                        }));

                    // CONTENT
                    page.Content().Column(col =>
                    {
                        // Блок персонала
                        col.Item().Text("Ответственный персонал:").Bold();
                        col.Item().Text($"Диспетчер: {data.DispatcherFio}");
                        col.Item().Text($"Сервисные инженеры: {string.Join(", ", data.EngineerFios)}");
                        col.Item().PaddingTop(10).LineHorizontal(0.5f);

                        // Блок работ
                        col.Item().PaddingTop(10).Text("Выполненные работы:").Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Наименование").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Стоимость").Bold();
                            });

                            foreach (var work in data.Works)
                            {
                                table.Cell().Element(CellStyle).Text(work.WorkName);
                                table.Cell().Element(CellStyle).AlignRight().Text($"{work.Cost:F2} ₽");
                            }

                            // Итого за работы
                            table.Cell().Element(CellStyle).Text("Итого за работы:").Bold();
                            table.Cell().Element(CellStyle).AlignRight().Text($"{data.TotalWorksCost:F2} ₽").Bold();
                        });

                        // Блок ЗИП
                        // Блок ЗИП
                        col.Item().PaddingTop(20).Text("Списанные запчасти (ЗИП):").Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);   // Наименование
                                columns.RelativeColumn(1);   // Кол-во
                                columns.RelativeColumn((float)1.2); // Цена закупки
                                columns.RelativeColumn((float)1.2); // Цена реализации
                                columns.RelativeColumn((float)1.2); // Прибыль
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Наименование").Bold();
                                header.Cell().Element(CellStyle).AlignCenter().Text("Кол-во").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Закупка, ₽").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Реализация, ₽").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Прибыль, ₽").Bold();
                            });

                            foreach (var spare in data.Spares)
                            {
                                table.Cell().Element(CellStyle).Text(spare.SpareName);
                                table.Cell().Element(CellStyle).AlignCenter().Text(spare.Quantity.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text($"{(spare.UnitPrice * spare.Quantity):F2}");
                                table.Cell().Element(CellStyle).AlignRight().Text($"{(spare.SellingPrice * spare.Quantity):F2}");
                                table.Cell().Element(CellStyle).AlignRight().Text($"{spare.Profit:F2}");
                            }

                            // Итого
                            table.Cell().Element(CellStyle).Text("Итого за ЗИП:").Bold();
                            table.Cell().Element(CellStyle).AlignCenter().Text(" ");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{data.TotalSparesCost:F2}").Bold();
                            table.Cell().Element(CellStyle).AlignRight().Text($"{data.Spares.Sum(s => s.TotalSellingPrice):F2}").Bold();
                            table.Cell().Element(CellStyle).AlignRight().Text($"{data.TotalSpareProfit:F2}").Bold()
                                   .FontColor(data.TotalSpareProfit >= 0 ? Colors.Green.Medium : Colors.Red.Medium);
                        });

                        // Итого общая сумма
                        col.Item().PaddingTop(20).AlignRight().Column(c =>
                        {
                            c.Item().Text($"Общая сумма работ: {data.TotalWorksCost:F2} ₽");
                            c.Item().Text($"Общая сумма закупки ЗИП: {data.TotalSparesCost:F2} ₽");
                            c.Item().Text($"Общая сумма реализации ЗИП: {data.Spares.Sum(s => s.TotalSellingPrice):F2} ₽");
                            c.Item().PaddingTop(5)
                                .Text($"Прибыль по ЗИП: {data.TotalSpareProfit:F2} ₽")
                                .FontColor(data.TotalSpareProfit >= 0 ? Colors.Green.Medium : Colors.Red.Medium);
                            c.Item().PaddingTop(10)
                                .Text($"ВСЕГО К ОПЛАТЕ: {data.TotalWorksCost + data.TotalSparesCost:F2} ₽")
                                .Bold()
                                .FontSize(16)
                                .FontColor(Colors.Blue.Medium);
                        });
                    });

                    // FOOTER
                    page.Footer()
                        .AlignCenter()
                        .PaddingVertical(10)
                        .Text(x =>
                        {
                            x.DefaultTextStyle(t => t.FontSize(9).FontColor(Colors.Grey.Medium));
                            x.Span("Страница ");
                            x.CurrentPageNumber();
                            x.Span(" из ");
                            x.TotalPages();
                        });
                });
            });

            document.GeneratePdf(outputPath);
            _logger.LogInformation("✅ Отчёт для бухгалтерии сформирован: {Path}", outputPath);
        }

        // Класс для данных счёта
        private class InvoiceData
        {
            public string clientFio { get; set; } = string.Empty;
            public string clientPhone { get; set; } = string.Empty;
            public List<SpareInfo> spares { get; set; } = new();
            public decimal totalAmount { get; set; }
            public string companyName { get; set; } = string.Empty;
            public string companyInn { get; set; } = string.Empty;
            public string companyAddress { get; set; } = string.Empty;
            public string companyPhone { get; set; } = string.Empty;
            public string bankAccount { get; set; } = string.Empty;
            public string bankName { get; set; } = string.Empty;
            public string bankCorrespondent { get; set; } = string.Empty;
            public string bankBik { get; set; } = string.Empty;
        }

        // Класс для данных счёта на оплату работ (финальный платёж)
        private class InvoiceData2
        {
            // 📋 Информация о заявке
            public int RequestId { get; set; }
            public string DeviceModel { get; set; } = string.Empty;
            public string SerialNumber { get; set; } = string.Empty;
            public string FaultDescription { get; set; } = string.Empty;

            // 👤 Клиент
            public string ClientFio { get; set; } = string.Empty;
            public string ClientPhone { get; set; } = string.Empty;
            public string ClientAddress { get; set; } = string.Empty;

            // 🔧 Выполненные работы (вместо ЗИП!)
            public List<WorkInfo> Works { get; set; } = new();

            // 💰 Финансы
            public decimal WorksTotal { get; set; }        // Сумма за работы
            public decimal PrepaidAmount { get; set; }     // Уже оплачено за ЗИП
            public decimal TotalAmount { get; set; }       // Итого к оплате (только работы!)
            public string Currency { get; set; } = "RUB";

            // 📄 Реквизиты счёта
            public string InvoiceNumber { get; set; } = string.Empty;
            public DateTime InvoiceDate { get; set; }
            public DateTime PaymentDeadline { get; set; }

            // 🏢 Данные компании-исполнителя
            public string CompanyName { get; set; } = string.Empty;
            public string CompanyInn { get; set; } = string.Empty;
            public string CompanyKpp { get; set; } = string.Empty;
            public string CompanyAddress { get; set; } = string.Empty;
            public string CompanyPhone { get; set; } = string.Empty;
            public string CompanyEmail { get; set; } = string.Empty;

            // 🏦 Банковские реквизиты
            public string BankAccount { get; set; } = string.Empty;
            public string BankName { get; set; } = string.Empty;
            public string BankCorrespondent { get; set; } = string.Empty;
            public string BankBik { get; set; } = string.Empty;

            // 📝 Дополнительно
            public string ActReference { get; set; } = string.Empty;  // № акта выполненных работ
            public string Note { get; set; } = string.Empty;           // Примечание
            public string Footer { get; set; } = "Счёт действителен в течение 7 дней";
        }
        //DTO для договора 
        private class ContractData
        {
            public string contractNumber { get; set; } = string.Empty;  
            public string city { get; set; } = string.Empty;
            public string clientFio { get; set; } = string.Empty;
            public string clientPhone { get; set; } = string.Empty;
            public string? clientAddress { get; set; }
            public string svtType { get; set; } = string.Empty;
            public string model { get; set; } = string.Empty;
            public string serialNumber { get; set; } = string.Empty;
            public string description { get; set; } = string.Empty;
            public List<WorkInfo> works { get; set; } = new();
            public List<SpareInfo> spares { get; set; } = new();
            public decimal worksCost { get; set; }
            public decimal sparesCost { get; set; }
            public decimal totalCost { get; set; }
            public string? estimatedTime { get; set; }
            public string companyName { get; set; } = string.Empty;
            public string? companyInn { get; set; }
            public string? companyOgrn { get; set; }
            public string? companyAddress { get; set; }
            public string? companyPhone { get; set; }

            public DateTime createdAt { get; set; }  // 🔥 Добавьте это
        }
        private class WorkInfo
        {
            public string workName { get; set; } = string.Empty;
            public decimal estimatedCost { get; set; }
        }
        private class SpareInfo
        {
            public string spareName { get; set; } = string.Empty;
            public int quantity { get; set; }
            public string unit { get; set; } = string.Empty;
            public decimal totalPrice { get; set; }
        }

        public class SparePartsService
        {
            // Базовые цены для примера (можно заменить на реальные данные из БД или прайса)
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
}