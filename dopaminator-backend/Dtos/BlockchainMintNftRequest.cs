namespace dopaminator_backend.Dtos
{
    public class BlockchainMintNftRequest
    {
        public required string user { get; set; }
        public required byte[] image { get; set; }
        public required string description { get; set; }
    }
}
