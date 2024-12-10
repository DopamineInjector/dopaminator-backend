namespace dopaminator_backend.Dtos
{
    public class BlockchainTransferDopeRequest
    {
        public required string Sender { get; set; }
        public required string Recipient { get; set; }
        public required float Amount { get; set; }
    }
}
