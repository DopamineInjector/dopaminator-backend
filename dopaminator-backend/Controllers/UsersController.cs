using Microsoft.AspNetCore.Mvc;
using Dopaminator.Models;
using Dopaminator.Dtos;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Dopaminator.Services;
using System.ComponentModel;

namespace Dopaminator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly string _imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly BlockchainService _blockchainService;
        public UsersController(
            AppDbContext context,
            IPasswordHasher<User> passwordHasher,
            IConfiguration configuration,
            BlockchainService blockchainService
            )
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _blockchainService = blockchainService;
        }

        [HttpPost("signup")]
        async public Task<IActionResult> SignUp([FromBody] CreateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return Conflict(new { message = "A user with this email address already exists." });
            }
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                Password = _passwordHasher.HashPassword(null, request.Password),
                Balance = 400,
                Posts = []
            };
            var dbUser = _context.Users.Add(user);
            _context.SaveChanges();
            await _blockchainService.createWallet(dbUser.Entity.Id.ToString());
            await _blockchainService.transferDopeToAdminWallet(user.Id.ToString(), 500);
            var loginRequest = new LoginRequest
            {
                Email = request.Email,
                Password = request.Password
            };
            return Login(loginRequest);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Wrong email or password" });
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new { message = "Wrong email or password" });
            }
            Console.WriteLine(user.Id.ToString());
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]);
            Console.WriteLine(key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Claims = new Dictionary<string, object>{
                    { "userId", user.Id.ToString() }
                }
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            var response = new LoginResponse
            {
                Username = user.Username,
                Token = tokenString
            };

            return Ok(response);
        }

        [HttpPost("find")]
        public IActionResult findUser([FromBody] GetUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);
            bool exists = (user != null);
            return Ok(new { exists });
        }

        [HttpPost("get")]
        [Authorize]
        public IActionResult getUser([FromBody] GetUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _context.Users
                .Include(u => u.Posts)
                .FirstOrDefault(u => u.Username == request.Username);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var response = new GetUserResponse
            {
                Username = user.Username,
                Id = user.Id,
                Posts = user.Posts.Select(p => new PostResponse
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content
                }).ToList(),
            };
            return Ok(response);
        }

        [HttpGet("main")]
        public IActionResult GetMainPageImg()
        {
            if (!Directory.Exists(_imagesPath))
            {
                return NotFound("Image directory not found.");
            }

            var files = Directory.GetFiles(_imagesPath, "*.jpg");

            if (files.Length == 0)
            {
                return NotFound("No images found.");
            }

            var randomFile = files[new Random().Next(files.Length)];
            var fileName = Path.GetFileName(randomFile);

            return PhysicalFile(randomFile, "image/jpeg", fileName);
        }
    }
}
