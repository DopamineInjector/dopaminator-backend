using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;

namespace Dopaminator.Models
{
    public class User
    {
        public required Guid Id { get; set; }

        [Required]
        public required string Username { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }

        public string? WalletId { get; set; }

        public ICollection<Post> Posts { get; set; }
    }
}