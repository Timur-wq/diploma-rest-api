using Diploma.Core.Models;
using Diploma.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Xml;

namespace Diploma.DataAccess
{
    public class DiplomaDbContext : DbContext
    {
        public DiplomaDbContext(DbContextOptions<DiplomaDbContext> options) : base(options)
        {
            
        }

        public DbSet<RequestEntity> Requests { get; set; }
        public DbSet<ClientEntity> Clients { get; set; }
        public DbSet<StafEntity> Stafs { get; set; }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<SpareEntity> Spares { get; set; }
        public DbSet<DiagnosticAct> DiagnosticActs { get; set; }
        public DbSet<WorkEntity> WorkEntities { get; set; }
        public DbSet<WorkClassEntity> WorkClassEntities { get; set; }
        public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }

        public DbSet<SparePartsDiagnosticAct> SparePartsDiagnosticActs { get; set; }
        public DbSet<WorksDiagnosticAct> WorkDiagnosticActs { get; set; }

        public DbSet<CertificateEntity> Certificates { get; set; }

        public DbSet<StafCertificate> StafCertificates { get; set; }

        public DbSet<WorkClassAccessEntity> WorkClassAccessEntities { get; set; }

        public DbSet<TaskEntity> TaskEntities { get; set; }

        public DbSet<ModelEntity> ModelEntities { get; set; }

        public DbSet<CertificateModelEntity> CertificateModels { get; set; }

        public DbSet<WorkSpareRelationInAct> WorkSpareRelationsInAct { get; set; } 
        public DbSet<SupplierEntity> SupplierEntities { get; set; }
        public DbSet<SupplierSpareEntity> SupplierSpares { get; set; }
        public DbSet<SpareOrderEntity> SpareOrders { get; set; }
        public DbSet<ContractEntity> Contracts { get; set; }
        public DbSet<InvoiceEntity> Invoices { get; set; }
        public DbSet<SparePartsWriteOffEntity> SparePartsWriteOffEntities { get; set; }
        public DbSet<GoodsReceiptEntity> GoodsReceipts { get; set; }
        public DbSet<RepairActEntity> RepairActs { get; set; }

        public DbSet<WarrantyCardEntity> WarrantyCards { get; set; }

        public DbSet<SignatureEntity> Signatures { get; set; }
        public DbSet<ReportEntity> ReportEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RepairActEntity>(entity =>
            {
                // ... ваши существующие конфигурации ...

                // 🔥 Явно игнорируем свойства-обёртки
                entity.Ignore(e => e.IsSignedByClient);
                entity.Ignore(e => e.SignedAt);
                entity.Ignore(e => e.SignatureMethod);
                entity.Ignore(e => e.ClientVerificationData);
                entity.Ignore(e => e.IsSignedByDispatcher);
                entity.Ignore(e => e.DispatcherSignedAt);
                entity.Ignore(e => e.DispatcherSignedByTabNumber);
                entity.Ignore(e => e.DispatcherVerificationData);
                entity.Ignore(e => e.IsVerifiedByDispatcher);
            });

            // 🔥 НОВОЕ: Уникальное ограничение для заявок на ЗИП
            modelBuilder.Entity<SpareOrderEntity>()
                .HasIndex(o => new { o.SupplierSpareId, o.Status, o.ActCode })
                .IsUnique();

