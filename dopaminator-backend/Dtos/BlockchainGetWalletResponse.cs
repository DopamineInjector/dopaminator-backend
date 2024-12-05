using Microsoft.AspNetCore.Http.HttpResults;

namespace dopaminator_backend.Dtos
{
    public class BlockchainGetWalletResponse
    {
        public required string Id { get; set; }
        public required string PublicKey { get; set; }

        public required float Balance { get; set; }
    }
}
