//// Diploma.Application/Services/ISignatureService.cs
//using Diploma.DataAccess.Entities;

//namespace Diploma.Application.Services
//{
//    public interface ISignatureService
//    {
//        Task<SignatureEntity> GetOrCreateSignatureAsync(string documentType, int documentId);
//        Task<SignatureEntity> SignByClientAsync(int signatureId, string method, string? ip, string? userAgent);
//        Task<SignatureEntity> SignByDispatcherAsync(int signatureId, int dispatcherTabNum, string? ip, string? userAgent);
//        Task<bool> IsFullySignedAsync(string documentType, int documentId);
//    }
//}