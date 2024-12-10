namespace dopaminator_backend.Dtos
{
    public class TransferFundsRequest
    {
        public required float Amount { get; set; }
        public required Guid Recipient { get; set; }
    }
}
