namespace dopaminator_backend.Dtos
{
    public class BlockchainTransferNftRequest
    {
        public required string sender { get; set; }
        public required string recipent { get; set; }
        public required int tokenId { get; set; }
    }
}
