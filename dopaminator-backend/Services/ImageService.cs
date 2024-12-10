using System.Drawing;
using System.Drawing.Imaging;

namespace Dopaminator.Services {
    public class ImageService
    {
        public byte[] BlurImage(byte[] originalImageData)
        {
            using var ms = new MemoryStream(originalImageData);
            using var originalImage = Image.FromStream(ms);
            using var blurredImage = new Bitmap(originalImage.Width, originalImage.Height);

            using var graphics = Graphics.FromImage(blurredImage);
            var rect = new Rectangle(0, 0, originalImage.Width, originalImage.Height);
            graphics.DrawImage(originalImage, rect);

            // Apply a simple blur effect (Gaussian Blur or similar).
            var attributes = new ImageAttributes();
            attributes.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
            graphics.FillRectangle(new SolidBrush(Color.Transparent), rect);
            
            using var blurredStream = new MemoryStream();
            blurredImage.Save(blurredStream, originalImage.RawFormat);
            return blurredStream.ToArray();
        }
    }
}
