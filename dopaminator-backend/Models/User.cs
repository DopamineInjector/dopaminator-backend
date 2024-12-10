using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;

namespace Dopaminator.Models
{
    public class User
    {
        public Guid Id { get; set; }

        [Required]
        public required string Username { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }

        public required float Balance { get; set; }

        public ICollection<Post> Posts { get; set; }

        public ICollection<Auction> Auctions { get; set; } = new List<Auction>();
    }
}
