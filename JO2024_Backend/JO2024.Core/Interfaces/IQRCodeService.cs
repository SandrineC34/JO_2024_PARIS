// IQRCodeService.cs
// ============================================
namespace JO2024.Core.Interfaces;

public interface IQRCodeService
{
    string GenerateQRCode(string data);
    byte[] GenerateQRCodeBytes(string data);
}