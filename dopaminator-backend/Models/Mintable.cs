using System.ComponentModel.DataAnnotations;

namespace Dopaminator.Models
{
    public class Mintable
    {
        public int Id { get; set; }
        [Required]
        public required bool Minted { get; set; }
        [Required]
        public required string Name { get; set; }
        [Required]
        public required byte[] Image { get; set;}
    }
}
