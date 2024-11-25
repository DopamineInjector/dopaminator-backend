using Dopaminator.Models;

namespace Dopaminator.Dtos
{
    public class GetUserResponse
    {
        public required string Username { get; set; }
        public ICollection<PostResponse> Posts { get; set; }
    }
}
