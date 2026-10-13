//// Diploma.Application/Services/SignatureService.cs
//using Diploma.DataAccess;
//using Diploma.DataAccess.Entities;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using System.Text.Json;

//namespace Diploma.Application.Services
//{
//    public class SignatureService : ISignatureService
//    {
//        private readonly DiplomaDbContext _context;
//        private readonly ILogger<SignatureService> _logger;

//        public SignatureService(DiplomaDbContext context, ILogger<SignatureService> logger)
//        {
//            _context = context;
//            _logger = logger;
//        }

//        public async Task<SignatureEntity> GetOrCreateSignatureAsync(int documentId)
//        {
//            var signature = await _context.Signatures
//                .FirstOrDefaultAsync(s => s.DocumentId == documentId);

//            if (signature == null)
//            {
//                signature = new SignatureEntity
//                {
//                    DocumentType = documentType,
//                    DocumentId = documentId,
//                    CreatedAt = DateTime.UtcNow
//                };
//                _context.Signatures.Add(signature);
//                await _context.SaveChangesAsync();
//                _logger.LogInformation("✅ Создана запись подписи: {Type}-{Id}", documentType, documentId);
//            }

//            return signature;
//        }

//        public async Task<SignatureEntity> SignByClientAsync(int signatureId, string method, string? ip, string? userAgent)
//        {
//            var signature = await _context.Signatures.FindAsync(signatureId);
//            if (signature == null) throw new Exception("Подпись не найдена");

//            signature.IsSignedByClient = true;
//            signature.SignedAt = DateTime.UtcNow;
//            signature.SignatureMethod = method;
//            signature.ClientVerificationData = JsonSerializer.Serialize(new
//            {
//                ip,
//                userAgent,
//                signed_at = DateTime.UtcNow
//            });

//            await _context.SaveChangesAsync();
//            _logger.LogInformation("✅ Клиент подписал документ через подпись #{Id}", signatureId);
//            return signature;
//        }

//        public async Task<SignatureEntity> SignByDispatcherAsync(int signatureId, int dispatcherTabNum, string? ip, string? userAgent)
//        {
//            var signature = await _context.Signatures.FindAsync(signatureId);
//            if (signature == null) throw new Exception("Подпись не найдена");

//            signature.IsSignedByDispatcher = true;
//            signature.DispatcherSignedAt = DateTime.UtcNow;
//            signature.DispatcherSignedByTabNumber = dispatcherTabNum;
//            signature.DispatcherVerificationData = JsonSerializer.Serialize(new
//            {
//                dispatcher_tab = dispatcherTabNum,
//                ip,
//                userAgent,
//                signed_at = DateTime.UtcNow
//            });

//            await _context.SaveChangesAsync();
//            _logger.LogInformation("✅ Диспетчер #{TabNum} подписал документ через подпись #{Id}", dispatcherTabNum, signatureId);
//            return signature;
//        }

//        public async Task<bool> IsFullySignedAsync(string documentType, int documentId)
//        {
//            var signature = await _context.Signatures
//                .FirstOrDefaultAsync(s => s.DocumentType == documentType && s.DocumentId == documentId);

//            return signature?.IsSignedByClient == true && signature?.IsSignedByDispatcher == true;
//        }
//    }
//}