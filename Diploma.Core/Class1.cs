using Diploma.Core.Enums;
using Diploma.DataAccess.Entities;
using System.ComponentModel.DataAnnotations;
using static Diploma.Core.Class1;

namespace Diploma.Core
{
    public class Class1
    {
        public class AssignWorkOrderRequest
        {
            public int WorkId { get; set; }      // Код работы из Кодификатор работ
            public int RequestId { get; set; }   // Код заявки
            public int? MaxActiveOrders { get; set; } = 2; // Лимит активных нарядов
        }

        public class AssignWorkOrderResponse
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public int? TaskId { get; set; }
            public int? AssignedEngineerId { get; set; }
            public string? EngineerName { get; set; }
            public DateTime? AssignedAt { get; set; }
        }

        public class CandidateDto
        {
            public int TabNumber { get; set; }
            public string FIO { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public int ActiveOrdersCount { get; set; }
            public List<string> Certificates { get; set; } = new();
            public List<string> WorkClassAccess { get; set; } = new();
        }

        public class EngineerTaskDto
        {
            public int TaskId { get; set; }
            public int RequestId { get; set; }
            public string ClientFio { get; set; } = string.Empty;
            public string ClientPhone { get; set; } = string.Empty;
            public string SvtType { get; set; } = string.Empty;
            public string Model { get; set; } = string.Empty;
            public string SerialNumber { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public DateTime AssignedAt { get; set; }
            public DateTime? StartedAt { get; set; }
            public DateTime? CompletedAt { get; set; }
            public string WorkName { get; set; } = string.Empty;
            public int WorkClassCode { get; set; }
        }

        public class EngineerStatsDto
        {
            public int TotalTasks { get; set; }
            public int ActiveTasks { get; set; }
            public int CompletedTasks { get; set; }
            public int CancelledTasks { get; set; }
        }

        public class StartTaskRequest
        {
            public int TaskId { get; set; }
        }

        public class CompleteTaskRequest
        {
            public int TaskId { get; set; }
            public string? Comment { get; set; }
        }

        public class CreateDiagnosticActRequest
        {
            public int TaskId { get; set; }
            public string DiagnosticDate { get; set; } = string.Empty;
            public string ExternalCondition { get; set; } = string.Empty;
            public string IdentifiedIssues { get; set; } = string.Empty;
            public string TestResults { get; set; } = string.Empty;
            public string Recommendations { get; set; } = string.Empty;
            public decimal? EstimatedCost { get; set; }
            public string EstimatedTime { get; set; } = string.Empty;

            // Общие ЗИП (не привязанные к работам)
            public List<ActSpareItemDto> RequiredSpares { get; set; } = new();

            // Работы с привязанными ЗИП
            public List<ActWorkWithSparesDto> RequiredWorks { get; set; } = new();
        }

        public class ActSpareItemDto
        {
            public int SpareCode { get; set; }
            public int Quantity { get; set; }
        }

        public class ActWorkWithSparesDto  // 🔥 Новый интерфейс
        {
            public int WorkCode { get; set; }
            public List<ActSpareLinkDto> RequiredSpares { get; set; } = new();
        }

        public class ActSpareLinkDto
        {
            public int SpareCode { get; set; }
            public int Quantity { get; set; } = 1;
            public bool IsRequired { get; set; } = true;
        }

        public class ActWorkItemDto
        {
            public int WorkCode { get; set; }

            // 🔥 Список ЗИП, необходимых для этой работы
            public List<WorkSpareLinkDto> RequiredSpares { get; set; } = new();
        }

        public class WorkSpareLinkDto
        {
            public int SpareCode { get; set; }
            public int Quantity { get; set; } = 1;
            public bool IsRequired { get; set; } = true;  // Обязательный ЗИП для работы
        }

        // === Ответ создания акта ===
        public class DiagnosticActResponseDto
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public int? ActCode { get; set; }
        }

