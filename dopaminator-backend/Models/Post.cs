using System.ComponentModel.DataAnnotations;

namespace Dopaminator.Models
{
    public class Post
    {
        public int Id { get; set; }
        [Required]
        public required string Title { get; set; }
        [Required]
        public required string Content { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}
