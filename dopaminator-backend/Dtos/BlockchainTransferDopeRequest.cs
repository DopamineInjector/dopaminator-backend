namespace dopaminator_backend.Dtos
{
    public class BlockchainTransferDopeRequest
    {
        public required string sender { get; set; }
        public required string recipent { get; set; }
        public required int amount { get; set; }
    }
}