        // === DTO для просмотра акта ===
        public class DiagnosticActDto
        {
            public int ActCode { get; set; }
            public int TaskId { get; set; }
            public int RequestId { get; set; }
            public string ClientFio { get; set; } = string.Empty;
            public string ClientPhone { get; set; } = string.Empty;
            public string EngineerFio { get; set; } = string.Empty;
            public int EngineerTabNum { get; set; }
            public DateOnly DiagnosticDate { get; set; }
            public TimeOnly DiagnosticTime { get; set; }
            public string ExternalCondition { get; set; } = string.Empty;
            public string IdentifiedIssues { get; set; } = string.Empty;
            public string TestResults { get; set; } = string.Empty;
            public string Recommendations { get; set; } = string.Empty;
            public decimal? EstimatedCost { get; set; }
            public string? EstimatedTime { get; set; }
            public DiagnosticActStatus Status { get; set; }
            public string? DispatcherComment { get; set; }
            public string? ClientComment { get; set; }
            public DateTime? SubmittedToDispatcherAt { get; set; }
            public DateTime? SentToClientAt { get; set; }
            public DateTime? ApprovedByClientAt { get; set; }
            public DateTime CreatedAt { get; set; }
            public List<DiagnosticActWorkItemDto> Works { get; set; } = new();
            public List<DiagnosticActSpareItemDto> Spares { get; set; } = new();
        }

        public class DiagnosticActWorkItemDto
        {
            public int RecordId { get; set; }
            public int WorkCode { get; set; }
            public string WorkName { get; set; } = string.Empty;
            public string? Description { get; set; }
            public int Quantity { get; set; }
            public decimal? EstimatedCost { get; set; }
            public bool? IsApprovedByClient { get; set; }

            // 🔥 ДОБАВЬТЕ ЭТО СВОЙСТВО:
            public List<DiagnosticActSpareItemDto> RequiredSpares { get; set; } = new();
        }

        public class DiagnosticActSpareItemDto
        {
            public int RecordId { get; set; }
            public int SpareCode { get; set; }
            public string SpareName { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public string Unit { get; set; } = string.Empty;

            public decimal? UnitPrice { get; set; }
            public bool? IsApprovedByClient { get; set; }

            // 🔥 НОВЫЕ ПОЛЯ
            public int AvailableInWarehouse { get; set; }  // Доступно на складе
            public string Source { get; set; } = string.Empty;  // "Warehouse" или "Supplier"
        }

        // === Запросы для согласования ===
        public class SubmitActToClientRequest
        {
            public string? Comment { get; set; }
        }

        public class UpdateDiagnosticActRequest
        {
            public string? ExternalCondition { get; set; }
            public string? IdentifiedIssues { get; set; }
            public string? TestResults { get; set; }
            public string? Recommendations { get; set; }
            public decimal? EstimatedCost { get; set; }
            public string? EstimatedTime { get; set; }
            public string? DispatcherComment { get; set; }
        }

        public class ClientActDecisionRequest
        {
            public string? Comment { get; set; }
            public List<ApprovedWorkItem> ApprovedWorks { get; set; } = new();
            public List<ApprovedSpareItem> ApprovedSpares { get; set; } = new();
        }

        public class ApprovedWorkItem
        {
            public int RecordId { get; set; }
            public bool IsApproved { get; set; }
        }

        public class ApprovedSpareItem
        {
            public int RecordId { get; set; }
            public bool IsApproved { get; set; }
        }

        public class ActDecisionResponseDto
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public decimal? ApprovedCost { get; set; }
            public DiagnosticActStatus NewStatus { get; set; }
        }

        public class ProcurementResultDto
        {
            public int ActCode { get; set; }
            public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

            /// <summary>
            /// Матрица закупок: какие запчасти у каких поставщиков закупать
            /// </summary>
            public List<ProcurementItemDto> ProcurementItems { get; set; } = new();

            /// <summary>
            /// Сводка по поставщикам
            /// </summary>
            public List<SupplierSummaryDto> SupplierSummaries { get; set; } = new();

            /// <summary>
            /// Общая стоимость закупки
            /// </summary>
            public decimal TotalCost { get; set; }

            /// <summary>
            /// Статус выполнения: все ли потребности удовлетворены
            /// </summary>
            public bool IsFullySatisfied { get; set; }