            // 🔥 НОВОЕ: Связь GoodsReceipt → SpareOrder
            modelBuilder.Entity<GoodsReceiptEntity>()
                .HasOne(gr => gr.SpareOrder)
                .WithMany()
                .HasForeignKey(gr => gr.SpareOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔥 НОВОЕ: Связь SpareOrder → GoodsReceipt
            modelBuilder.Entity<SpareOrderEntity>()
                .HasOne(so => so.GoodsReceipt)
                .WithMany()
                .HasForeignKey(so => so.GoodsReceiptId)
                .OnDelete(DeleteBehavior.SetNull);
            base.OnModelCreating(modelBuilder);

            // 🔥 Явно указываем тип для проблемных колонок
            //modelBuilder.Entity<StafEntity>()
            //    .Property(e => e.UserId)
            //    .HasColumnType("integer");  // или "character varying", если так в БД

            //modelBuilder.Entity<ClientEntity>()
            //    .Property(e => e.UserId)
            //    .HasColumnType("integer");
            // Настройка таблицы "Клиенты"
            //modelBuilder.Entity<ClientEntity>(entity =>
            //{
            //    entity.ToTable("Клиенты");
            //    entity.HasKey(e => e.Id).HasName("Код клиента");
            //    entity.Property(e => e.Id).HasColumnName("Код клиента");
            //    entity.Property(e => e.FIO)
            //        .HasColumnName("ФИО клиента")
            //        .HasColumnType("character varying(50)")
            //        .HasMaxLength(50);
            //    entity.Property(e => e.Phone)
            //        .HasColumnName("Контактный телефон клиента")
            //        .HasColumnType("character(18)")
            //        .HasMaxLength(18);
            //});

            //modelBuilder.Entity<StafEntity>(entity =>
            //{
            //    entity.ToTable("Сотрудники");
            //    entity.HasKey(e => e.TabNum).HasName("Табельный номер");
            //    entity.Property(e => e.TabNum).HasColumnName("Табельный номер");

            //    entity.Property(e => e.User.FullName)
            //        .HasColumnName("ФИО сотрудника")
            //        .HasColumnType("character varying(100)")
            //        .HasMaxLength(100);

            //    entity.Property(e => e.Role)
            //        .HasColumnName("Роль")
            //        .HasConversion<string>()  // Храним как строку в БД
            //        .HasMaxLength(50);

            //    // Индекс для поиска по роли
            //    entity.HasIndex(e => e.Role);
            //});

            modelBuilder.Entity<StafEntity>(entity =>
            {
                entity.HasKey(e => e.TabNum);
                entity.ToTable("Сотрудники");

                entity.Property(e => e.TabNum).HasColumnName("Табельный номер");
                entity.Property(e => e.DateStartJob).HasColumnName("Дата приёма в штат");
                entity.Property(e => e.UserId).HasColumnName("Id пользователя");

                // 🔥 Исправление для enum
                entity.Property(e => e.Role)
                    .HasConversion<string>()
                    .HasColumnName("Роль");

                entity.HasOne(e => e.User)
                    .WithMany(u => u.StafEntities)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Настройка таблицы "Заявки на ТО СВТ"
            modelBuilder.Entity<RequestEntity>(entity =>
            {
                entity.ToTable("Заявки на ТО СВТ");
                entity.HasKey(e => e.Id).HasName("Код заявки");
                entity.Property(e => e.Id).HasColumnName("Код заявки");

                entity.Property(e => e.ClientId)
                    .HasColumnName("Код клиента");

                entity.Property(e => e.DateOnly)
                    .HasColumnName("Дата поступления заявки")
                    .HasColumnType("date");

                entity.Property(e => e.TimeOnly)
                    .HasColumnName("Время поступления заявки")
                    .HasColumnType("time");

                entity.Property(e => e.SerialNumber)
                    .HasColumnName("Серийный номер СВТ")
                    .HasColumnType("character varying(20)")
                    .HasMaxLength(20);

                entity.Property(e => e.SvtType)
                    .HasColumnName("Тип СВТ")
                    .HasColumnType("character varying(20)")
                    .HasMaxLength(20);

                entity.Property(e => e.Model)
                .HasColumnName("Модель")
                .HasColumnType("character varying(30)")
                .HasMaxLength(30);

                entity.Property(e => e.Description)
                    .HasColumnName("Описание неисправности")
                    .HasColumnType("character varying(2000)")
                    .HasMaxLength(2000);
                entity.Property(e => e.Status)
                    .HasColumnName("Статус")
                    .HasConversion<string>()
                    .HasMaxLength(20);
                entity.Property(e => e.RejectionReason)
                    .HasColumnName("Причина отклонения")
                    .HasColumnType("character varying(500)")
                    .HasMaxLength(500);
                // Связь с ClientEntity
                entity.HasOne(e => e.ClientEntity)
                    .WithMany(c => c.requestEntities)
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            //// Настройка таблицы RefreshTokens
            //modelBuilder.Entity<RefreshTokenEntity>(entity =>
            //{
            //    entity.ToTable("Refresh-токены");
            //    entity.HasKey(e => e.Id);

            //    entity.Property(e => e.Token)
            //        .HasColumnName("Токен")
            //        .HasColumnType("character varying(256)")
            //        .HasMaxLength(256)
            //        .IsRequired();

            //    entity.Property(e => e.ExpiresAt)
            //        .HasColumnName("Истекает")
            //        .IsRequired();

            //    entity.Property(e => e.CreatedAt)
            //        .HasColumnName("Создан")
            //        .HasDefaultValueSql("CURRENT_TIMESTAMP");

            //    entity.Property(e => e.IsUsed)
            //        .HasColumnName("Использован")
            //        .HasDefaultValue(false);

            //    entity.Property(e => e.IsRevoked)
            //        .HasColumnName("Отозван")
            //        .HasDefaultValue(false);

            //    entity.Property(e => e.CreatedByIp)
            //        .HasColumnName("IP адрес")
            //        .HasColumnType("character varying(45)")
            //        .HasMaxLength(45);

            //    entity.Property(e => e.CreatedByUserAgent)
            //        .HasColumnName("User агент")
            //        .HasColumnType("character varying(500)")
            //        .HasMaxLength(500);

            //    // Индексы для быстрого поиска
            //    entity.HasIndex(e => e.Token).IsUnique();  // Уникальный токен
            //    entity.HasIndex(e => new { e.StafId, e.IsUsed, e.IsRevoked });  // Для поиска активных

            //    // Связь с Staf
            //    entity.HasOne(e => e.User)
            //        .WithMany(u => u.RefreshTokens)
            //        .HasForeignKey(e => e.StafId)
            //        .OnDelete(DeleteBehavior.Cascade);  // При удалении пользователя удаляются токены
            //});

            ////Акт диагностики
            //modelBuilder.Entity<DiagnosticAct>(entity =>
            //{
            //    entity.ToTable("Акт диагностики");
            //    entity.HasKey(e => e.ActCode).HasName("Код акта");
            //    entity.Property(e => e.ActCode).HasColumnName("Код акта");
            //    entity.Property(e => e.DateOnly)
            //        .HasColumnName("Дата диагностики")
            //        .HasColumnType("date");
            //    // Связь с Staf
            //    entity.HasOne(e => e.StafEntity)
            //        .WithMany(u => u.diagnosticActs)
            //        .HasForeignKey(e => e.ActCode)
            //        .OnDelete(DeleteBehavior.Cascade);
            //    // Связь с Request
            //    entity.HasOne(e => e.RequestEntity)
            //        .WithMany(u => u.diagnosticActs)
            //        .HasForeignKey(e => e.ReqCode)
            //        .OnDelete(DeleteBehavior.Cascade);

            //});

            ////Учёт ЗИП
            //modelBuilder.Entity<SpareEntity>(entity =>
            //{
            //    entity.ToTable("Учёт ЗИП");
            //    entity.HasKey(e => e.SpareCode).HasName("Код ЗИП");
            //    entity.Property(e => e.SpareCode).HasColumnName("Код ЗИП");
            //    entity.Property(e => e.SpareName)
            //        .HasColumnName("Наименование ЗИП")
            //        .HasColumnType("character varying(50)")
            //        .HasMaxLength(50);
            //    entity.Property(e => e.Amount)
            //        .HasColumnName("Количество на складе");

            //});

            ////Классификатор работ
            //modelBuilder.Entity<WorkClassEntity>(entity =>
            //{
            //    entity.ToTable("Классификатор работ");
            //    entity.HasKey(e => e.WorkClassCode).HasName("Код класса работ");
            //    entity.Property(e => e.WorkClassCode).HasColumnName("Код класса работ");
            //    entity.Property(e => e.WorkClassName)
            //        .HasColumnName("Наименование класса работ")
            //        .HasColumnType("character varying(20)")
            //        .HasMaxLength(20);

            //});

            ////Кодификатор работ
            //modelBuilder.Entity<WorkEntity>(entity =>
            //{
            //    entity.ToTable("Кодификатор работ");
            //    entity.HasKey(e => e.WorkCode).HasName("Код работы");
            //    entity.Property(e => e.WorkCode).HasColumnName("Код работы");

            //    entity.Property(e => e.WorkName)
            //        .HasColumnName("Наименование работы")
            //        .HasColumnType("character varying(20)")
            //        .HasMaxLength(20);
            //    // Связь с Классификатором работ
            //    entity.HasOne(e => e.WorkClass)
            //        .WithMany(u => u.workEntities)
            //        .HasForeignKey(e => e.WorkClassCode)
            //        .OnDelete(DeleteBehavior.Cascade);
            //    entity.Property(e => e.PricePerHour)
            //        .HasColumnName("Стоимость работы в час");

            //});

            //modelBuilder.Entity<SparePartsDiagnosticAct>(entity =>
            //{
            //    entity.ToTable("Перечень ЗИП - Акт диагностики");
            //    entity.HasKey(spd => new { spd.SpareCode, spd.ActCode });
            //    entity.Property(e => e.Amount)
            //        .HasColumnName("Количество");

            //    entity.HasOne(spd => spd.Spare)
            //        .WithMany()
            //        .HasForeignKey(spd => spd.SpareCode)
            //        .OnDelete(DeleteBehavior.Cascade);

            //    entity.HasOne(spd => spd.DiagnosticAct)
            //        .WithMany()
            //        .HasForeignKey(spd => spd.ActCode)
            //        .OnDelete(DeleteBehavior.Cascade);
            //});

            //modelBuilder.Entity<WorksDiagnosticAtc>(entity =>
            //{
            //    entity.ToTable("Перечень работ - Акт диагностики");
            //    entity.HasKey(spd => new { spd.WorkCode, spd.ActCode });

            //    entity.HasOne(e => e.Work)
            //        .WithMany()
            //        .HasForeignKey(e => e.WorkCode)
            //        .OnDelete(DeleteBehavior.Cascade);

            //    entity.HasOne(e => e.DiagnosticAct)
            //        .WithMany()
            //        .HasForeignKey(e => e.ActCode)
            //        .OnDelete(DeleteBehavior.Cascade);
            //});

            //// === Сертификаты ===
            //modelBuilder.Entity<CertificateEntity>(entity =>
            //{
            //    entity.HasKey(e => e.CertificateId).HasName("Код сертификата");
            //    entity.Property(e => e.CertificateId).HasColumnName("Код сертификата"); ;

            //    entity.Property(e => e.Name)
            //        .HasColumnName("Наименование")
            //        .IsRequired()
            //        .HasMaxLength(300);

            //    entity.Property(e => e.Issuer)
            //        .HasColumnName("Эмитент")
            //        .IsRequired()
            //        .HasMaxLength(200);

            //    entity.Property(e => e.Type)
            //        .HasColumnName("Тип")
            //        .IsRequired()
            //        .HasMaxLength(50)
            //        .HasDefaultValue("vendor");

            //    entity.Property(e => e.Category)
            //        .HasColumnName("Категория")
            //        .HasMaxLength(100);


            //    entity.Property(e => e.RequiresRenewal)
            //        .HasColumnName("Необходимость продления")
            //        .HasDefaultValue(true);

            //    entity.Property(e => e.IsActive)
            //        .HasColumnName("Активен")
            //        .HasDefaultValue(true);

            //});

            //modelBuilder.Entity<StafCertificate>(entity =>
            //{
            //    entity.HasKey(e => e.Id);  // ✅ Суррогатный ключ


            //    // Внешние ключи
            //    entity.HasOne(e => e.Staf)
            //        .WithMany()
            //        .HasForeignKey(e => e.StafId)
            //        .OnDelete(DeleteBehavior.Cascade);

            //    entity.HasOne(e => e.Certificate)
            //        .WithMany()
            //        .HasForeignKey(e => e.CertificateId)
            //        .OnDelete(DeleteBehavior.Cascade);
            //});

            //// === WorkClassAccessEntity ===
            //modelBuilder.Entity<WorkClassAccessEntity>(entity =>
            //{
            //    entity.HasKey(e => new { e.WorkClassCode, e.StafCertificateId });

            //    entity.Property(e => e.WorkClassCode).HasColumnName("Код класса работ");
            //    entity.Property(e => e.StafCertificateId).HasColumnName("Код записи сертификата сотрудника");

            //    entity.HasOne(e => e.WorkClass)
            //        .WithMany()
            //        .HasForeignKey(e => e.WorkClassCode)
            //        .OnDelete(DeleteBehavior.Cascade)
            //        .HasConstraintName("FK_WorkClassAccess_WorkClass");

            //    entity.HasOne(e => e.StafCertificate)
            //        .WithMany(s => s.WorkClassAccessEntities)  // ✅ Теперь работает!
            //        .HasForeignKey(e => e.StafCertificateId)
            //        .OnDelete(DeleteBehavior.Cascade)
            //        .HasConstraintName("FK_WorkClassAccess_StafCertificate");
            //});

        }
    }
}
