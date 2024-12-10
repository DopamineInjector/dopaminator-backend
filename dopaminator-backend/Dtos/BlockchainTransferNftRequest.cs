namespace dopaminator_backend.Dtos
{
    public class BlockchainTransferNftRequest
    {
        public required string Sender { get; set; }
        public required string Recipient { get; set; }
        public required int TokenId { get; set; }
    }
}
