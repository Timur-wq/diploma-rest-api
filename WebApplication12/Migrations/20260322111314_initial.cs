using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Diploma.API.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // === Классификатор работ ===
            migrationBuilder.CreateTable(
                name: "Классификатор работ",
                columns: table => new
                {
                    Кодклассаработ = table.Column<int>(name: "Код класса работ", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Наименованиеклассаработ = table.Column<string>(name: "Наименование класса работ", type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkClass", x => x.Кодклассаработ);
                });

            // === Клиенты ===
            migrationBuilder.CreateTable(
                name: "Клиенты",
                columns: table => new
                {
                    Кодклиента = table.Column<int>(name: "Код клиента", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ФИОклиента = table.Column<string>(name: "ФИО клиента", type: "character varying(50)", maxLength: 50, nullable: false),
                    Контактныйтелефонклиента = table.Column<string>(name: "Контактный телефон клиента", type: "character(18)", maxLength: 18, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Client", x => x.Кодклиента);
                });

            // === Сертификаты ===
            migrationBuilder.CreateTable(
                name: "Сертификаты",
                columns: table => new
                {
                    Кодсертификата = table.Column<int>(name: "Код сертификата", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Наименование = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Эмитент = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Тип = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "vendor"),
                    Категория = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Описание = table.Column<string>(type: "text", nullable: true),
                    Срокдействиямесяцев = table.Column<int>(name: "Срок действия месяцев", type: "integer", nullable: true),
                    Активен = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Необходимостьпродления = table.Column<bool>(name: "Необходимость продления", type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificate", x => x.Кодсертификата);
                });

            // === Сотрудники ===
            migrationBuilder.CreateTable(
                name: "Сотрудники",
                columns: table => new
                {
                    Табельныйномер = table.Column<int>(name: "Табельный номер", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ФИОсотрудника = table.Column<string>(name: "ФИО сотрудника", type: "character varying(100)", maxLength: 100, nullable: false),
                    Датаприёмавштат = table.Column<DateOnly>(name: "Дата приёма в штат", type: "date", nullable: false),
                    Логин = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Пароль = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Роль = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Активен = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Датапоследнеговхода = table.Column<DateTime>(name: "Дата последнего входа", type: "timestamp with time zone", nullable: true),
                    Неудачныепопыткивхода = table.Column<int>(name: "Неудачные попытки входа", type: "integer", nullable: false, defaultValue: 0),
                    Заблокировандо = table.Column<DateTime>(name: "Заблокирован до", type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staf", x => x.Табельныйномер);
                });

            // === Учёт ЗИП ===
            migrationBuilder.CreateTable(
                name: "Учёт ЗИП",
                columns: table => new
                {
                    КодЗИП = table.Column<int>(name: "Код ЗИП", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    НаименованиеЗИП = table.Column<string>(name: "Наименование ЗИП", type: "character varying(50)", maxLength: 50, nullable: false),
                    Количествонаскладе = table.Column<int>(name: "Количество на складе", type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spare", x => x.КодЗИП);
                });

            // === Кодификатор работ ===
            migrationBuilder.CreateTable(
                name: "Кодификатор работ",
                columns: table => new
                {
                    Кодработы = table.Column<int>(name: "Код работы", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Наименованиеработы = table.Column<string>(name: "Наименование работы", type: "character varying(20)", maxLength: 20, nullable: false),
                    Кодклассаработ = table.Column<int>(name: "Код класса работ", type: "integer", nullable: false),
                    Стоимостьработывчас = table.Column<int>(name: "Стоимость работы в час", type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Work", x => x.Кодработы);
                    table.ForeignKey(
                        name: "FK_Work_WorkClass",
                        column: x => x.Кодклассаработ,
                        principalTable: "Классификатор работ",
                        principalColumn: "Код класса работ",
                        onDelete: ReferentialAction.Cascade);
                });

            // === Заявки на ТО СВТ ===
            migrationBuilder.CreateTable(
                name: "Заявки на ТО СВТ",
                columns: table => new
                {
                    Кодзаявки = table.Column<int>(name: "Код заявки", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Кодклиента = table.Column<int>(name: "Код клиента", type: "integer", nullable: false),
                    Датапоступлениязаявки = table.Column<DateOnly>(name: "Дата поступления заявки", type: "date", nullable: false),
                    Времяпоступлениязаявки = table.Column<TimeOnly>(name: "Время поступления заявки", type: "time", nullable: false),
                    СерийныйномерСВТ = table.Column<string>(name: "Серийный номер СВТ", type: "character varying(20)", maxLength: 20, nullable: false),
                    ТипСВТ = table.Column<string>(name: "Тип СВТ", type: "character varying(20)", maxLength: 20, nullable: false),
                    Модель = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Описаниенеисправности = table.Column<string>(name: "Описание неисправности", type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Статус = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Причинаотклонения = table.Column<string>(name: "Причина отклонения", type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Request", x => x.Кодзаявки);
                    table.ForeignKey(
                        name: "FK_Request_Client",
                        column: x => x.Кодклиента,
                        principalTable: "Клиенты",
                        principalColumn: "Код клиента",
                        onDelete: ReferentialAction.Cascade);
                });

            // === Refresh-токены ===
            migrationBuilder.CreateTable(
                name: "Refresh-токены",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Кодсотрудника = table.Column<int>(name: "Код сотрудника", type: "integer", nullable: false),
                    Токен = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Истекает = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Создан = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Использован = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Отозван = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IPадрес = table.Column<string>(name: "IP адрес", type: "character varying(45)", maxLength: 45, nullable: true),
                    Userагент = table.Column<string>(name: "User агент", type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshToken_Staf",
                        column: x => x.Кодсотрудника,
                        principalTable: "Сотрудники",
                        principalColumn: "Табельный номер",
                        onDelete: ReferentialAction.Cascade);
                });

            // === Сотрудники-Сертификаты ===
            migrationBuilder.CreateTable(
                name: "Сотрудники-Сертификаты",
                columns: table => new
                {
                    Кодзаписи = table.Column<int>(name: "Код записи", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Кодсервисногоинженера = table.Column<int>(name: "Код сервисного инженера", type: "integer", nullable: false),
                    Кодсертификата = table.Column<int>(name: "Код сертификата", type: "integer", nullable: false),
                    Номерсертификата = table.Column<string>(name: "Номер сертификата", type: "character varying(100)", maxLength: 100, nullable: false),
                    Датаполучения = table.Column<DateTime>(name: "Дата получения", type: "timestamp with time zone", nullable: false),
                    Датаистечения = table.Column<DateTime>(name: "Дата истечения", type: "timestamp with time zone", nullable: true),
                    Статус = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StafCertificate", x => x.Кодзаписи);
                    table.ForeignKey(
                        name: "FK_StafCertificate_Certificate",
                        column: x => x.Кодсертификата,
                        principalTable: "Сертификаты",
                        principalColumn: "Код сертификата",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StafCertificate_Staf",
                        column: x => x.Кодсервисногоинженера,
                        principalTable: "Сотрудники",
                        principalColumn: "Табельный номер",
                        onDelete: ReferentialAction.Cascade);
                });

            // === Акт диагностики ===
            migrationBuilder.CreateTable(
                name: "Акт диагностики",
                columns: table => new
                {
                    Кодакта = table.Column<int>(name: "Код акта", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Табельныйномерсервисногоинженера = table.Column<int>(name: "Табельный номер сервисного инженера", type: "integer", nullable: false),
                    Кодзаявки = table.Column<int>(name: "Код заявки", type: "integer", nullable: false),
                    Датадиагностики = table.Column<DateOnly>(name: "Дата диагностики", type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiagnosticAct", x => x.Кодакта);
                    table.ForeignKey(
                        name: "FK_DiagnosticAct_Request",
                        column: x => x.Кодзаявки,
                        principalTable: "Заявки на ТО СВТ",
                        principalColumn: "Код заявки",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiagnosticAct_Staf",
                        column: x => x.Табельныйномерсервисногоинженера,
                        principalTable: "Сотрудники",
                        principalColumn: "Табельный номер",
                        onDelete: ReferentialAction.Cascade);
                });

            // === Наряд ===
            migrationBuilder.CreateTable(
                name: "Наряд",
                columns: table => new
                {
                    Коднаряда = table.Column<int>(name: "Код наряда", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Табельныйномерсервисногоинженера = table.Column<int>(name: "Табельный номер сервисного инженера", type: "integer", nullable: false),
                    Кодработы = table.Column<int>(name: "Код работы", type: "integer", nullable: false),
                    Кодзаявки = table.Column<int>(name: "Код заявки", type: "integer", nullable: false),
                    Датаивремяназначениянаряда = table.Column<DateTime>(name: "Дата и время назначения наряда", type: "timestamp with time zone", nullable: false),
                    Датаивремяприступлениякнаряду = table.Column<DateTime>(name: "Дата и время приступления к наряду", type: "timestamp with time zone", nullable: true),
                    Датаивремязавершениянаряда = table.Column<DateTime>(name: "Дата и время завершения наряда", type: "timestamp with time zone", nullable: true),
                    Статус = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrder", x => x.Коднаряда);
                    table.ForeignKey(
                        name: "FK_WorkOrder_Request",
                        column: x => x.Кодзаявки,
                        principalTable: "Заявки на ТО СВТ",
                        principalColumn: "Код заявки",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrder_Work",
                        column: x => x.Кодработы,
                        principalTable: "Кодификатор работ",
                        principalColumn: "Код работы",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrder_Staf",
                        column: x => x.Табельныйномерсервисногоинженера,
                        principalTable: "Сотрудники",
                        principalColumn: "Табельный номер",
                        onDelete: ReferentialAction.Cascade);
                });

            // === Допуск к классам работ ===
            migrationBuilder.CreateTable(
                name: "Допуск к классам работ",
                columns: table => new
                {
                    Кодклассаработ = table.Column<int>(name: "Код класса работ", type: "integer", nullable: false),
                    Кодзаписисертификатасотрудника = table.Column<int>(name: "Код записи сертификата сотрудника", type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkClassAccess", x => new { x.Кодклассаработ, x.Кодзаписисертификатасотрудника });
                    table.ForeignKey(
                        name: "FK_WorkClassAccess_WorkClass",
                        column: x => x.Кодклассаработ,
                        principalTable: "Классификатор работ",
                        principalColumn: "Код класса работ",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkClassAccess_StafCertificate",
                        column: x => x.Кодзаписисертификатасотрудника,
                        principalTable: "Сотрудники-Сертификаты",
                        principalColumn: "Код записи",
                        onDelete: ReferentialAction.Cascade);
                });

            // === Перечень ЗИП - Акт диагностики ===
            migrationBuilder.CreateTable(
                name: "Перечень ЗИП - Акт диагностики",
                columns: table => new
                {
                    КодЗИП = table.Column<int>(name: "Код ЗИП", type: "integer", nullable: false),
                    Кодакта = table.Column<int>(name: "Код акта", type: "integer", nullable: false),
                    Количество = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpareDiagnosticAct", x => new { x.КодЗИП, x.Кодакта });
                    table.ForeignKey(
                        name: "FK_SpareDiagnosticAct_DiagnosticAct",
                        column: x => x.Кодакта,
                        principalTable: "Акт диагностики",
                        principalColumn: "Код акта",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpareDiagnosticAct_Spare",
                        column: x => x.КодЗИП,
                        principalTable: "Учёт ЗИП",
                        principalColumn: "Код ЗИП",
                        onDelete: ReferentialAction.Cascade);
                });

            // === Перечень работ - Акт диагностики (ИСПРАВЛЕНО!) ===
            migrationBuilder.CreateTable(
                name: "Перечень работ - Акт диагностики",
                columns: table => new
                {
                    Кодработы = table.Column<int>(name: "Код работы", type: "integer", nullable: false),
                    Кодакта = table.Column<int>(name: "Код акта", type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkDiagnosticAct", x => new { x.Кодработы, x.Кодакта });

                    // ✅ Короткие имена внешних ключей (не более 50 символов)
                    table.ForeignKey(
                        name: "FK_WorkDiagnosticAct_DiagnosticAct",
                        column: x => x.Кодакта,
                        principalTable: "Акт диагностики",
                        principalColumn: "Код акта",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_WorkDiagnosticAct_Work",
                        column: x => x.Кодработы,
                        principalTable: "Кодификатор работ",
                        principalColumn: "Код работы",
                        onDelete: ReferentialAction.Cascade);
                });

            // === Индексы (с короткими именами) ===
            migrationBuilder.CreateIndex(name: "IX_RefreshToken_Staf", table: "Refresh-токены", column: "Код сотрудника");
            migrationBuilder.CreateIndex(name: "IX_RefreshToken_Token", table: "Refresh-токены", column: "Токен", unique: true);
            migrationBuilder.CreateIndex(name: "IX_DiagnosticAct_Request", table: "Акт диагностики", column: "Код заявки");
            migrationBuilder.CreateIndex(name: "IX_DiagnosticAct_Staf", table: "Акт диагностики", column: "Табельный номер сервисного инженера");
            migrationBuilder.CreateIndex(name: "IX_WorkClassAccess_StafCertificate", table: "Допуск к классам работ", column: "Код записи сертификата сотрудника");
            migrationBuilder.CreateIndex(name: "IX_Request_Client", table: "Заявки на ТО СВТ", column: "Код клиента");
            migrationBuilder.CreateIndex(name: "IX_Work_WorkClass", table: "Кодификатор работ", column: "Код класса работ");
            migrationBuilder.CreateIndex(name: "IX_WorkOrder_Request", table: "Наряд", column: "Код заявки");
            migrationBuilder.CreateIndex(name: "IX_WorkOrder_Work", table: "Наряд", column: "Код работы");
            migrationBuilder.CreateIndex(name: "IX_WorkOrder_Staf", table: "Наряд", column: "Табельный номер сервисного инженера");
            migrationBuilder.CreateIndex(name: "IX_SpareDiagnosticAct_DiagnosticAct", table: "Перечень ЗИП - Акт диагностики", column: "Код акта");
            migrationBuilder.CreateIndex(name: "IX_WorkDiagnosticAct_DiagnosticAct", table: "Перечень работ - Акт диагностики", column: "Код акта");
            migrationBuilder.CreateIndex(name: "IX_StafCertificate_Staf", table: "Сотрудники-Сертификаты", column: "Код сервисного инженера");
            migrationBuilder.CreateIndex(name: "IX_StafCertificate_Certificate", table: "Сотрудники-Сертификаты", column: "Код сертификата");
            migrationBuilder.CreateIndex(name: "UQ_StafCertificate_Unique", table: "Сотрудники-Сертификаты", columns: new[] { "Код сервисного инженера", "Код сертификата" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_Staf_Login", table: "Сотрудники", column: "Логин", unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Refresh-токены");
            migrationBuilder.DropTable(name: "Допуск к классам работ");
            migrationBuilder.DropTable(name: "Наряд");
            migrationBuilder.DropTable(name: "Перечень ЗИП - Акт диагностики");
            migrationBuilder.DropTable(name: "Перечень работ - Акт диагностики");
            migrationBuilder.DropTable(name: "Сотрудники-Сертификаты");
            migrationBuilder.DropTable(name: "Учёт ЗИП");
            migrationBuilder.DropTable(name: "Акт диагностики");
            migrationBuilder.DropTable(name: "Кодификатор работ");
            migrationBuilder.DropTable(name: "Сертификаты");
            migrationBuilder.DropTable(name: "Заявки на ТО СВТ");
            migrationBuilder.DropTable(name: "Сотрудники");
            migrationBuilder.DropTable(name: "Классификатор работ");
            migrationBuilder.DropTable(name: "Клиенты");
        }
    }
}