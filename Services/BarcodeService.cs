using InvoiceManagement.Api.Data;
using InvoiceManagement.Api.Models;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using ZXing;
using ZXing.Common;
using ZXing.SkiaSharp;

namespace InvoiceManagement.Api.Services
{
    public class BarcodeService
    {

        private readonly AppDbContext _context;

        public BarcodeService(AppDbContext context)
        {
            _context = context;
        }

        private async Task<string> GenerateCode()
        {
            var prefix = "INV" + DateTime.UtcNow.ToString("MMyyyy");

            var lastCode = await _context.Barcodes
                .Where(b => b.Code.StartsWith(prefix))
                .OrderByDescending(b => b.Code)
                .Select(b => b.Code)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastCode != null)
            {
                nextNumber = int.Parse(lastCode.Replace(prefix, string.Empty)) + 1;
            }

            return $"{prefix}{nextNumber:D4}";
        }

        public async Task<Barcode> GenerateBarcodeAsync(int invoiceId)
        {
            return new Barcode
            {
                InvoiceId = invoiceId,
                Code = await GenerateCode(),
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        public byte[] GenerateBarcodeImage(string code)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Height = 80,
                    Width = 400,
                    PureBarcode = true,
                },
            };

            SKBitmap barcode = writer.Write(code);

            int textAreaHeight = 20;
            int horizontalPadding = 8;
            int verticalPadding = 8;

            var output = new SKBitmap(
                barcode.Width + horizontalPadding * 2,
                barcode.Height + textAreaHeight + verticalPadding * 2);

            using var canvas = new SKCanvas(output);
            canvas.Clear(SKColors.White);
            canvas.DrawBitmap(barcode, horizontalPadding, verticalPadding, SKSamplingOptions.Default);

            using var paint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true
            };

            using var font = new SKFont { Size = textAreaHeight * .8f };

            canvas.DrawText(code, output.Width / 2, (barcode.Height + 8) + (textAreaHeight / 2) + verticalPadding, SKTextAlign.Center, font, paint);

            using var image = SKImage.FromBitmap(output);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            return data.ToArray();
        }

    }
}
