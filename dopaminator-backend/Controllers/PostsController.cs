using Dopaminator.Models;
using dopaminator_backend.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dopaminator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PostsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Post
        [HttpGet("get")]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .ToListAsync();
            return Ok(posts);
        }

        // GET: api/Post/{id}
        [HttpGet("get/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return NotFound("Post not found.");

            return Ok(post);
        }

        // POST: api/Post
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreatePostRequest body)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized("User not authorized.");

            var post = new Post
            {
                Title = body.Title,
                Content = body.Content,
                UserId = userId.Value
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = post.Id }, post);
        }

        // PUT: api/Post/{id}
        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreatePostRequest updatedPost)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized("User not authorized.");

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound("Post not found.");

            if (post.UserId != userId.Value)
                return Forbid("You are not the owner of this post.");

            post.Title = updatedPost.Title;
            post.Content = updatedPost.Content;

            _context.Posts.Update(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Post/{id}
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized("User not authorized.");

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound("Post not found.");

            if (post.UserId != userId.Value)
                return Forbid("You are not the owner of this post.");

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Helper method to extract the user ID from the claims
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

