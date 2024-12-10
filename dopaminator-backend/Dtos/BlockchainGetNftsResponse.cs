namespace dopaminator_backend.Dtos
{
    public class BlockchainGetNftsResponse
    {
        public required Nft[] nfts { get; set; }
    }

    public class Nft
    {
        public required string id { get; set; }

        public required int tokenId { get; set; }

        public required string description { get; set; }

        public required string image { get; set; }
    }
}