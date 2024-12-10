using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Dopaminator.Services {
    public class ImageService
    {
        public byte[] BlurImage(byte[] originalImageData)
        {
        using var inputStream = new MemoryStream(originalImageData);
        using var image = Image.Load(inputStream);

        // Apply blur effect
        image.Mutate(x => x.GaussianBlur(10)); // Adjust blur intensity as needed

        using var outputStream = new MemoryStream();
        image.SaveAsPng(outputStream); // Save back to PNG
        return outputStream.ToArray();
        }
    }
}
