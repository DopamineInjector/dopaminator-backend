using Microsoft.AspNetCore.Mvc;
using Dopaminator.Models;
using Dopaminator.Dtos;
using Microsoft.AspNetCore.Authorization;
using Dopaminator.Services;
using dopaminator_backend.Dtos;

namespace Dopaminator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamblingController : ControllerBase
    {
        private readonly MintableService _mintableService;
        private readonly BlockchainService _blockchainService;
        public GamblingController(
            MintableService mintableService,
            BlockchainService blockchainService
            )
        {
            _mintableService = mintableService;
            _blockchainService = blockchainService;
        }

        [HttpGet("spin")]
        [Authorize]
        public async Task<IActionResult> GetSpin()
        {
            //todo check account balance
            SpinResponse response = new SpinResponse { isWin = new Random().NextDouble() < 0.33 };
            if (response.isWin)
            {
                Mintable? mintedMintable = await _mintableService.Mint();
                if (mintedMintable != null)
                {
                    response.Name = mintedMintable.Name;
                    response.Image = mintedMintable.Image;
                    BlockchainMintNftRequest blockchainMintNftRequest = new BlockchainMintNftRequest
                    {
                        user = GetUserId().ToString(),
                        image = mintedMintable.Image,
                        description = mintedMintable.Name,
                    };
                    await _blockchainService.mintNft(blockchainMintNftRequest);
                }
            }
            return Ok(response);
        }

        private Guid? GetUserId()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
                return null;
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
                return null;
            return new Guid(userIdClaim.Value);
        }
    }
}