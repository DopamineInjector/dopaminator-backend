using Dopaminator.Models;
using Dopaminator.Services;
using dopaminator_backend.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Dopaminator.Dtos;

namespace Dopaminator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ImageService _imageSerice;
        private readonly PostService _postService;

        public PostsController(AppDbContext context, ImageService service, PostService postService)
        {
            _context = context;
            this._imageSerice = service;
            this._postService = postService;
        }

        // GET: api/Post/{id}
        [HttpGet("get/{id}")]
        [Authorize]
        public async Task<IActionResult> Get(Guid id)
        {
            var post = this.GetPostFromDb(id); 
            if (post == null) {
                return NotFound(new {message = "No such post"});
            }
            User? user = this.GetAuthorizedUser();
            if (user == null) {
                return Unauthorized(new {message = "For some reason user is unauthorized"});
            }
            PostResponse? res = this._postService.ParsePostForUser(id, user.Id);
            return Ok(res);
        }

        // POST: api/Post
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreatePostRequest body)
        {
            if(body.Price < 1) {
                return BadRequest(new {message = "Invalid post price"});
            }
            User? user = this.GetAuthorizedUser();
            if(user == null) {
                return Unauthorized(new {message = "For some reason user is unauthorized"});
            }
            byte[] blurred = this._imageSerice.BlurImage(body.Content);
            Post created = new Post {
                Id = Guid.NewGuid(),
                Price = body.Price,
                ImageData = body.Content,
                BlurredImageData = blurred,
                Author = user,
                UserId = user.Id,
                Title = body.Title,
                PurchasedBy = new()
            };
            Console.WriteLine(created.Author.ToString());
            await this._context.Posts.AddAsync(created);
            this._context.SaveChanges();
            return Created();
        }

        // PUT: api/Post/{id}
        [HttpPut("edit/{id}")]
        [Authorize]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePostRequest updatedPost)
        {
            Post? post = this.GetPostFromDb(id);
            if(post == null) {
                return NotFound(new {message = "No such post"});
            }
            User? user = this.GetAuthorizedUser();
            if (user == null) {
                return Unauthorized(new {message = "For some reason user is unauthorized"});
            }
            if(!post.Author.Id.Equals(user.Id)) {
                return StatusCode(403, new {message = "Can not edit post"});
            }
            post.Price = updatedPost.Price;
            this._context.Posts.Update(post);
            this._context.SaveChanges();
            return Created();
        }

        [HttpPut("buy/{id}")]
        [Authorize]
        public async Task<IActionResult> BuyPost(Guid id) {
            Post? post = this.GetPostFromDb(id);
            if(post == null) {
                return NotFound(new {message = "No such post"});
            }
            User? user = this.GetAuthorizedUser();
            if (user == null) {
                return Unauthorized(new {message = "For some reason user is unauthorized"});
            }
            if(this.IsBoughtByAuthUser(post)) {
                return StatusCode(409, new {message = "Post already bought"});
            }
            if(user.Balance < post.Price) {
                return StatusCode(400, new {message = "Cannot afford this post. Maybe stop buying pics on the internet and get an actual job lil bro"});
            }
            User author = this._context.Users.First(p => p.Id.Equals(post.UserId));
            user.Balance -= post.Price;
            author.Balance += post.Price;
            post.PurchasedBy.Add(user.Id);
            this._context.Users.Update(user);
            this._context.Users.Update(author);
            this._context.Posts.Update(post);
            this._context.SaveChanges();
            return Ok(new {message = "bought da post"});
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

        private User? GetAuthorizedUser() {
            Guid? id = this.GetUserId();
            if(id == null) {
                return null;
            }
            return this._context.Users.First(u => u.Id.Equals(id));
        }

        private bool IsBoughtByAuthUser(Post post) {
            var id = this.GetUserId();
            return post.PurchasedBy.Any(p => p.Equals(id));
        }

        private Post? GetPostFromDb(Guid postId) {
            return this._context.Posts.First(p => p.Id.Equals(postId));
        }
    }
}

