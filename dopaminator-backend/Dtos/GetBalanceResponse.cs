namespace Dopaminator.Dtos
{
    public class GetBalanceResponse
    {
        public required float BlockchainBalance { get; set; }
        public required float DepositBalance { get; set; }
    }
}
