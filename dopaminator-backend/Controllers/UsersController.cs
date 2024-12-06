using Microsoft.AspNetCore.Mvc;
using Dopaminator.Models;
using Dopaminator.Dtos;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
        public UsersController(AppDbContext context, IPasswordHasher<User> passwordHasher, IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        [HttpPost("signup")]
        public IActionResult SignUp([FromBody] CreateUserRequest request)
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
                Posts = []
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            createUserWallet(user.Id);

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
        public async Task<IActionResult> getUser([FromBody] GetUserRequest request)
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
                Posts = user.Posts.Select(p => new PostResponse
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content
                }).ToList(),
                WalletBalance = null,
            };

            if (user.Id == GetUserId())
            {
                int? walletBalance = await getUserWalletAsync(user.Id);
                if (walletBalance.HasValue)
                {
                    response.WalletBalance = walletBalance;
                }
            }

            return Ok(response);
        }

        [HttpGet("spin")]
        [Authorize]
        public IActionResult GetSpin()
        {
            bool isWin = new Random().NextDouble() < 0.33;
            return Ok(new { isWin });
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

        private async void createUserWallet(Guid userId)
        {
            string url = "http://localhost:8083/api/wallet/" + userId.ToString();

            using HttpClient client = new HttpClient();
            try
            {
                HttpResponseMessage response = await client.PostAsync(url, null);
                response.EnsureSuccessStatusCode(); // Rzuca wyj¹tek, jeœli kod statusu nie jest 2xx

                //getUserWallet(userId);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Wyst¹pi³ b³¹d podczas tworzenia portfela: {e.Message}");
            }
        }

        private async Task<int?> getUserWalletAsync(Guid userId)
        {
            string url = "http://localhost:8083/api/wallet/" + userId.ToString();

            using HttpClient client = new HttpClient();
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Rzuca wyj¹tek, jeœli kod statusu nie jest 2xx

                string responseBody = await response.Content.ReadAsStringAsync(); // Odczyt body jako string

                // Deserializacja JSON
                using JsonDocument doc = JsonDocument.Parse(responseBody);
                JsonElement root = doc.RootElement;

                // Wyci¹gniêcie pola balance
                if (root.TryGetProperty("balance", out JsonElement balanceElement) &&
                    balanceElement.TryGetInt32(out int balance))
                {
                    return balance;
                }

                Console.WriteLine("Pole 'balance' nie zosta³o znalezione w odpowiedzi.");
                return null; // Zwraca null, jeœli pole balance nie istnieje
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Wyst¹pi³ b³¹d podczas szukania portfela: {e.Message}");
                return null;
            }
        }

        private Guid? GetUserId()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
                return null;

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
                return null;

            return Guid.Parse(userIdClaim.Value);
        }
    }

}