            /// <summary>
            /// Невыполненные потребности (если есть)
            /// </summary>
            public List<UnmetDemandDto> UnmetDemands { get; set; } = new();

            /// <summary>
            /// Сообщение алгоритма
            /// </summary>
            public string Message { get; set; } = string.Empty;
        }

        /// <summary>
        /// Элемент матрицы закупок
        /// </summary>
        public class ProcurementItemDto
        {
            public int SpareCode { get; set; }
            public string SpareName { get; set; } = string.Empty;
            public int RequiredQuantity { get; set; }
            public int ProcuredQuantity { get; set; }

            /// <summary>
            /// Закупки по поставщикам для этой запчасти
            /// </summary>
            public List<SupplierProcurementDto> SupplierAllocations { get; set; } = new();
        }

        /// <summary>
        /// Закупка у конкретного поставщика
        /// </summary>
        public class SupplierProcurementDto
        {
            public int SupplierId { get; set; }
            public string SupplierName { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice => Quantity * UnitPrice;
            public int DeliveryDays { get; set; }
        }

        /// <summary>
        /// Сводка по поставщику
        /// </summary>
        public class SupplierSummaryDto
        {
            public int SupplierId { get; set; }
            public string SupplierName { get; set; } = string.Empty;
            public int TotalItems { get; set; }  // Всего позиций
            public int TotalQuantity { get; set; }  // Всего штук
            public decimal TotalCost { get; set; }  // Общая стоимость
            public int MaxDeliveryDays { get; set; }  // Максимальный срок поставки
        }

        /// <summary>
        /// Невыполненная потребность
        /// </summary>
        public class UnmetDemandDto
        {
            public int SpareCode { get; set; }
            public string SpareName { get; set; } = string.Empty;
            public int Required { get; set; }
            public int Procured { get; set; }
            public int Shortage => Required - Procured;
        }

        public class ContractDto
        {
            public int ContractId { get; set; }
            public int RequestId { get; set; }
            public string? ContractNumber { get; set; }
            public string Status { get; set; } = string.Empty;
            public string? FilePath { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? SignedAt { get; set; }
            public bool IsSignedByClient { get; set; }
            public string? Comment { get; set; }
        }

