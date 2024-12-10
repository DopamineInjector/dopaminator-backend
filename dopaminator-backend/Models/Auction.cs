namespace Dopaminator.Models
{
    public class Auction
    {
        public int Id { get; set; }
        public required Guid UserId { get; set; }
        public int MintableId { get; set; }
        public required int TokenId { get; set; }
        public required float Price { get; set; }
        public required string Description { get; set; }
        public User User { get; set; } = null!;
        public Mintable Mintable { get; set; } = null!;
    }
}