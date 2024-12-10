using System.ComponentModel.DataAnnotations;

namespace Dopaminator.Models
{
    public class Post
    {
        public Guid Id { get; set; }

        [Required]
        public required string Title { get; set; }

        [Required]
        public required byte[] ImageData { get; set; } // Switched to store image direct as binary data.
        [Required]
        public required byte[] BlurredImageData { get; set; } // Blurred version of the image.

        [Required]
        public required float Price {get; set;}

        [Required]
        public required User Author { get; set; }

        // New ting: List of users who copped access to di post.
        public List<User> PurchasedBy { get; set; } = new();
    }
}