        public class InvoiceDto
        {
            public int InvoiceId { get; set; }
            public int RequestId { get; set; }
            public string? InvoiceNumber { get; set; }
            public string Status { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public string? FilePath { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? IssuedAt { get; set; }
            public DateTime? PaidAt { get; set; }
            public string? ReceiptPath { get; set; }
            public string? Comment { get; set; }
        }

        // DTO для запросов
        public class GenerateDocumentRequest
        {
            public string FilePath { get; set; } = string.Empty;
        }

        public class GenerateInvoiceRequest : GenerateDocumentRequest
        {
            public decimal Amount { get; set; }
        }

        public class UploadReceiptRequest
        {
            public string ReceiptPath { get; set; } = string.Empty;
        }

        public class PaymentInfoDto
        {
            public string ContractStatus { get; set; } = string.Empty;
            public string? ContractFilePath { get; set; }
            public bool IsContractSigned { get; set; }
            public string InvoiceStatus { get; set; } = string.Empty;
            public string? InvoiceFilePath { get; set; }
            public string? PaymentReceiptPath { get; set; }
            public bool IsPrepaymentCompleted { get; set; }
        }

        /// <summary>
        /// DTO для создания приходной накладной
        /// </summary>
        public class CreateGoodsReceiptRequest
        {
            public int SpareOrderId { get; set; }
            public int ReceivedQuantity { get; set; }
            public int GoodQuantity { get; set; }
            public int DefectQuantity { get; set; }
            public string? Comment { get; set; }
            public int ReceivedByEmployeeId { get; set; }
            public GoodsReceiptStatus Status { get; set; }
        }

        /// <summary>
        /// DTO для ответа о приходной накладной
        /// </summary>
        public class GoodsReceiptDto
        {
            public int ReceiptId { get; set; }
            public int SpareOrderId { get; set; }
            public int OrderedQuantity { get; set; }
            public int ReceivedQuantity { get; set; }
            public int GoodQuantity { get; set; }
            public int DefectQuantity { get; set; }
            public string? Comment { get; set; }
            public DateTime ReceivedAt { get; set; }
            public int? ReceivedByEmployeeId { get; set; }
            public string? ReceivedByEmployeeFio { get; set; }
            public GoodsReceiptStatus Status { get; set; }
            public string SpareName { get; set; } = string.Empty;
            public string SupplierName { get; set; } = string.Empty;
        }

        /// <summary>
        /// DTO для списания ЗИП (брак)
        /// </summary>
        public class CreateWriteOffRequest
        {
            public int SupplierSpareId { get; set; }
            public int Quantity { get; set; }
            public WriteOffType WriteOffType { get; set; }
            public int? TaskEntityId { get; set; }
            public int? GoodsReceiptId { get; set; }
            public string? Comment { get; set; }
        }

        /// <summary>
        /// Запрос для приёмки товара
        /// </summary>
        public class AcceptGoodsRequest
        {
            public int ReceivedQuantity { get; set; }
            public int GoodQuantity { get; set; }
            public int DefectQuantity => ReceivedQuantity - GoodQuantity;
            public string? Comment { get; set; }
            public int EmployeeId { get; set; }
        }

        public class CreateClientRequest
        {
            [Required(ErrorMessage = "ФИО обязательно")]
            [MinLength(3, ErrorMessage = "ФИО должно быть не менее 3 символов")]
            public string Fio { get; set; } = string.Empty;

            [Required(ErrorMessage = "Логин обязателен")]
            [MinLength(3, ErrorMessage = "Логин должен быть не менее 3 символов")]
            [MaxLength(50, ErrorMessage = "Логин должен быть не более 50 символов")]
            public string Login { get; set; } = string.Empty;

            [Required(ErrorMessage = "Пароль обязателен")]
            [MinLength(8, ErrorMessage = "Пароль должен быть не менее 8 символов")]
            // 🔥 НОВАЯ РЕГУЛЯРКА: латиница + цифры + спецсимволы, минимум 8 символов
            [RegularExpression(
    @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d!@#$%^&*()_+\-=\[\]{}|;:',.<>?\\/~`]{8,}$",
    ErrorMessage = "Пароль должен содержать латинские буквы, цифры и может содержать спецсимволы")]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Телефон обязателен")]
            [Phone(ErrorMessage = "Неверный формат телефона")]
            public string Phone { get; set; } = string.Empty;

            [EmailAddress(ErrorMessage = "Неверный формат email")]
            public string? Email { get; set; }
        }

        // 🔥 Класс для данных счёта на оплату работ (финальный платёж)
        public class InvoiceData2
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
            public List<WorkInfo2> Works { get; set; } = new();

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
        public class RegistrationResponseDto
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public int? ClientId { get; set; }
            public string? Login { get; set; }
        }

        // ============================================
        // 🔥 DTO ДЛЯ РЕМОНТА (НОВОЕ)
        // ============================================
        public class AssignRepairWorkOrdersRequest
        {
            public int RequestId { get; set; }
            public int? MaxActiveOrders { get; set; }
        }

        public class AssignRepairWorkOrdersResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public List<int> TaskIds { get; set; } = new();  // Все наряды
            public int AssignedEngineerId { get; set; }
            public string EngineerName { get; set; } = string.Empty;
            public int WorksCount { get; set; }
            public List<RepairWorkItemDto> Works { get; set; } = new();
            public DateTime AssignedAt { get; set; }
        }

        public class RepairWorkItemDto
        {
            public int TaskId { get; set; }  // Код наряда
            public int WorkCode { get; set; }  // Код работы
            public string WorkName { get; set; } = string.Empty;
            public decimal EstimatedCost { get; set; }
        }

        public class RepairTaskPdfData
        {
            public int TaskId { get; set; }
            public int RequestId { get; set; }
            public string ClientFio { get; set; } = string.Empty;
            public string DeviceModel { get; set; } = string.Empty;
            public string SerialNumber { get; set; } = string.Empty;
            public string WorkName { get; set; } = string.Empty;
            public string EngineerFio { get; set; } = string.Empty;
            public int EngineerTabNumber { get; set; }
            public List<SpareItemData> ApprovedSpares { get; set; } = new();
        }

