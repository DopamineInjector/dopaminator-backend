namespace dopaminator_backend.Dtos
{
    public class CreateAuctionRequest
    {
        public required int tokenId { get; set; }
        public required float price { get; set; }
        public required string description { get; set; }
    }
}
