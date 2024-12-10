namespace dopaminator_backend.Dtos
{
    public class BuyNftRequest
    {
        public required Guid UserId { get; set; }
        public required int TokenId { get; set; }
        public required float Price { get; set; }
    }
}