        public class RepairActData
        {
            public int TaskId { get; set; }
            public int RequestId { get; set; }
            public string ClientFio { get; set; } = string.Empty;
            public string ClientPosition { get; set; } = string.Empty;  // 🔥 Новое поле
            public string DeviceModel { get; set; } = string.Empty;
            public string SerialNumber { get; set; } = string.Empty;
            public string WorkName { get; set; } = string.Empty;
            public DateTime CompletedAt { get; set; }
            public string EngineerFio { get; set; } = string.Empty;
            public int EngineerTabNumber { get; set; }

            // 🔥 Информация о подписях
            public DateTime? ClientSignedAt { get; set; }
            public string? ClientSignatureMethod { get; set; }
            public string? ClientIp { get; set; }
            public string? ClientUserAgent { get; set; }

            public DateTime? DispatcherSignedAt { get; set; }
            public string? DispatcherFio { get; set; }
            public int? DispatcherTabNumber { get; set; }
        }

        public class SpareWriteOffData
        {
            public int TaskId { get; set; }
            public int RequestId { get; set; }
            public string ClientFio { get; set; } = string.Empty;
            public string DeviceModel { get; set; } = string.Empty;
            public string SerialNumber { get; set; } = string.Empty;
            public List<SpareItemData> Spares { get; set; } = new();
            public DateTime CompletedAt { get; set; }
        }

        public class SpareItemData
        {
            public string SpareName { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice => Quantity * UnitPrice;
        }

        public class WarrantyResponse
        {
            public string Message { get; set; } = string.Empty;
            public string? WarrantyPath { get; set; }
            public DateTime? ValidUntil { get; set; }
        }

        public class FinalizationStatusResponse
        {
            public bool ActExists { get; set; }
            public bool ActSigned { get; set; }
            public DateTime? ActSignedAt { get; set; }
            public bool PaymentReceived { get; set; }
            public bool WarrantyExists { get; set; }
            public string? WarrantyPath { get; set; }
            public DateTime? WarrantyValidUntil { get; set; }

            // Подписи акта
            public bool ActSignedByClient { get; set; }
            public DateTime? ClientSignedAt { get; set; }
            public string? ClientSignatureMethod { get; set; }

            public bool ActSignedByDispatcher { get; set; }
            public DateTime? DispatcherSignedAt { get; set; }
            public int? DispatcherSignedByTabNumber { get; set; }

            public bool IsVerifiedByDispatcher { get; set; }
            public bool IsFullySigned => ActSignedByClient && ActSignedByDispatcher;
        }

        public class WarrantyData
        {
            public int RequestId { get; set; }
            public int ActId { get; set; }
            public string ClientFio { get; set; } = string.Empty;
            public string DeviceModel { get; set; } = string.Empty;
            public string SerialNumber { get; set; } = string.Empty;
            public DateTime RepairDate { get; set; }
            public int WarrantyMonths { get; set; } = 3;

            // 🔥 НОВОЕ: Информация о подписях
            public DateTime? ClientSignedAt { get; set; }
            public string? ClientSignatureMethod { get; set; }

            public DateTime? DispatcherSignedAt { get; set; }
            public string? DispatcherFio { get; set; }
            public int? DispatcherTabNumber { get; set; }
        }

        /// <summary>
        /// Статус подписания акта
        /// </summary>
        public class ActSignatureStatus
        {
            public int ActId { get; set; }

            // Клиент
            public bool IsSignedByClient { get; set; }
            public DateTime? ClientSignedAt { get; set; }  // мапим из SignedAt
            public string? ClientSignatureMethod { get; set; }  // мапим из SignatureMethod
            public int? RequestId { get; set; }  // ← НОВОЕ ПОЛЕ!
            public string? ClientVerificationData { get; set; }

            // Диспетчер
            public bool IsSignedByDispatcher { get; set; }
            public DateTime? DispatcherSignedAt { get; set; }
            public int? DispatcherSignedByTabNumber { get; set; }
            public string? DispatcherVerificationData { get; set; }

