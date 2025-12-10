using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

public interface IQrService
{
    byte[] GenerateQrCode(string content);
}

public class QrService : IQrService
{
    public byte[] GenerateQrCode(string content)
    {
        QRCodeGenerator qr = new QRCodeGenerator();
        QRCodeData data = qr.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        QRCode qrCode = new QRCode(data);

        using Bitmap bitmap = qrCode.GetGraphic(20);
        using MemoryStream ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }
}
