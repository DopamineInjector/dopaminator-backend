using Microsoft.AspNetCore.Mvc;
using Dopaminator.Models;
using Dopaminator.Dtos;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

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
                Username = request.Username,
                Email = request.Email,
                Password = _passwordHasher.HashPassword(null, request.Password)
            };
            _context.Users.Add(user);
            _context.SaveChanges();
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
                    { "userId", user.Id }
                }
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            user.JwtToken = tokenString;
            _context.Users.Update(user);
            _context.SaveChanges();

            var response = new LoginResponse
            {
                Username = user.Username,
                Token = tokenString
            };

            return Ok(response);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid user session" });

            var user = _context.Users.FirstOrDefault(u => u.Id == int.Parse(userId));
            if (user == null)
                return NotFound(new { message = "User not found" });

            user.JwtToken = "";
            _context.Users.Update(user);
            _context.SaveChanges();

            return Ok(new { message = "Logged out successfully" });
        }

        [HttpPost("find")]
        public IActionResult findUser([FromBody] FindUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);
            bool exists = (user != null);
            return Ok(new { exists });
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
    }

}