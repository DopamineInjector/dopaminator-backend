using Dopaminator.Models;

namespace dopaminator_backend.Dtos
{
    public class GetUserResponse
    {
        public required string Username { get; set; }
        public ICollection<Post> Posts { get; set; }
    }
}
