using dopaminator_backend.Dtos;

namespace Dopaminator.Services
{

    public class BlockchainService
    {

        private readonly HttpClient _httpClient;
        public BlockchainService(string blockchainUrl)
        {
            var httpHandler = new HttpInterceptor
            {
                InnerHandler = new HttpClientHandler()
            };

            _httpClient = new HttpClient(httpHandler)
            {
                BaseAddress = new Uri(blockchainUrl)
            };
        }

        public async Task createWallet(string userId)
        {
            Console.WriteLine($"REQ: {_httpClient.BaseAddress}/wallet/{userId}");
            using HttpResponseMessage response = await _httpClient.PostAsync($"/api/wallet/{userId}", null);
            Console.WriteLine(response.RequestMessage);
            response.EnsureSuccessStatusCode();
            return;
        }

        public async Task<BlockchainGetWalletResponse> getUserWallet(string userId)
        {
            return await _httpClient.GetFromJsonAsync<BlockchainGetWalletResponse>($"/api/wallet/{userId}");
        }

        public async Task<BlockchainGetNftsResponse> getUserNfts(string userId)
        {
            return await _httpClient.GetFromJsonAsync<BlockchainGetNftsResponse>($"/api/wallet/{userId}/nfts");
        }

        public async Task mintNft(BlockchainMintNftRequest body)
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/nft/mint", body);
            return;
        }

        public async Task transferNft(BlockchainTransferNftRequest body)
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/nft/transfer", body);
            return;
        }

        public async Task transferDope(BlockchainTransferDopeRequest body)
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/transfer", body);
            return;
        }
    }
}