            // Общий статус
            public bool IsVerifiedByDispatcher { get; set; }
            public bool IsFullySigned { get; set; }

            // 🔥 Новые поля для агрегации
            public int ActsCount { get; set; }
            public int SignedActsCount { get; set; }
        }

        public class ContractSignatureStatusDto
        {
            public int ContractId { get; set; }
            public bool IsSignedByClient { get; set; }
            public DateTime? ClientSignedAt { get; set; }
            public string? ClientSignatureMethod { get; set; }
            public bool IsSignedByDispatcher { get; set; }
            public DateTime? DispatcherSignedAt { get; set; }
            public int? DispatcherSignedByTabNumber { get; set; }
            public bool IsFullySigned { get; set; }
        }
        public class InvoiceResponse
        {
            public int InvoiceId { get; set; }
            public decimal Amount { get; set; }
            public string? FilePath { get; set; }
            public bool IsPaid { get; set; }
            public string? ReceiptPath { get; set; }
            public DateTime? CreatedAt { get; set; }

            public bool ReceiptUploaded { get; set; }

            // 🔥 Новое поле для пояснения
            public string? Note { get; set; }
        }

        // 🔥 Вспомогательный класс для работы
        public class WorkInfo2
        {
            public string WorkName { get; set; } = string.Empty;
            public string WorkCode { get; set; } = string.Empty;
            public int Quantity { get; set; } = 1;
            public string Unit { get; set; } = "час";  // или "шт"
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice { get; set; }
            public string Description { get; set; } = string.Empty;  // Детали работы
        }

        public class ContractPdfData
        {
            public int RequestId { get; set; }
            public string? ContractNumber { get; set; }
            public string ClientFio { get; set; } = string.Empty;
            public string ClientPhone { get; set; } = string.Empty;
            public string DeviceModel { get; set; } = string.Empty;
            public string SerialNumber { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }

            // 🔥 Данные подписей
            public DateTime? ClientSignedAt { get; set; }
            public string? ClientSignatureMethod { get; set; }

            public string? DispatcherSignatureMethod { get; set; }  // 🔥 Добавьте это
            public string? ClientIp { get; set; }
            public DateTime? DispatcherSignedAt { get; set; }
            public string DispatcherFio { get; set; } = string.Empty;
            public int? DispatcherTabNumber { get; set; }
        }

        public class CreateStaffDto
        {
            public string FullName { get; set; } = string.Empty;
            public DateOnly HireDate { get; set; }
            public string Login { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            // Роли: Dispatcher, ServiceEngineer, Accountant, OmtEmployee
            public string Role { get; set; } = "ServiceEngineer";
        }

        public class StaffDto
        {
            public int UserId { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string Login { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public int TabNumber { get; set; }
            public DateTime HireDate { get; set; }
            public bool IsActive { get; set; }
        }

        public class AccountingReportData
        {
            public int RequestId { get; set; }
            public DateTime GeneratedAt { get; set; }
            public string DispatcherFio { get; set; } = string.Empty;
            public List<string> EngineerFios { get; set; } = new(); // Может быть несколько инженеров
            public decimal TotalWorksCost { get; set; }
            public decimal TotalSparesCost { get; set; }

            public List<ReportWorkItem> Works { get; set; } = new();
            public List<ReportSpareItem> Spares { get; set; } = new();

            // Добавляем сводную прибыль по ЗИП
            public decimal TotalSpareProfit => Spares.Sum(s => s.Profit);
        }

        public class ReportWorkItem
        {
            public string WorkName { get; set; } = string.Empty;
            public decimal Cost { get; set; }
        }

        public class ReportSpareItem
        {
            public int SpareCode { get; set; }
            public string SpareName { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }        // Цена закупки
            public decimal SellingPrice { get; set; }     // Цена реализации (из Учёт ЗИП)
            public decimal TotalPrice { get; set; }       // Quantity * UnitPrice
            public decimal TotalSellingPrice { get; set; } // Quantity * SellingPrice
            public decimal Profit => TotalSellingPrice - TotalPrice; // Прибыль
        }



    }
}
