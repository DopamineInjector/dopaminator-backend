using Microsoft.AspNetCore.Mvc;
using Dopaminator.Models;
using Microsoft.AspNetCore.Authorization;
using Dopaminator.Services;
using dopaminator_backend.Dtos;
using Microsoft.EntityFrameworkCore;
using Dopaminator.Dtos;

namespace Dopaminator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlockchainController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly BlockchainService _blockchainService;
        private readonly float TAX_RATE = 0.1F;
        public BlockchainController(
            AppDbContext context,
            BlockchainService blockchainService
            )
        {
            _context = context;
            _blockchainService = blockchainService;
        }

        [HttpGet("balance")]
        [Authorize]
        public async Task<IActionResult> GetBalance()
        {
            var userId = GetUserId();
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            var wallet = await _blockchainService.getUserWallet(userId.ToString());
            var response = new GetBalanceResponse { BlockchainBalance = wallet.Balance, DepositBalance = user.Balance };
            return Ok(response);
        }

        [HttpPost("withdraw")]
        [Authorize]
        public async Task<IActionResult> WithdrawFunds([FromBody] WithdrawFundsRequest request) 
        {
            if (request.Amount < 1) {
                return BadRequest("Imagine trying to cheese the system lil fella");
            }
            var userId = GetUserId();
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if(user.Balance < request.Amount) {
                return BadRequest("You are too broke to withdraw that much lil bro");
            }
            int taxedAmount = (int)((1-this.TAX_RATE) * request.Amount);
            await this._blockchainService.withdrawFundsToUserWallet(userId.ToString(), taxedAmount);
            user.Balance -= request.Amount;
            this._context.Update(user);
            this._context.SaveChanges();
            return Ok();
        }

        [HttpPost("deposit")]
        [Authorize]
        public async Task<IActionResult> DepositFunds([FromBody] WithdrawFundsRequest request) 
        {
            if (request.Amount < 1) {
                return BadRequest("Imagine trying to cheese the system lil fella");
            }
            var userId = GetUserId();
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            // Incredibly based check mechanism
            for (int i = 0; i<5; i++) {
                await this._blockchainService.transferDopeToAdminWallet(userId.ToString(), 1);
            }
            var walletInfo = await this._blockchainService.getUserWallet(userId.ToString());
            if(walletInfo.Balance < request.Amount) {
                return BadRequest(new {message = "You are too broke to withdraw that much lil bro"});
            }
            await this._blockchainService.transferDopeToAdminWallet(userId.ToString(), request.Amount);
            user.Balance += request.Amount;
            this._context.Update(user);
            this._context.SaveChanges();
            // Incredibly based check mechanism
            for (int i = 0; i<5; i++) {
                await this._blockchainService.transferDopeToAdminWallet(userId.ToString(), 1);
            }
            return Ok();
        }


        [HttpPost("transfer")]
        [Authorize]
        public async Task<IActionResult> TransferFunds([FromBody] TransferFundsRequest request) 
        {
            var userId = GetUserId();
            var recipient = _context.Users.FirstOrDefault(u => u.Id == request.Recipient);
            if (recipient == null) {
                return BadRequest("that bro does not exist");
            }
            if (request.Amount < 1) {
                return BadRequest("Imagine trying to cheese the system lil fella");
            }
            var serveRequest = new BlockchainTransferDopeRequest {
                Sender = userId.ToString(),
                Recipient = request.Recipient.ToString(),
                Amount = request.Amount
            };
            await this._blockchainService.transferDope(serveRequest);
            return Ok();
        }

        [HttpPost("sell")]
        [Authorize]
        public async Task<IActionResult> CreateAuction([FromBody] CreateAuctionRequest request)
        {
            var userId = GetUserId();
            var mintable = _context.Mintables.FirstOrDefault(m => m.Name == request.description);
            if(mintable == null){
                return BadRequest();
            }
            var auction = new Auction
            {
                UserId = userId ?? Guid.Empty,
                TokenId = request.tokenId,
                Price = request.price,
                Description = request.description,
                MintableId = mintable.Id,
            };
            _context.Auctions.Add(auction);
            _context.SaveChanges();
            return Ok();
        }

        [HttpPost("buy")]
        [Authorize]
        public async Task<IActionResult> BuyNft([FromBody] BuyNftRequest request)
        {
            var userId = GetUserId();
            var buyer = _context.Users.FirstOrDefault(u => u.Id == userId);
            var seller = _context.Users.FirstOrDefault(u => u.Id == request.UserId);
            if(buyer.Balance < request.Price){
                return Conflict(new { message = "Insufficient balance." });
            }
            buyer.Balance -= request.Price;
            seller.Balance += request.Price;
            _context.Users.Update(buyer);
            _context.Users.Update(seller);
            _context.SaveChanges();
            var transferNftRequest = new BlockchainTransferNftRequest {
                Recipient = userId.ToString(),
                Sender = request.UserId.ToString(),
                TokenId = request.TokenId,
            };
            await _blockchainService.transferNft(transferNftRequest);
            var auctionsToDelete = _context.Auctions
                .Where(a => a.TokenId == request.TokenId)
                .ToList();
            _context.Auctions.RemoveRange(auctionsToDelete);
            _context.SaveChanges();
            return Ok();
        }

        [HttpGet("auctions")]
        [Authorize]
        public IActionResult GetAuctions()
        {
            var result = _context.Auctions
                .Select(auction => new
                    {
                    auction.Id,
                    auction.Description,
                    auction.UserId,
                    auction.Price,
                    auction.Mintable.Image,
                    auction.User.Username,
                    auction.TokenId,
                    })
                .ToList();
            return Ok(result);
        }

        [HttpGet("nfts/{username}")]
        [Authorize]
        public async Task<IActionResult> GetUserNfts([FromRoute] string username)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            return Ok(await _blockchainService.getUserNfts(user.Id.ToString()));
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
